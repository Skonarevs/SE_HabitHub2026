import { useEffect, useState } from 'react';
import { Clock, Bell, AlertCircle } from 'lucide-react';
import {
  getUserReminders,
  getHabitEntries,
  logProgress,
  type ReminderResponseDto,
} from '../../../api/teamsApi';

export const Reminders = () => {
  const [reminders, setReminders] = useState<ReminderResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [loggingId, setLoggingId] = useState<string | null>(null);
  const [currentTime, setCurrentTime] = useState<string>('');

  const loadReminders = async () => {
    try {
      setLoading(true);
      setError(null);

      const data = await getUserReminders();
      const activeReminders = data.filter((r) => r.enabled);

      const todayStr = new Date().toISOString().slice(0, 10);

      const unloggedReminders = (
        await Promise.all(
          activeReminders.map(async (reminder) => {
            try {
              const entries = await getHabitEntries(reminder.habitId);
              const isDoneToday = entries.some(
                (e) =>
                  e.date.slice(0, 10) === todayStr &&
                  (e.status === 'Logged' || e.status === 'Skipped')
              );

              return isDoneToday ? null : reminder;
            } catch (err) {
              console.error(
                `Failed to load entries for ${reminder.habitId}`,
                err
              );
              return reminder;
            }
          })
        )
      ).filter((r): r is ReminderResponseDto => r !== null);

      setReminders(unloggedReminders);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load reminders');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadReminders();
  }, []);

  useEffect(() => {
    const updateTime = () => {
      const now = new Date();
      const hours = now.getHours().toString().padStart(2, '0');
      const minutes = now.getMinutes().toString().padStart(2, '0');
      setCurrentTime(`${hours}:${minutes}`);
    };

    updateTime();
    const interval = setInterval(updateTime, 10000);
    return () => clearInterval(interval);
  }, []);

  const handleLogHabit = async (habitId: string) => {
    try {
      setLoggingId(habitId);

      await logProgress(habitId, {
        status: 'Logged',
        notes: '',
      });

      setReminders((prev) => prev.filter((r) => r.habitId !== habitId));
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to log habit.');
    } finally {
      setLoggingId(null);
    }
  };

  const activeNotifications = reminders.filter((r) => {
    if (!r.reminderTime) return false;
    return currentTime >= r.reminderTime;
  });

  if (loading) {
    return (
      <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 items-center justify-center">
        <p className="text-gray-500 font-medium">
          Checking for notifications...
        </p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 items-center justify-center">
        <AlertCircle className="text-red-500 mb-3" size={40} />
        <p className="text-red-600 font-medium mb-4">{error}</p>
        <button
          onClick={() => void loadReminders()}
          className="px-4 py-2 bg-black text-white rounded-xl text-sm font-semibold hover:bg-gray-800 transition-colors"
        >
          Try Again
        </button>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100">
      <div className="mb-8 border-b border-gray-100 pb-4 flex justify-between items-end">
        <div>
          <h2 className="text-2xl font-bold text-gray-900 flex items-center gap-3">
            Notifications Inbox
          </h2>
          <p className="text-gray-500 mt-1">
            Habits will appear here when it's time to complete them.
          </p>
        </div>
        <div className="text-sm font-bold text-gray-400 bg-gray-50 px-3 py-1.5 rounded-lg">
          Current Time: {currentTime}
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 overflow-y-auto pr-2">
        {activeNotifications.length === 0 ? (
          <div className="col-span-full text-center py-16 bg-gray-50 rounded-2xl border border-gray-100 transition-all">
            <Bell className="mx-auto text-gray-300 mb-3" size={48} />
            <p className="text-gray-500 font-medium text-lg">
              You're all caught up!
            </p>
            <p className="text-gray-400 text-sm mt-1">
              {reminders.length > 0
                ? `You have ${reminders.length} reminder(s) waiting for later today.`
                : 'No pending reminders for today.'}
            </p>
          </div>
        ) : (
          activeNotifications.map((reminder) => (
            <div
              key={reminder.habitId}
              className="flex flex-col justify-between p-6 rounded-2xl border-2 border-blue-100 bg-blue-50/30 hover:border-blue-300 hover:shadow-sm transition-all group animate-in fade-in slide-in-from-bottom-4 duration-500"
            >
              <div>
                <div className="flex justify-between items-start mb-4">
                  <div className="flex items-center gap-2 text-sm font-bold text-blue-600 bg-blue-100 px-3 py-1.5 rounded-lg border border-blue-200">
                    <Clock size={14} />
                    {reminder.reminderTime}
                  </div>
                </div>

                <h3 className="font-bold text-gray-900 text-xl leading-tight mb-2">
                  It's time for: {reminder.habitName}
                </h3>
              </div>

              <div className="flex justify-end mt-6 pt-4 border-t border-blue-100">
                <button
                  onClick={() => handleLogHabit(reminder.habitId)}
                  disabled={loggingId === reminder.habitId}
                  className={`px-5 py-2.5 rounded-xl text-sm font-semibold transition-all shadow-sm ${
                    loggingId === reminder.habitId
                      ? 'bg-gray-400 text-white cursor-not-allowed'
                      : 'bg-blue-600 hover:bg-blue-700 text-white hover:-translate-y-0.5'
                  }`}
                >
                  {loggingId === reminder.habitId
                    ? 'Saving...'
                    : 'Mark as Done'}
                </button>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};
