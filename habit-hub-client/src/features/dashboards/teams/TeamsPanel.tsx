import { useEffect, useState } from 'react';
import { useAuthStore } from '../../../store/useAuthStore';
import { getTeamsInfo } from '../../../api/teamsApi';
import type { TeamInfo } from '../../../types/teamsTypes';
import { Link } from 'lucide-react';
import { NavLink } from 'react-router-dom';

type TeamUI = {
  id: string;
  name: string;
  notes: string;
  chatLink: string;
  inviteCode: string;
  membersLink: string;
  memberCount: number;
};

export const TeamsPanel = () => {
  const { role } = useAuthStore();
  const isCreator = role === 'Creator';

  const [teams, setTeams] = useState<TeamUI[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchTeams = async () => {
      try {
        const data: TeamInfo[] = await getTeamsInfo();

        // 🔁 map backend → UI model
        const mapped: TeamUI[] = data.map((team) => ({
          id: team.teamId,
          name: team.name,
          notes: 'No description', // TODO: replace when backend supports it
          chatLink: `/chat/${team.teamId}`,
          inviteCode: 'N/A', // TODO: replace when backend supports it
          membersLink: `/teams/${team.teamId}/members`,
          memberCount: 0, // TODO: replace when backend supports it
        }));

        setTeams(mapped);
      } catch (err) {
        console.error('Failed to fetch teams:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchTeams();
  }, []);

  const handleLeaveTeam = (id: string) => {
    console.log(`Leaving team: ${id}`);
    setTeams((prev) => prev.filter((team) => team.id !== id));
  };

  const handleDeleteTeam = (id: string, name: string) => {
    if (
      window.confirm(
        `Are you absolutely sure you want to delete "${name}"? This cannot be undone.`
      )
    ) {
      console.log(`Deleting team: ${id}`);
      setTeams((prev) => prev.filter((team) => team.id !== id));
    }
  };

  if (loading) {
    return <div className="p-6 text-gray-500">Loading teams...</div>;
  }

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden">
      {/* Header */}
      <div className="mb-6 flex justify-between items-end">
        <div>
          <h2 className="text-2xl font-bold text-gray-800">
            {isCreator ? 'Managed Teams' : 'Your Teams'}
          </h2>
          <p className="text-gray-500 text-sm">
            {isCreator
              ? 'Workspaces you have created and manage'
              : 'Manage your collaborative workspaces'}
          </p>
        </div>
        <NavLink
          to={isCreator ? 'create-team' : 'join-team'}
          className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-xl text-sm font-medium transition-colors shadow-sm"
        >
          {isCreator ? '+ Create a New Team' : '+ Join a Team'}
        </NavLink>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-left border-separate border-spacing-y-3">
          <thead>
            <tr className="text-gray-400 text-sm uppercase tracking-wider">
              <th className="px-4 py-2">Team Name</th>
              <th className="px-4 py-2">Habits</th>
              <th className="px-4 py-2 text-center">Chat</th>

              {isCreator && (
                <>
                  <th className="px-4 py-2">Invite Code</th>
                  <th className="px-4 py-2">Members</th>
                </>
              )}

              <th className="px-4 py-2 text-right">Action</th>
            </tr>
          </thead>

          <tbody>
            {teams.map((team) => (
              <tr
                key={team.id}
                className="bg-gray-50 hover:bg-gray-100 transition-colors group"
              >
                <td className="px-4 py-4 rounded-l-2xl">
                  <span className="font-semibold text-gray-700">
                    {team.name}
                  </span>
                </td>

                <td className="px-4 py-4">
                  <p className="text-gray-600 text-sm truncate max-w-[200px]">
                    {team.notes}
                  </p>
                </td>

                <td className="px-4 py-4 text-center">
                  <a href={team.chatLink}>💬</a>
                </td>

                {isCreator && (
                  <>
                    <td className="px-4 py-4">{team.inviteCode}</td>
                    <td className="px-4 py-4">
                      <a href={team.membersLink}>
                        Members ({team.memberCount})
                      </a>
                    </td>
                  </>
                )}

                <td className="px-4 py-4 text-right">
                  {isCreator ? (
                    <button
                      onClick={() => handleDeleteTeam(team.id, team.name)}
                      className="text-red-600"
                    >
                      Delete
                    </button>
                  ) : (
                    <button
                      onClick={() => handleLeaveTeam(team.id)}
                      className="text-red-500"
                    >
                      Leave
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {teams.length === 0 && (
          <div className="text-center py-12 text-gray-400 italic">
            {isCreator
              ? "You haven't created any teams yet."
              : "You aren't a member of any teams yet."}
          </div>
        )}
      </div>
    </div>
  );
};
