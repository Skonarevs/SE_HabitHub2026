import { Link } from 'react-router-dom';

export const JoinTeams = () => {
  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100">
      {/* Header with Back link */}
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-2xl font-bold">Join a Team</h2>
        <Link
          to="/main-member/teams-member"
          className="text-sm text-blue-600 hover:text-blue-800 hover:underline"
        >
          ← Back
        </Link>
      </div>

      <p className="mb-6 text-gray-600">
        Enter the invite code to join a team.
      </p>

      {/* Inline label + input */}
      <div className="flex items-center gap-4 mb-6">
        <label className="font-medium text-gray-700 whitespace-nowrap">
          Invite code:
        </label>

        <input
          type="text"
          placeholder="Enter invite code"
          className="flex-1 border border-gray-300 rounded-xl px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500 transition-colors"
        />
      </div>

      <button className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-xl text-sm font-medium transition-colors shadow-sm self-start">
        Join Team
      </button>
    </div>
  );
};
