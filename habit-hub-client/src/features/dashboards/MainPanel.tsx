import { useEffect, useState } from 'react';
import { useAuthStore } from '../../store/useAuthStore';
import {
  getAllMemberHabits,
  getTeamsInfo,
  logProgress,
  getHabitEntries,
  undoLog,
} from '../../api/teamsApi';
import type { TeamHabitInfo } from '../../types/teamsTypes';

interface DashboardHabit extends TeamHabitInfo {
  isLoggedToday?: boolean;
  isSkippedToday?: boolean;
  todayEntryId?: string;
}

export const MainPanel = () => {
  const { userName, role } = useAuthStore();

  const [habits, setHabits] = useState<DashboardHabit[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [loggingId, setLoggingId] = useState<string | null>(null);

  const today = new Date();
  const formattedDate = `${today.getDate()} ${today.toLocaleString('en-US', { month: 'short' })}, ${today.getFullYear()}`;
  const todayStr = today.toISOString().slice(0, 10);

  const loadAllDashboardHabits = async () => {
    try {
      setLoading(true);

      const teams = await getTeamsInfo();
      if (!teams || teams.length === 0) {
        setHabits([]);
        return;
      }

      const habitPromises = teams.map((team) =>
        getAllMemberHabits(team.teamId)
      );
      const habitsFromAllTeams = (await Promise.all(habitPromises)).flat();

      const habitsWithLogStatus = await Promise.all(
        habitsFromAllTeams.map(async (habit) => {
          try {
            const entries = await getHabitEntries(habit.id);
            const todayEntry = entries.find(
              (e) => e.date.slice(0, 10) === todayStr
            );

            return {
              ...habit,
              isLoggedToday: todayEntry?.status === 'Logged',
              isSkippedToday: todayEntry?.status === 'Skipped',
              todayEntryId: todayEntry?.id,
            };
          } catch (err) {
            console.error(`Failed to load entries for habit ${habit.id}`, err);
            return { ...habit, isLoggedToday: false, isSkippedToday: false };
          }
        })
      );

      setHabits(habitsWithLogStatus);
    } catch (err) {
      setError('Could not load habits at this time.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadAllDashboardHabits();
  }, []);

  const handleLogProgress = async (habit: DashboardHabit) => {
    try {
      setLoggingId(habit.id);
      let value: number | undefined = undefined;

      // 1. Ask for quantity if it's a quantitative habit
      if (habit.habitType === 'Quantitative') {
        const input = window.prompt(
          `Enter amount for ${habit.name} (in ${habit.unit || 'units'}):`
        );
        if (!input) return; // User clicked cancel, abort the log completely
        value = Number(input);
        if (isNaN(value) || value <= 0) {
          alert('Please enter a valid positive number.');
          return;
        }
      }

      // 2. ✅ NEW: Ask for an optional note
      // If they click cancel or leave it empty, it just defaults to ""
      const noteInput =
        window.prompt(`Add an optional note for ${habit.name}:`) || '';

      const newEntry = await logProgress(habit.id, {
        status: 'Logged',
        value: value,
        notes: noteInput.trim(), // Send the note to the backend
      });

      setHabits((prevHabits) =>
        prevHabits.map((h) =>
          h.id === habit.id
            ? {
                ...h,
                isLoggedToday: true,
                isSkippedToday: false,
                todayEntryId: newEntry.id,
              }
            : h
        )
      );
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to log progress.');
    } finally {
      setLoggingId(null);
    }
  };

  const handleSkip = async (habit: DashboardHabit) => {
    try {
      setLoggingId(habit.id);

      // ✅ NEW: Ask for an optional reason when skipping
      const skipNote =
        window.prompt(`Reason for skipping ${habit.name} (optional):`) || '';

      const newEntry = await logProgress(habit.id, {
        status: 'Skipped',
        notes: skipNote.trim(), // Send the note to the backend
      });

      setHabits((prevHabits) =>
        prevHabits.map((h) =>
          h.id === habit.id
            ? {
                ...h,
                isLoggedToday: false,
                isSkippedToday: true,
                todayEntryId: newEntry.id,
              }
            : h
        )
      );
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to skip habit.');
    } finally {
      setLoggingId(null);
    }
  };

  const handleUndo = async (habit: DashboardHabit) => {
    if (!habit.todayEntryId) return;
    if (!window.confirm('Are you sure you want to undo this?')) return;

    try {
      setLoggingId(habit.id);

      await undoLog(habit.id, habit.todayEntryId);

      setHabits((prevHabits) =>
        prevHabits.map((h) =>
          h.id === habit.id
            ? {
                ...h,
                isLoggedToday: false,
                isSkippedToday: false,
                todayEntryId: undefined,
              }
            : h
        )
      );
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to undo log.');
    } finally {
      setLoggingId(null);
    }
  };

  return (
    <main className="flex flex-col p-8 justify-between h-full border bg-white border-gray-100 rounded-3xl shadow-sm">
      <header className="flex justify-between items-end mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            {role === 'Creator' ? `Hello, ${userName}!` : `Hey, ${userName}!`}
          </h1>
          <p className="text-gray-400 mt-2">
            Track team progress here. You almost reach a goal!
          </p>
        </div>
        <div className="font-medium text-gray-900">{formattedDate}</div>
      </header>

      <div className="flex-1 overflow-y-auto">
        <h2 className="text-xl font-semibold text-gray-800 mb-4">
          Today's Habits
        </h2>

        {loading ? (
          <div className="text-gray-500 animate-pulse">
            Loading your habits...
          </div>
        ) : error ? (
          <div className="text-red-500 bg-red-50 p-4 rounded-xl">{error}</div>
        ) : habits.length === 0 ? (
          <div className="text-gray-400 italic bg-gray-50 p-8 rounded-2xl text-center">
            No habits found for today.
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {habits.map((habit, index) => {
              const isLogged = habit.isLoggedToday;
              const isSkipped = habit.isSkippedToday;
              const isProcessing = loggingId === habit.id;

              return (
                <div
                  key={`${habit.id}-${index}`}
                  className={`p-5 border rounded-2xl transition-all flex flex-col justify-between ${
                    isLogged
                      ? 'bg-green-50 border-green-200 shadow-sm'
                      : isSkipped
                        ? 'bg-gray-100 border-gray-200 opacity-80'
                        : 'bg-gray-50 border-gray-100 hover:shadow-md'
                  }`}
                >
                  <div>
                    <div className="flex justify-between items-start mb-2">
                      <h3
                        className={`font-semibold text-lg ${
                          isLogged
                            ? 'text-green-900'
                            : isSkipped
                              ? 'text-gray-600 line-through'
                              : 'text-gray-800'
                        }`}
                      >
                        {habit.name}
                      </h3>
                      <span
                        className={`text-xs font-medium px-2 py-1 border rounded-full ${
                          isLogged
                            ? 'bg-green-100 border-green-200 text-green-700'
                            : isSkipped
                              ? 'bg-gray-200 border-gray-300 text-gray-600'
                              : 'bg-white border-gray-200 text-gray-600'
                        }`}
                      >
                        {habit.habitType}
                      </span>
                    </div>
                    <p
                      className={
                        isLogged
                          ? 'text-green-700/70 text-sm'
                          : isSkipped
                            ? 'text-gray-500 text-sm line-through'
                            : 'text-gray-500 text-sm'
                      }
                    >
                      {habit.goal}
                    </p>
                  </div>

                  <div className="mt-6 flex justify-between items-center">
                    <span
                      className={`text-sm font-medium ${
                        isLogged ? 'text-green-600' : 'text-gray-400'
                      }`}
                    >
                      {habit.habitType === 'Quantitative'
                        ? `Unit: ${habit.unit}`
                        : 'Binary'}
                    </span>

                    <div className="flex gap-2">
                      {isLogged || isSkipped ? (
                        <div className="flex items-center gap-3">
                          <span
                            className={`text-sm font-bold ${isLogged ? 'text-green-700' : 'text-gray-500'}`}
                          >
                            {isProcessing
                              ? 'Saving...'
                              : isLogged
                                ? '✓ Logged'
                                : 'Skipped'}
                          </span>
                          <button
                            onClick={() => handleUndo(habit)}
                            disabled={isProcessing}
                            className="text-xs font-semibold text-red-500 hover:text-red-700 bg-red-50 hover:bg-red-100 px-2 py-1.5 rounded-lg transition-colors"
                          >
                            Undo
                          </button>
                        </div>
                      ) : (
                        <>
                          <button
                            onClick={() => handleLogProgress(habit)}
                            disabled={isProcessing}
                            className={`text-sm font-semibold py-2 px-4 rounded-xl transition-all ${
                              isProcessing
                                ? 'bg-gray-400 text-white cursor-not-allowed'
                                : 'bg-black hover:bg-gray-800 text-white'
                            }`}
                          >
                            {isProcessing ? 'Saving...' : 'Log Progress'}
                          </button>

                          <button
                            onClick={() => handleSkip(habit)}
                            disabled={isProcessing}
                            className="bg-white border border-gray-300 hover:bg-gray-100 text-gray-600 px-4 py-2 rounded-xl text-sm font-semibold disabled:opacity-50 transition-all"
                          >
                            Skip
                          </button>
                        </>
                      )}
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        )}
      </div>
    </main>
  );
};
