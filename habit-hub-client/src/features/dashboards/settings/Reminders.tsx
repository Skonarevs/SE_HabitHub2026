import { useEffect, useState } from 'react';
import { Clock, Bell, AlertCircle } from 'lucide-react';
import {
  getUserReminders,
  getHabitEntries,
  type ReminderResponseDto,
} from '../../../api/teamsApi';

export const Reminders = () => {
  const [reminders, setReminders] = useState<ReminderResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [currentTime, setCurrentTime] = useState<string>('');

  const [readIds, setReadIds] = useState<Set<string>>(new Set());
  const [clearedIds, setClearedIds] = useState<Set<string>>(new Set());

  const storageKey = `reminders_state_${new Date().toISOString().slice(0, 10)}`;

  useEffect(() => {
    const stored = localStorage.getItem(storageKey);
    if (stored) {
      const parsed = JSON.parse(stored);
      setReadIds(new Set(parsed.read || []));
      setClearedIds(new Set(parsed.cleared || []));
    }
  }, [storageKey]);

  const syncStorage = (read: Set<string>, cleared: Set<string>) => {
    localStorage.setItem(
      storageKey,
      JSON.stringify({ read: Array.from(read), cleared: Array.from(cleared) })
    );
  };

  const loadReminders = async () => {
    try {
      setLoading(true);
      setError(null);

      const data = await getUserReminders();
      const activeReminders = data;

      const now = new Date();
      const todayStr = new Date(now.getTime() - now.getTimezoneOffset() * 60000)
        .toISOString()
        .slice(0, 10);

      const unloggedReminders = (
        await Promise.all(
          activeReminders.map(async (reminder) => {
            try {
              const entries = await getHabitEntries(reminder.habitId);
              const isDoneToday = entries.some((e) => {
                const entryDateStr = e.date.split('T')[0];
                return (
                  entryDateStr === todayStr &&
                  (e.status === 'Logged' || e.status === 'Skipped')
                );
              });

              if (isDoneToday) return null;
              return reminder;
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
    const interval = setInterval(updateTime, 5000);
    return () => clearInterval(interval);
  }, []);

  const getNormalizedTime = (reminder: any) => {
    const timeStr =
      reminder.defaultReminderTime ||
      reminder.ReminderTime ||
      reminder.reminderTime;

    if (!timeStr) return '26:00';

    const timePart = timeStr.includes('T') ? timeStr.split('T')[1] : timeStr;
    const parts = timePart.split(':');
    const h = (parts[0] || '00').padStart(2, '0');
    const m = (parts[1] || '00').padStart(2, '0');

    return `${h}:${m}`;
  };

  const activeNotifications = reminders.filter((r) => {
    if (clearedIds.has(r.habitId)) return false;

    const normalizedReminderTime = getNormalizedTime(r);

    return currentTime >= normalizedReminderTime;
  });

  const markAsRead = (id: string) => {
    setReadIds((prev) => {
      const next = new Set(prev).add(id);
      syncStorage(next, clearedIds);
      return next;
    });
  };

  const markAllAsRead = () => {
    const allActiveIds = activeNotifications.map((n) => n.habitId);

    setReadIds((prev) => {
      const next = new Set([...prev, ...allActiveIds]);
      syncStorage(next, clearedIds);
      return next;
    });
  };

  const deleteNotification = (id: string) => {
    setClearedIds((prev) => {
      const next = new Set(prev).add(id);
      syncStorage(readIds, next);
      return next;
    });
  };

  const clearAllNotifications = () => {
    const allActiveIds = activeNotifications.map((n) => n.habitId);

    setClearedIds((prev) => {
      const next = new Set([...prev, ...allActiveIds]);
      syncStorage(readIds, next);
      return next;
    });
  };

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
            Reminders Inbox
          </h2>

          <p className="text-gray-500 mt-1">
            Habits will appear here when it's time to complete them.
          </p>
        </div>

        <div className="flex gap-4 items-center">
          <div className="text-sm font-bold text-gray-400 bg-gray-50 px-3 py-1.5 rounded-lg border border-gray-100 mr-2">
            {currentTime}
          </div>

          {activeNotifications.length > 0 && (
            <>
              <button
                onClick={markAllAsRead}
                className="text-sm font-semibold text-gray-500 hover:text-gray-900 transition-colors"
              >
                Mark all as read
              </button>

              <button
                onClick={clearAllNotifications}
                className="text-sm font-semibold text-gray-500 hover:text-gray-900 transition-colors"
              >
                Clear all
              </button>
            </>
          )}
        </div>
      </div>

      <div className="space-y-4 overflow-y-auto pr-2">
        {activeNotifications.length === 0 ? (
          <div className="text-center py-16 bg-gray-50 rounded-2xl border border-gray-100 transition-all">
            <Bell className="mx-auto text-gray-300 mb-3" size={48} />

            <p className="text-gray-500 font-medium text-lg">
              You're all caught up!
            </p>

            <p className="text-gray-400 text-sm mt-1">
              {reminders.filter((r) => !clearedIds.has(r.habitId)).length > 0
                ? `You have reminders waiting for later today.`
                : 'No pending reminders for today.'}
            </p>
          </div>
        ) : (
          activeNotifications.map((reminder) => {
            const displayTime = getNormalizedTime(reminder);
            const isRead = readIds.has(reminder.habitId);

            return (
              <div
                key={reminder.habitId}
                onMouseEnter={() => {
                  if (!isRead) markAsRead(reminder.habitId);
                }}
                className={`group relative flex items-start gap-4 p-5 rounded-2xl border transition-all duration-300 animate-in fade-in slide-in-from-bottom-4 ${
                  isRead
                    ? 'bg-white border-gray-100 opacity-70'
                    : 'bg-gray-50/50 border-gray-200 shadow-sm hover:shadow-md'
                }`}
              >
                <div className="mt-1 flex items-center justify-center w-10 h-10 rounded-full bg-blue-50 border border-blue-100 text-blue-500 shrink-0">
                  <Clock size={20} />
                </div>

                <div className="flex-1 pr-20">
                  <div className="flex justify-between items-start">
                    <h3
                      className={`font-bold ${
                        isRead ? 'text-gray-700' : 'text-gray-900'
                      }`}
                    >
                      {reminder.habitName}
                    </h3>

                    <span className="text-xs font-medium text-gray-400 whitespace-nowrap ml-4">
                      Scheduled: {displayTime}
                    </span>
                  </div>

                  <p className="text-sm text-gray-500 mt-1">
                    {reminder.teamName
                      ? `Team: ${reminder.teamName}`
                      : "It's time to log your progress for this habit."}
                  </p>
                </div>

                {!isRead && (
                  <div className="w-2.5 h-2.5 bg-blue-500 rounded-full mt-2 group-hover:opacity-0 transition-opacity" />
                )}

                {isRead && (
                  <button
                    onClick={() => deleteNotification(reminder.habitId)}
                    className="absolute right-4 top-1/2 -translate-y-1/2 opacity-0 group-hover:opacity-100 transform translate-x-2 group-hover:translate-x-0 bg-white shadow-md border border-gray-100 px-3 py-1.5 rounded-lg text-sm font-semibold text-gray-600 hover:text-red-600 flex items-center gap-2 transition-all duration-200"
                  >
                    Clear
                  </button>
                )}
              </div>
            );
          })
        )}
      </div>
    </div>
  );
};
