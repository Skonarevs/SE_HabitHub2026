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

      navigate('/main-creator/teams-creator');
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
          to="/main-creator/teams-creator"
          className="inline-flex items-center gap-1 text-sm font-medium text-gray-400 hover:text-black transition-colors mb-2"
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
          className="flex-1 border border-gray-300 rounded-xl px-4 py-2 focus:outline-none focus:ring-2 focus:ring-black"
        />
      </div>

      <button
        onClick={handleCreate}
        disabled={loading}
        className="bg-black hover:bg-gray-800 text-white px-5 py-2.5 rounded-xl text-sm font-semibold transition-all shadow-sm hover:shadow hover:-translate-y-0.5"
      >
        {loading ? 'Creating...' : 'Create Team'}
      </button>
    </div>
  );
};


