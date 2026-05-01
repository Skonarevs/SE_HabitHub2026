import { useEffect, useState } from 'react'; //added please
import { useAuthStore } from '../../../store/useAuthStore';
import { Link, useParams } from 'react-router-dom'; //added please
import {
  archiveHabit,
  deleteHabit,
  getActiveHabits,
  getArchivedHabits,
  updateHabit,
} from '../../../api/teamsApi'; //added please
import type { TeamHabitInfo } from '../../../types/teamsTypes'; //added please

export const HabitsList = () => {
  const { role } = useAuthStore();
  const isCreator = role === 'Creator';
  const { teamId } = useParams<{ teamId: string }>(); //added please

  const [habits, setHabits] = useState<TeamHabitInfo[]>([]); //added please
  const [loading, setLoading] = useState(true); //added please
  const [error, setError] = useState<string | null>(null); //added please
  const [showArchived, setShowArchived] = useState(false);
  const [editingHabit, setEditingHabit] = useState<TeamHabitInfo | null>(null);
  const [editName, setEditName] = useState('');
  const [editGoal, setEditGoal] = useState('');
  const [editType, setEditType] = useState<'Binary' | 'Quantitative'>('Binary');
  const [editUnit, setEditUnit] = useState('');
  const [editExpiryDate, setEditExpiryDate] = useState('');
  const [editError, setEditError] = useState<string | null>(null);
  const [editLoading, setEditLoading] = useState(false);

  const loadHabits = async () => { //added please
    if (!teamId) { //added please
      setError('Team id is missing.'); //added please
      setLoading(false); //added please
      return; //added please
    } //added please

    try { //added please
      setLoading(true); //added please
      setError(null); //added please
      const data = showArchived
        ? await getArchivedHabits(teamId)
        : await getActiveHabits(teamId); //added please
      setHabits(data); //added please
    } catch (err) { //added please
      const message = err instanceof Error ? err.message : 'Failed to load habits.'; //added please
      setError(message); //added please
    } finally { //added please
      setLoading(false); //added please
    } //added please
  }; //added please

  useEffect(() => { //added please
    void loadHabits(); //added please
  }, [teamId, showArchived]); //added please

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

  const startEdit = (habit: TeamHabitInfo) => {
    setEditingHabit(habit);
    setEditName(habit.name);
    setEditGoal(habit.goal);
    setEditType(habit.habitType === 'Quantitative' ? 'Quantitative' : 'Binary');
    setEditUnit(habit.unit ?? '');
    setEditExpiryDate(habit.expiryDate ? habit.expiryDate.toISOString().slice(0, 10) : '');
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

      setHabits((prev) => prev.map((h) => (h.id === editingHabit.id ? { ...h, ...updated } : h)));
      cancelEdit();
    } catch (err) {
      setEditError(err instanceof Error ? err.message : 'Failed to update habit.');
    } finally {
      setEditLoading(false);
    }
  };

  if (loading) { //added please
    return ( //added please
      <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden"> //added please
        Loading habits...
      </div> //added please
    ); //added please
  } //added please

  if (error) { //added please
    return ( //added please
      <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden"> //added please
        <p className="text-red-600 mb-4">{error}</p> //added please
        <button //added please
          onClick={() => void loadHabits()} //added please
          className="w-fit bg-black hover:bg-gray-800 text-white px-4 py-2 rounded-xl text-sm font-semibold" //added please
        > //added please
          Retry //added please
        </button> //added please
      </div> //added please
    ); //added please
  } //added please

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
          {isCreator && (
            <button
              onClick={() => setShowArchived((prev) => !prev)}
              className="text-sm font-medium text-gray-500 hover:text-gray-800 transition-colors px-2"
            >
              {showArchived ? 'View Active' : 'View Archive'}
            </button>
          )}

          {/* Conditional New Habit Button */}
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

      {/* Table */}
      <div className="overflow-x-auto">
        {editingHabit && (
          <div className="mb-6 border border-gray-200 rounded-2xl p-4 bg-gray-50">
            <h3 className="text-lg font-semibold text-gray-800 mb-4">Edit Habit</h3>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Name</label>
                <input
                  value={editName}
                  onChange={(e) => setEditName(e.target.value)}
                  className="w-full border border-gray-300 rounded-xl px-3 py-2 text-sm"
                  type="text"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Goal</label>
                <input
                  value={editGoal}
                  onChange={(e) => setEditGoal(e.target.value)}
                  className="w-full border border-gray-300 rounded-xl px-3 py-2 text-sm"
                  type="text"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Type</label>
                <select
                  value={editType}
                  onChange={(e) => setEditType(e.target.value as 'Binary' | 'Quantitative')}
                  className="w-full border border-gray-300 rounded-xl px-3 py-2 text-sm"
                >
                  <option value="Binary">Binary</option>
                  <option value="Quantitative">Quantitative</option>
                </select>
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Unit</label>
                <input
                  value={editUnit}
                  onChange={(e) => setEditUnit(e.target.value)}
                  className="w-full border border-gray-300 rounded-xl px-3 py-2 text-sm"
                  type="text"
                  disabled={editType !== 'Quantitative'}
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Expiry Date</label>
                <input
                  value={editExpiryDate}
                  onChange={(e) => setEditExpiryDate(e.target.value)}
                  className="w-full border border-gray-300 rounded-xl px-3 py-2 text-sm"
                  type="date"
                />
              </div>
            </div>

            {editError && <p className="text-sm text-red-600 mt-3">{editError}</p>}

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
                className="bg-white border border-gray-300 text-gray-700 px-4 py-2 rounded-xl text-sm font-semibold"
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

              {/* Conditional Headers based on Role */}
              {isCreator ? (
                <>
                  <th className="px-4 py-2 font-medium">Type</th>
                  <th className="px-4 py-2 font-medium">End Date</th>
                </>
              ) : (
                <th className="px-4 py-2 font-medium">End Date</th>
              )}

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
                        {habit.habitType}
                      </span>
                    </td>
                    {/* End Date (Creator) */}
                    <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                      <span className="text-gray-500 text-sm">
                        {habit.expiryDate ? habit.expiryDate.toLocaleDateString() : '-'}
                      </span>
                    </td>
                  </>
                ) : (
                  /* Create Date (Member) */
                  <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                    <span className="text-gray-500 text-sm">
                      {habit.expiryDate ? habit.expiryDate.toLocaleDateString() : '-'}
                    </span>
                  </td>
                )}

                {/* Action Column (Creator vs Member) */}
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
