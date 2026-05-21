import { useEffect, useState } from 'react';
import { Clock, Users, Calendar, AlertCircle } from 'lucide-react';
import {
  getUserReminders,
  logProgress,
  type ReminderResponseDto,
} from '../../../api/teamsApi';

export const Reminders = () => {
  const [reminders, setReminders] = useState<ReminderResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [loggingId, setLoggingId] = useState<string | null>(null);

  const loadReminders = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await getUserReminders();

      setReminders(data.filter((r) => r.enabled));
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load reminders');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadReminders();
  }, []);

  const handleLogHabit = async (habitId: string, habitName: string) => {
    try {
      setLoggingId(habitId);

      await logProgress(habitId, {
        status: 'Logged',
        notes: '',
      });

      setReminders((prev) => prev.filter((r) => r.habitId !== habitId));

      alert(`🎉 Logged progress for ${habitName}!`);
    } catch (err) {
      alert(err instanceof Error ? err.message : 'Failed to log habit.');
    } finally {
      setLoggingId(null);
    }
  };

  if (loading) {
    return (
      <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 items-center justify-center">
        <p className="text-gray-500 font-medium">Loading reminders...</p>
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
            Reminders
          </h2>
          <p className="text-gray-500 mt-1">
            Never miss an important habit or task.
          </p>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 overflow-y-auto pr-2">
        {reminders.length === 0 ? (
          <div className="col-span-full text-center py-16 bg-gray-50 rounded-2xl border border-gray-100">
            <Calendar className="mx-auto text-gray-300 mb-3" size={48} />
            <p className="text-gray-500 font-medium">No active reminders.</p>
            <p className="text-gray-400 text-sm mt-1">
              Set reminders in your team habits list.
            </p>
          </div>
        ) : (
          reminders.map((reminder) => (
            <div
              key={reminder.habitId}
              className="flex flex-col justify-between p-6 rounded-2xl border border-gray-200 bg-white hover:border-gray-300 hover:shadow-sm transition-all group"
            >
              <div>
                <div className="flex justify-between items-start mb-4">
                  <div className="flex items-center gap-2 text-sm font-bold text-blue-600 bg-blue-50 px-3 py-1.5 rounded-lg border border-blue-100">
                    <Clock size={14} />
                    {reminder.reminderTime}
                  </div>

                  {reminder.teamName && (
                    <div className="flex items-center gap-1.5 text-xs font-semibold text-purple-600 bg-purple-50 px-2.5 py-1 rounded-md border border-purple-100">
                      <Users size={12} />
                      {reminder.teamName}
                    </div>
                  )}
                </div>

                <h3 className="font-bold text-gray-900 text-xl leading-tight mb-6">
                  {reminder.habitName}
                </h3>
              </div>

              <div className="flex justify-end mt-auto pt-4 border-t border-gray-50">
                <button
                  onClick={() =>
                    handleLogHabit(reminder.habitId, reminder.habitName)
                  }
                  disabled={loggingId === reminder.habitId}
                  className={`px-5 py-2.5 rounded-xl text-sm font-semibold transition-all shadow-sm ${
                    loggingId === reminder.habitId
                      ? 'bg-gray-400 text-white cursor-not-allowed'
                      : 'bg-black hover:bg-gray-800 text-white hover:-translate-y-0.5'
                  }`}
                >
                  {loggingId === reminder.habitId ? 'Saving...' : 'Log Habit'}
                </button>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};
