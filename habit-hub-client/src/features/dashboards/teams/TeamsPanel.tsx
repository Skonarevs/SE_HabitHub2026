import { useEffect, useState } from 'react';
import { useAuthStore } from '../../../store/useAuthStore';
import { deleteTeam, getTeamsInfo, leaveTeam } from '../../../api/teamsApi';
import type { TeamInfo } from '../../../types/teamsTypes';

import { NavLink } from 'react-router-dom';
import { getTeamMembers } from '../../../api/teamsApi';

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
        console.log('TEAMS FROM API:', data);

        // const enriched = await Promise.all(
        //   data.map(async (team) => {
        //     const members = await getTeamMembers(team.teamId);

        //     return {
        //       id: team.teamId,
        //       name: team.name,
        //       notes: 'No description',
        //       chatLink: isCreator
        //         ? `/main-creator/teams-creator/chat/${team.teamId}`
        //         : `/main-member/teams-member/chat/${team.teamId}`,
        //       inviteCode: team.inviteCode ?? 'N/A',
        //       membersLink: isCreator
        //         ? `/main-creator/teams-creator/teams/${team.teamId}/members`
        //         : `/main-member/teams-member/teams/${team.teamId}/members`,
        //       memberCount: members.length,
        //     };
        //   })
        // );
        const mapped: TeamUI[] = data.map((team) => ({
          id: team.teamId,
          name: team.name,
          notes: 'No description',
          chatLink: isCreator
            ? `/main-creator/teams-creator/chat/${team.teamId}`
            : `/main-member/teams-member/chat/${team.teamId}`,
          inviteCode: team.inviteCode ?? 'N/A',
          membersLink: isCreator
            ? `/main-creator/teams-creator/teams/${team.teamId}/members`
            : `/main-member/teams-member/teams/${team.teamId}/members`,
          memberCount: 0, // temporary
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

  const handleLeaveTeam = async (id: string) => {
    try {
      await leaveTeam(id);
      console.log(`Left team: ${id}`);
      setTeams((prev) => prev.filter((team) => team.id !== id));
    } catch (error) {
      console.error('Failed to leave team:', error);
      alert('Failed to leave team');
    }
  };

  const handleDeleteTeam = async (id: string, name: string) => {
    if (
      window.confirm(
        `Are you absolutely sure you want to delete "${name}"? This cannot be undone.`
      )
    ) {
      try {
        await deleteTeam(id);
      } catch (error) {
        console.error('Failed to delete team:', error);
        alert('Failed to delete team');
      }
      console.log(`Deleting team: ${id}`);
      setTeams((prev) => prev.filter((team) => team.id !== id));
    }
  };

  if (loading) {
    return (
      <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden">
        Loading teams...
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden">
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
          to={
            isCreator
              ? '/main-creator/teams-creator/create-team'
              : '/main-member/teams-member/join-team'
          }
          className="bg-black hover:bg-gray-800 text-white px-5 py-2.5 rounded-xl text-sm font-semibold transition-all shadow-sm hover:shadow hover:-translate-y-0.5"
        >
          {isCreator ? '+ Create a New Team' : '+ Join a Team'}
        </NavLink>
      </div>

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

                <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                  <NavLink
                    to={
                      isCreator
                        ? `/main-creator/teams-creator/habits-creator/${team.id}`
                        : `/main-member/teams-member/habits-member/${team.id}`
                    }
                    className="inline-flex items-center justify-center bg-black text-white px-5 py-2.5 rounded-xl text-sm font-bold transition-all duration-300 hover:bg-black hover:shadow-md hover:-translate-y-1 active:translate-y-0"
                  >
                    View Habits
                  </NavLink>
                </td>
                <td className="px-4 py-4 text-center text-black text-md font-medium">
                  <NavLink to={team.chatLink} className="hover:underline">
                    Show Chat
                  </NavLink>
                </td>
                {isCreator && (
                  <>
                    <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                      <div className="inline-flex items-center justify-center bg-pink-50 border border-pink-200 text-pink-700 px-3 py-1.5 rounded-xl shadow-sm">
                        <span className="font-mono text-sm font-bold tracking-wider">
                          {team.inviteCode}
                        </span>
                      </div>
                    </td>

                    <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                      <NavLink
                        to={team.membersLink}
                        className="inline-flex items-center gap-2 text-sm font-semibold text-black px-3 py-1.5 rounded-xl transition-colors"
                      >
                        Members
                        <span className="bg-blue-100 text-black py-0.5 px-2 rounded-full text-xs">
                          {team.memberCount}
                        </span>
                      </NavLink>
                    </td>
                  </>
                )}
                <td className="px-4 py-4 text-right ">
                  {isCreator ? (
                    <button
                      onClick={() => handleDeleteTeam(team.id, team.name)}
                      className="text-red-600 font-medium"
                    >
                      Delete
                    </button>
                  ) : (
                    <button
                      onClick={() => handleLeaveTeam(team.id)}
                      className="text-red-500 font-medium"
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
