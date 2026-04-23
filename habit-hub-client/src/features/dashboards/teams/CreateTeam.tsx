import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { createTeam } from '../../../api/teamsApi';

export const CreateTeam = () => {
  const [teamName, setTeamName] = useState('');
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const handleCreate = async () => {
    if (!teamName.trim()) return;

    try {
      setLoading(true);
      await createTeam(teamName);

      // 👉 redirect after success
      navigate('/teams');
    } catch (err) {
      console.error('Failed to create team', err);
      alert('Failed to create team');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-2xl font-bold">Create a Team</h2>
        <Link
          to="/teams/join"
          className="text-sm text-blue-600 hover:underline"
        >
          ← Back
        </Link>
      </div>

      <p className="mb-6 text-gray-600">
        Create a new team to collaborate with others.
      </p>

      <div className="flex items-center gap-4 mb-6">
        <label className="font-medium text-gray-700 whitespace-nowrap">
          Team Name:
        </label>

        <input
          value={teamName}
          onChange={(e) => setTeamName(e.target.value)}
          type="text"
          placeholder="Enter team name"
          className="flex-1 border border-gray-300 rounded-xl px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <button
        onClick={handleCreate}
        disabled={loading}
        className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-xl text-sm font-medium transition-colors shadow-sm disabled:opacity-50"
      >
        {loading ? 'Creating...' : 'Create Team'}
      </button>
    </div>
  );
};
