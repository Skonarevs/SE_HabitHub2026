import { useState } from 'react';
import { Clock, Plus, Trash2, Calendar } from 'lucide-react';

interface Reminder {
  id: string;
  task: string;
  time: string;
}

const MOCK_REMINDERS: Reminder[] = [
  { id: '1', task: 'Review team performance report', time: 'Today, 14:00' },
  { id: '2', task: 'Drink a glass of water', time: 'Every day, 10:00' },
  { id: '3', task: 'Update billing information', time: 'Tomorrow, 09:00' },
];

export const Reminders = () => {
  const [reminders, setReminders] = useState(MOCK_REMINDERS);

  const deleteReminder = (id: string) => {
    setReminders(reminders.filter((r) => r.id !== id));
  };

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

        <button className="flex items-center gap-2 px-4 py-2 bg-gray-900 hover:bg-gray-800 text-white rounded-xl text-sm font-semibold transition-all">
          <Plus size={18} />
          Add Reminder
        </button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4 overflow-y-auto pr-2">
        {reminders.length === 0 ? (
          <div className="col-span-full text-center py-10">
            <Calendar className="mx-auto text-gray-300 mb-3" size={48} />
            <p className="text-gray-500">No active reminders.</p>
          </div>
        ) : (
          reminders.map((reminder) => (
            <div
              key={reminder.id}
              className="flex flex-col justify-between p-6 rounded-2xl border border-gray-200 bg-white hover:border-gray-300 transition-colors group"
            >
              <div>
                <div className="flex justify-between items-start mb-3">
                  <div className="flex items-center gap-2 text-sm font-bold text-blue-600 bg-blue-50 px-3 py-1 rounded-lg">
                    <Clock size={14} />
                    {reminder.time}
                  </div>
                  <button
                    onClick={() => deleteReminder(reminder.id)}
                    className="text-gray-300 hover:text-red-500 opacity-0 group-hover:opacity-100 transition-all"
                    title="Delete reminder"
                  >
                    <Trash2 size={18} />
                  </button>
                </div>
                <h3 className="font-semibold text-gray-900 text-lg leading-tight">
                  {reminder.task}
                </h3>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
};
