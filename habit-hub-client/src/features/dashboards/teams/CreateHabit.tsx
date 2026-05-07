import { useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { createTeamHabit } from '../../../api/teamsApi';
import type { CreateHabitDto } from '../../../types/teamsTypes';

export const CreateHabit = () => {
  const navigate = useNavigate();
  const { teamId } = useParams<{ teamId: string }>();

  const [name, setName] = useState('');
  const [goal, setGoal] = useState('');
  const [habitType, setHabitType] = useState<'Binary' | 'Quantitative'>(
    'Binary'
  );
  const [unit, setUnit] = useState('');
  const [expiryDate, setExpiryDate] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const habitsPath = `/main-creator/teams-creator/habits-creator/${teamId}`;

  const handleSubmit = async () => {
    if (!teamId) {
      setError('Team id is missing.');
      return;
    }

    if (!name.trim() || !goal.trim()) {
      setError('Name and goal are required.');
      return;
    }

    if (habitType === 'Quantitative' && !unit.trim()) {
      setError('Unit is required for quantitative habits.');
      return;
    }

    try {
      setLoading(true);
      setError(null);

      const dto: CreateHabitDto = {
        name: name.trim(),
        goal: goal.trim(),
        habitType,
        unit: habitType === 'Quantitative' ? unit.trim() : '',
        expiryDate: expiryDate
          ? new Date(`${expiryDate}T23:59:59.999Z`).toISOString()
          : undefined,
      };

      await createTeamHabit(teamId, dto);
      navigate(habitsPath);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create habit.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-2xl font-bold text-gray-800">Create a Habit</h2>
        <Link
          to={habitsPath}
          className="inline-flex items-center gap-1 text-sm font-medium text-gray-400 hover:text-black transition-colors mb-2"
        >
          ← Back
        </Link>
      </div>

      <p className="mb-6 text-gray-600">
        Add a new habit for this team using the fields below.
      </p>

      <div className="space-y-5 max-w-2xl">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Habit Name
          </label>
          <input
            value={name}
            onChange={(e) => setName(e.target.value)}
            type="text"
            placeholder="Enter habit name"
            className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-black"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Goal
          </label>
          <input
            value={goal}
            onChange={(e) => setGoal(e.target.value)}
            type="text"
            placeholder="Describe the goal"
            className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-black"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Habit Type
          </label>
          <select
            value={habitType}
            onChange={(e) =>
              setHabitType(e.target.value as 'Binary' | 'Quantitative')
            }
            className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-black"
          >
            <option value="Binary">Binary</option>
            <option value="Quantitative">Quantitative</option>
          </select>
        </div>

        {habitType === 'Quantitative' && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-2">
              Unit
            </label>
            <input
              value={unit}
              onChange={(e) => setUnit(e.target.value)}
              type="text"
              placeholder="e.g. glasses, km, minutes"
              className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-black"
            />
          </div>
        )}

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">
            Expiry Date
          </label>
          <input
            value={expiryDate}
            onChange={(e) => setExpiryDate(e.target.value)}
            type="date"
            className="w-full border border-gray-300 rounded-xl px-4 py-3 focus:outline-none focus:ring-2 focus:ring-black"
          />
        </div>

        {error && <p className="text-sm text-red-600">{error}</p>}

        <button
          onClick={handleSubmit}
          disabled={loading}
          className="bg-black hover:bg-gray-800 text-white px-5 py-3 rounded-xl text-sm font-semibold transition-colors shadow-sm disabled:opacity-50"
        >
          {loading ? 'Creating...' : 'Create Habit'}
        </button>
      </div>
    </div>
  );
};


