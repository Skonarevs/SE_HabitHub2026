import { useEffect, useState } from 'react';
import { useAuthStore } from '../../../store/useAuthStore';
import { Link, useParams } from 'react-router-dom';
import {
  archiveHabit,
  deleteHabit,
  getActiveHabits,
  getArchivedHabits,
  updateHabit,
  setHabitReminder,
  toggleHabitReminder,
} from '../../../api/teamsApi';
import type { TeamHabitInfo } from '../../../types/teamsTypes';

export const HabitsList = () => {
  const { role } = useAuthStore();
  const isCreator = role === 'Creator';
  const { teamId } = useParams<{ teamId: string }>();

  const [habits, setHabits] = useState<TeamHabitInfo[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [showArchived, setShowArchived] = useState(false);

  // Edit Habit States
  const [editingHabit, setEditingHabit] = useState<TeamHabitInfo | null>(null);
  const [editName, setEditName] = useState('');
  const [editGoal, setEditGoal] = useState('');
  const [editType, setEditType] = useState<'Binary' | 'Quantitative'>('Binary');
  const [editUnit, setEditUnit] = useState('');
  const [editExpiryDate, setEditExpiryDate] = useState('');
  const [editError, setEditError] = useState<string | null>(null);
  const [editLoading, setEditLoading] = useState(false);

  // Reminder States
  const [reminderTimes, setReminderTimes] = useState<Record<string, string>>(
    {}
  );
  const [disabledReminders, setDisabledReminders] = useState<
    Record<string, boolean>
  >({});
  const [editingReminderId, setEditingReminderId] = useState<string | null>(
    null
  );
  const [tempReminderTime, setTempReminderTime] = useState('');

  const loadHabits = async () => {
    if (!teamId) {
      setError('Team id is missing.');
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const data = showArchived
        ? await getArchivedHabits(teamId)
        : await getActiveHabits(teamId);
      setHabits(data);
    } catch (err) {
      const message =
        err instanceof Error ? err.message : 'Failed to load habits.';
      setError(message);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadHabits();
  }, [teamId, showArchived]);

  const handleArchive = async (id: string) => {
    if (!window.confirm('Archive this habit?')) return;
    try {
      await archiveHabit(id);
      setHabits((prev) => prev.filter((h) => h.id !== id));
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to archive habit.');
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Delete this habit?')) return;
    try {
      await deleteHabit(id);
      setHabits((prev) => prev.filter((h) => h.id !== id));
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to delete habit.');
    }
  };

  // --- Reminder Handlers ---
  const startEditingReminder = (habitId: string) => {
    setEditingReminderId(habitId);
    setTempReminderTime(reminderTimes[habitId] || '');
  };

  const saveReminder = async (habitId: string) => {
    if (tempReminderTime) {
      try {
        // 1. Send it to the C# backend!
        await setHabitReminder(habitId, tempReminderTime);

        // 2. If it succeeds, update the UI
        setReminderTimes((prev) => ({ ...prev, [habitId]: tempReminderTime }));
        setEditingReminderId(null);
      } catch (err) {
        alert('Failed to save the reminder to the server.');
      }
    } else {
      // (Optional logic if you want to allow them to DELETE a reminder by clearing the time)
      const newTimes = { ...reminderTimes };
      delete newTimes[habitId];
      setReminderTimes(newTimes);
      setEditingReminderId(null);
    }
  };

  // 2. Update toggleDisableReminder
  const toggleDisableReminder = async (habitId: string) => {
    const currentlyDisabled = disabledReminders[habitId] || false;
    const isNowEnabled = currentlyDisabled; // If it was disabled, we are turning it ON (Enabled = true)

    try {
      // 1. Send the new 'Enabled' status to the backend
      await toggleHabitReminder(habitId, isNowEnabled);

      // 2. If it succeeds, update the UI
      setDisabledReminders((prev) => ({ ...prev, [habitId]: !isNowEnabled }));
    } catch (err) {
      alert('Failed to update reminder status.');
    }
  };

  // --- Edit Handlers ---
  const startEdit = (habit: TeamHabitInfo) => {
    setEditingHabit(habit);
    setEditName(habit.name);
    setEditGoal(habit.goal);
    setEditType(habit.habitType === 'Quantitative' ? 'Quantitative' : 'Binary');
    setEditUnit(habit.unit ?? '');
    setEditExpiryDate(
      habit.expiryDate ? habit.expiryDate.toISOString().slice(0, 10) : ''
    );
    setEditError(null);
  };

  const cancelEdit = () => {
    setEditingHabit(null);
    setEditName('');
    setEditGoal('');
    setEditType('Binary');
    setEditUnit('');
    setEditExpiryDate('');
    setEditError(null);
    setEditLoading(false);
  };

  const saveEdit = async () => {
    if (!editingHabit) return;

    if (!editName.trim() || !editGoal.trim()) {
      setEditError('Name and goal are required.');
      return;
    }

    if (editType === 'Quantitative' && !editUnit.trim()) {
      setEditError('Unit is required for quantitative habits.');
      return;
    }

    try {
      setEditLoading(true);
      setEditError(null);

      const updated = await updateHabit(editingHabit.id, {
        name: editName.trim(),
        goal: editGoal.trim(),
        habitType: editType,
        unit: editType === 'Quantitative' ? editUnit.trim() : '',
        expiryDate: editExpiryDate
          ? new Date(`${editExpiryDate}T23:59:59.999Z`).toISOString()
          : undefined,
      });

      setHabits((prev) =>
        prev.map((h) => (h.id === editingHabit.id ? { ...h, ...updated } : h))
      );
      cancelEdit();
    } catch (err) {
      setEditError(
        err instanceof Error ? err.message : 'Failed to update habit.'
      );
    } finally {
      setEditLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden">
        Loading habits...
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden">
        <p className="text-red-600 mb-4">{error}</p>
        <button
          onClick={() => void loadHabits()}
          className="w-fit bg-black hover:bg-gray-800 text-white px-4 py-2 rounded-xl text-sm font-semibold"
        >
          Retry
        </button>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden">
      {/* Header */}
      <div className="mb-8 flex flex-col sm:flex-row sm:items-end sm:justify-between gap-4">
        <div className="flex flex-col items-start">
          <Link
            to={
              isCreator
                ? '/main-creator/teams-creator'
                : '/main-member/teams-member'
            }
            className="inline-flex items-center gap-1 text-sm font-medium text-gray-400 hover:text-black transition-colors mb-2"
          >
            <span>←</span> Back to Teams
          </Link>

          <h2 className="text-2xl font-bold text-gray-800">
            {isCreator ? 'Manage Team Habits' : 'Your Habits'}
          </h2>
          <p className="text-gray-500 text-sm mt-1">
            {isCreator
              ? 'Configure and track routines for your team'
              : 'Track your daily routines and progress'}
          </p>
        </div>

        <div className="flex items-center gap-4">
          {isCreator && (
            <button
              onClick={() => setShowArchived((prev) => !prev)}
              className="text-sm font-medium text-gray-500 hover:text-gray-800 transition-colors px-2"
            >
              {showArchived ? 'View Active' : 'View Archive'}
            </button>
          )}

          {isCreator && (
            <Link
              to={`/main-creator/teams-creator/habits-creator/${teamId}/create`}
              className="bg-black hover:bg-gray-800 text-white px-5 py-2.5 rounded-xl text-sm font-semibold transition-all shadow-sm hover:shadow hover:-translate-y-0.5"
            >
              + New Habit
            </Link>
          )}
        </div>
      </div>

      {/* Table & Edit Form Area */}
      <div className="overflow-x-auto">
        {/* EDIT HABIT FORM */}
        {editingHabit && (
          <div className="mb-6 border border-gray-200 rounded-2xl p-4 bg-gray-50">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">
              Edit Habit
            </h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Name
                </label>
                <input
                  value={editName}
                  onChange={(e) => setEditName(e.target.value)}
                  className="w-full border border-gray-300 rounded-xl px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-black"
                  type="text"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Goal
                </label>
                <input
                  value={editGoal}
                  onChange={(e) => setEditGoal(e.target.value)}
                  className="w-full border border-gray-300 rounded-xl px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-black"
                  type="text"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Type
                </label>
                <select
                  value={editType}
                  onChange={(e) =>
                    setEditType(e.target.value as 'Binary' | 'Quantitative')
                  }
                  className="w-full border border-gray-300 rounded-xl px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-black"
                >
                  <option value="Binary">Binary</option>
                  <option value="Quantitative">Quantitative</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Unit
                </label>
                <input
                  value={editUnit}
                  onChange={(e) => setEditUnit(e.target.value)}
                  className="w-full border border-gray-300 rounded-xl px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-black disabled:bg-gray-100 disabled:text-gray-400"
                  type="text"
                  disabled={editType !== 'Quantitative'}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                  Expiry Date
                </label>
                <input
                  value={editExpiryDate}
                  onChange={(e) => setEditExpiryDate(e.target.value)}
                  className="w-full border border-gray-300 rounded-xl px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-black"
                  type="date"
                />
              </div>
            </div>

            {editError && (
              <p className="text-sm text-red-600 mt-3">{editError}</p>
            )}

            <div className="flex gap-3 mt-4">
              <button
                onClick={() => void saveEdit()}
                disabled={editLoading}
                className="bg-black hover:bg-gray-800 text-white px-4 py-2 rounded-xl text-sm font-semibold disabled:opacity-50"
              >
                {editLoading ? 'Saving...' : 'Save'}
              </button>
              <button
                onClick={cancelEdit}
                disabled={editLoading}
                className="bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 px-4 py-2 rounded-xl text-sm font-semibold transition-colors"
              >
                Cancel
              </button>
            </div>
          </div>
        )}

        <table className="w-full text-left border-separate border-spacing-y-3">
          <thead>
            <tr className="text-gray-400 text-sm uppercase tracking-wider">
              <th className="px-4 py-2 font-medium">Name</th>
              <th className="px-4 py-2 font-medium">Goal</th>

              {isCreator ? (
                <>
                  <th className="px-4 py-2 font-medium">Type</th>
                  <th className="px-4 py-2 font-medium">End Date</th>
                </>
              ) : (
                <th className="px-4 py-2 font-medium">End Date</th>
              )}
              <th className="px-4 py-2 font-medium text-right w-48">
                Reminder
              </th>
              <th className="px-4 py-2 font-medium text-right">Leaderboard</th>

              <th className="px-4 py-2 font-medium text-right">
                {isCreator ? 'Manage' : 'Progress'}
              </th>
            </tr>
          </thead>

          <tbody>
            {habits.map((habit) => (
              <tr
                key={habit.id}
                className="bg-gray-50 hover:bg-gray-100 transition-colors group"
              >
                <td className="px-4 py-4 rounded-l-2xl border-y border-l border-transparent group-hover:border-gray-200">
                  <span className="font-semibold text-gray-700">
                    {habit.name}
                  </span>
                </td>

                <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                  <span className="text-gray-600 text-sm">{habit.goal}</span>
                </td>

                {isCreator ? (
                  <>
                    <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                        {habit.habitType}
                      </span>
                    </td>
                    <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                      <span className="text-gray-500 text-sm">
                        {habit.expiryDate
                          ? habit.expiryDate.toLocaleDateString()
                          : '-'}
                      </span>
                    </td>
                  </>
                ) : (
                  <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                    <span className="text-gray-500 text-sm">
                      {habit.expiryDate
                        ? habit.expiryDate.toLocaleDateString()
                        : '-'}
                    </span>
                  </td>
                )}

                {/* --- REMINDER COLUMN --- */}
                <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200 text-right">
                  {isCreator ? (
                    // CREATOR VIEW
                    editingReminderId === habit.id ? (
                      // 1. Inline edit mode for Creator
                      <div className="flex items-center justify-end gap-2">
                        <input
                          type="time"
                          value={tempReminderTime}
                          onChange={(e) => setTempReminderTime(e.target.value)}
                          className="border border-gray-300 rounded-lg px-2 py-1 text-sm w-24 focus:outline-none focus:ring-1 focus:ring-black bg-white"
                        />
                        <button
                          onClick={() => saveReminder(habit.id)}
                          className="text-green-600 hover:text-green-800 text-sm font-semibold transition-colors"
                        >
                          Save
                        </button>
                        <button
                          onClick={() => setEditingReminderId(null)}
                          className="text-gray-500 hover:text-gray-800 text-sm font-semibold transition-colors"
                        >
                          Cancel
                        </button>
                      </div>
                    ) : reminderTimes[habit.id] ? (
                      // 2. Reminder is set, show button to edit
                      <button
                        className="text-sm text-blue-600 bg-blue-50 hover:bg-blue-100 font-semibold px-3 py-1.5 rounded-lg transition-colors border border-blue-100"
                        onClick={() => startEditingReminder(habit.id)}
                      >
                        {reminderTimes[habit.id]}
                      </button>
                    ) : (
                      // 3. No reminder set, show "Set Reminder" link
                      <button
                        className="text-sm text-gray-500 hover:text-gray-800 font-medium px-2 py-1 transition-colors"
                        onClick={() => startEditingReminder(habit.id)}
                      >
                        Set Reminder
                      </button>
                    )
                  ) : (
                    // MEMBER VIEW
                    <div className="flex items-center justify-end gap-3">
                      {reminderTimes[habit.id] ? (
                        <>
                          {/* Member sees time (crossed out if disabled) */}
                          <span
                            className={`text-sm font-semibold transition-all ${
                              disabledReminders[habit.id]
                                ? 'text-gray-400 line-through'
                                : 'text-gray-700'
                            }`}
                          >
                            ⏰ {reminderTimes[habit.id]}
                          </span>
                          {/* Member Disable/Enable Toggle */}
                          <button
                            onClick={() => toggleDisableReminder(habit.id)}
                            className={`text-sm font-medium px-3 py-1.5 rounded-lg transition-colors border ${
                              disabledReminders[habit.id]
                                ? 'text-gray-500 bg-gray-100 border-gray-200 hover:bg-gray-200'
                                : 'text-pink-600 bg-pink-50 border-pink-100 hover:bg-pink-100'
                            }`}
                          >
                            {disabledReminders[habit.id] ? 'Enable' : 'Disable'}
                          </button>
                        </>
                      ) : (
                        <span className="text-gray-400 text-sm">--</span>
                      )}
                    </div>
                  )}
                </td>

                <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200 text-right">
                  <button className="text-sm text-gray-500 hover:text-gray-800 font-medium px-2 py-1 transition-colors">
                    View Leaderboard
                  </button>
                </td>

                <td className="px-4 py-4 text-right rounded-r-2xl border-y border-r border-transparent group-hover:border-gray-200">
                  {isCreator ? (
                    <div className="flex items-center justify-end gap-2">
                      <button
                        onClick={() => startEdit(habit)}
                        className="text-gray-500 hover:text-black text-sm font-medium px-2 py-1 transition-colors"
                      >
                        Edit
                      </button>
                      {!showArchived && (
                        <button
                          onClick={() => void handleArchive(habit.id)}
                          className="text-gray-500 hover:text-black text-sm font-medium px-2 py-1 transition-colors"
                        >
                          Archive
                        </button>
                      )}
                      <button
                        onClick={() => void handleDelete(habit.id)}
                        className="text-gray-500 hover:text-red-600 text-sm font-medium px-2 py-1 transition-colors"
                      >
                        Delete
                      </button>
                    </div>
                  ) : (
                    <div className="flex flex-col items-end">
                      <button className="text-pink-400 bg-pink-50 text-sm font-medium px-3 py-1.5 rounded-lg transition-colors border border-pink-100">
                        View
                      </button>
                      <span className="text-xs text-gray-400 mt-1">
                        Habit id: {habit.id}
                      </span>
                    </div>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {habits.length === 0 && (
          <div className="text-center py-12 text-gray-400 italic">
            No habits found.
          </div>
        )}
      </div>
    </div>
  );
};
