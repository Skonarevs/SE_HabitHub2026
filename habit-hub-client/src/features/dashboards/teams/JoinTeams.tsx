import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { joinTeam } from '../../../api/teamsApi';

export const JoinTeams = () => {
  const navigate = useNavigate();

  const [inviteCode, setInviteCode] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleJoinTeam = async () => {
    if (!inviteCode.trim()) {
      setError('Please enter an invite code.');
      return;
    }

    try {
      setLoading(true);
      setError(null);

      await joinTeam({ code: inviteCode.trim() });

      navigate('/main-member/teams-member');
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Failed to join team. Please check the code and try again.'
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-2xl font-bold">Join a Team</h2>
        <Link
          to="/main-member/teams-member"
          className="inline-flex items-center gap-1 text-sm font-medium text-gray-400 hover:text-black transition-colors mb-2"
        >
          ← Back
        </Link>
      </div>

      <p className="mb-6 text-gray-600">
        Enter the invite code to join a team.
      </p>

      {error && (
        <div className="mb-4 p-3 bg-red-50 border border-red-200 text-red-600 rounded-xl text-sm font-medium">
          {error}
        </div>
      )}

      <div className="flex items-center gap-4 mb-6">
        <label className="font-medium text-gray-700 whitespace-nowrap">
          Invite code:
        </label>

        <input
          type="text"
          value={inviteCode}
          onChange={(e) => setInviteCode(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter') void handleJoinTeam();
          }}
          placeholder="Enter invite code"
          className="flex-1 border border-gray-300 rounded-xl px-4 py-2 focus:outline-none focus:ring-2 focus:ring-black transition-colors"
          disabled={loading}
        />
      </div>

      <button
        className="w-fit bg-black hover:bg-gray-800 text-white px-5 py-2.5 rounded-xl text-sm font-semibold transition-all shadow-sm hover:shadow hover:-translate-y-0.5 disabled:opacity-50 disabled:hover:translate-y-0 disabled:cursor-not-allowed"
        onClick={() => void handleJoinTeam()}
        disabled={loading || !inviteCode.trim()}
      >
        {loading ? 'Joining...' : 'Join Team'}
      </button>
    </div>
  );
};
