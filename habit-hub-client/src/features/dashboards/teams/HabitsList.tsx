import { useState } from 'react';
import { useAuthStore } from '../../../store/useAuthStore';
import { Link } from 'react-router-dom';

type HabitUI = {
  id: string;
  name: string;
  goal: string;
  reminderTime: string;
  reminderEnabled: boolean;
  leaderboardRank: number;

  endDate?: string;
  lastProgressDate?: string;

  type?: string;
};

const mockHabits: HabitUI[] = [
  {
    id: '1',
    name: 'Morning Hydration',
    goal: 'Drink 500ml water',
    reminderTime: '07:00 AM',
    reminderEnabled: true,
    leaderboardRank: 2,
    endDate: 'May 12, 2026',
    lastProgressDate: 'Today, 08:15 AM',
    type: 'ml',
  },
  {
    id: '2',
    name: 'Code Review Hour',
    goal: 'Review 3 PRs',
    reminderTime: '02:00 PM',
    reminderEnabled: false,
    leaderboardRank: 5,
    endDate: 'May 15, 2026',
    lastProgressDate: 'Yesterday',
    type: 'count',
  },
];

export const HabitsList = () => {
  const { role } = useAuthStore();
  const isCreator = role === 'Creator';

  const [habits, setHabits] = useState<HabitUI[]>(mockHabits);

  const toggleReminder = (id: string) => {
    setHabits(
      habits.map((h) =>
        h.id === id ? { ...h, reminderEnabled: !h.reminderEnabled } : h
      )
    );
  };

  const handleDelete = (id: string) => {
    if (window.confirm('Delete this habit?')) {
      setHabits(habits.filter((h) => h.id !== id));
    }
  };

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden">
      {/* Header */}
      <div className="mb-8 flex flex-col sm:flex-row sm:items-end sm:justify-between gap-4">
        {/* Left Side: Back Link, Title, and Subtitle */}
        <div className="flex flex-col items-start">
          {/* Breadcrumb Back Link */}
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

        {/* Right Side: Actions */}
        <div className="flex items-center gap-4">
          {/* Archive Link/Button */}
          <button className="text-sm font-medium text-gray-500 hover:text-gray-800 transition-colors px-2">
            View Archive
          </button>

          {/* Conditional New Habit Button */}
          {isCreator && (
            <button className="bg-black hover:bg-gray-800 text-white px-5 py-2.5 rounded-xl text-sm font-semibold transition-all shadow-sm hover:shadow hover:-translate-y-0.5">
              + New Habit
            </button>
          )}
        </div>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-left border-separate border-spacing-y-3">
          <thead>
            <tr className="text-gray-400 text-sm uppercase tracking-wider">
              <th className="px-4 py-2 font-medium">Name</th>
              <th className="px-4 py-2 font-medium">Goal</th>

              {/* Conditional Headers based on Role */}
              {isCreator ? (
                <>
                  <th className="px-4 py-2 font-medium">Type</th>
                  <th className="px-4 py-2 font-medium">End Date</th>
                </>
              ) : (
                <th className="px-4 py-2 font-medium">End Date</th>
              )}

              <th className="px-4 py-2 font-medium">Reminder</th>
              <th className="px-4 py-2 font-medium text-center">LeaderBoard</th>

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
                {/* Name */}
                <td className="px-4 py-4 rounded-l-2xl border-y border-l border-transparent group-hover:border-gray-200">
                  <span className="font-semibold text-gray-700">
                    {habit.name}
                  </span>
                </td>

                {/* Goal */}
                <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                  <span className="text-gray-600 text-sm">{habit.goal}</span>
                </td>

                {/* Conditional Fields: Creator vs Member */}
                {isCreator ? (
                  <>
                    {/* Type (Creator) */}
                    <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                      <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-purple-100 text-purple-800">
                        {habit.type}
                      </span>
                    </td>
                    {/* End Date (Creator) */}
                    <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                      <span className="text-gray-500 text-sm">
                        {habit.endDate}
                      </span>
                    </td>
                  </>
                ) : (
                  /* Create Date (Member) */
                  <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                    <span className="text-gray-500 text-sm">
                      {habit.endDate}
                    </span>
                  </td>
                )}

                {/* Reminder (Both) */}
                <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                  <div className="flex items-center gap-3">
                    <span className="text-sm font-mono text-gray-600">
                      {habit.reminderTime}
                    </span>
                    <button
                      onClick={() => toggleReminder(habit.id)}
                      className={`relative inline-flex h-5 w-9 items-center rounded-full transition-colors ${habit.reminderEnabled ? 'bg-black' : 'bg-gray-300'}`}
                    >
                      <span
                        className={`inline-block h-3 w-3 transform rounded-full bg-white transition-transform ${habit.reminderEnabled ? 'translate-x-5' : 'translate-x-1'}`}
                      />
                    </button>
                  </div>
                </td>

                {/* Leaderboard (Both) */}
                <td className="px-4 py-4 text-center border-y border-transparent group-hover:border-gray-200">
                  <Link
                    to={`/leaderboard/${habit.id}`}
                    className="inline-flex items-center justify-center w-8 h-8 rounded-full bg-yellow-50 text-yellow-600 hover:bg-yellow-100 font-bold text-sm transition-colors"
                  >
                    #{habit.leaderboardRank}
                  </Link>
                </td>

                {/* Action Column (Creator vs Member) */}
                <td className="px-4 py-4 text-right rounded-r-2xl border-y border-r border-transparent group-hover:border-gray-200">
                  {isCreator ? (
                    <div className="flex items-center justify-end gap-2">
                      <button className="text-gray-500 hover:text-black text-sm font-medium px-2 py-1 transition-colors">
                        Edit
                      </button>
                      <button className="text-gray-500 hover:text-black text-sm font-medium px-2 py-1 transition-colors">
                        Archive
                      </button>
                      <button
                        onClick={() => handleDelete(habit.id)}
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
                        Last: {habit.lastProgressDate}
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
