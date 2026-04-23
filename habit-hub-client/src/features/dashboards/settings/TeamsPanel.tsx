import { useState } from 'react';
import { useAuthStore } from '../../../store/useAuthStore';
// import { getActiveSessions } from '../../../api/sessionApi';
// import type { SessionInfo } from '../../auth/types';

export const TeamsPanel = () => {
  // Grab the role from your store, just like in the Sidebar
  const { role } = useAuthStore();
  const isCreator = role === 'Creator';

  // Unified mock data containing fields for both roles
  const [teams, setTeams] = useState([
    {
      id: '1',
      name: 'Design Alpha',
      notes: 'Focusing on Q2 UI Refresh',
      chatLink: '/chat/1',
      inviteCode: 'ALPH-X9Q2',
      membersLink: '/teams/1/members',
      memberCount: 8,
    },
    {
      id: '2',
      name: 'Engine Room',
      notes: 'Backend optimization and API scaling',
      chatLink: '/chat/2',
      inviteCode: 'ENGI-B4M1',
      membersLink: '/teams/2/members',
      memberCount: 14,
    },
  ]);

  const handleLeaveTeam = (id: string) => {
    console.log(`Leaving team: ${id}`);
    setTeams(teams.filter((team) => team.id !== id));
  };

  const handleDeleteTeam = (id: string, name: string) => {
    if (
      window.confirm(
        `Are you absolutely sure you want to delete "${name}"? This cannot be undone.`
      )
    ) {
      console.log(`Deleting team: ${id}`);
      setTeams(teams.filter((team) => team.id !== id));
    }
  };

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden">
      {/* Header Section */}
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
        {isCreator ? (
          <button className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-xl text-sm font-medium transition-colors shadow-sm">
            + Create a New Team
          </button>
        ) : (
          <button className="bg-blue-600 hover:bg-blue-700 text-white px-4 py-2 rounded-xl text-sm font-medium transition-colors shadow-sm">
            + Join a Team
          </button>
        )}
      </div>

      {/* Table Section */}
      <div className="overflow-x-auto">
        <table className="w-full text-left border-separate border-spacing-y-3">
          <thead>
            <tr className="text-gray-400 text-sm uppercase tracking-wider">
              <th className="px-4 py-2 font-medium">Team Name</th>
              <th className="px-4 py-2 font-medium">Habits</th>
              <th className="px-4 py-2 font-medium text-center">Chat</th>

              {isCreator && (
                <>
                  <th className="px-4 py-2 font-medium">Invite Code</th>

                  <th className="px-4 py-2 font-medium">Members</th>
                </>
              )}

              <th className="px-4 py-2 font-medium text-right">Action</th>
            </tr>
          </thead>
          <tbody>
            {teams.map((team) => (
              <tr
                key={team.id}
                className="bg-gray-50 hover:bg-gray-100 transition-colors group"
              >
                {/* Team Name */}
                <td className="px-4 py-4 rounded-l-2xl border-y border-l border-transparent group-hover:border-gray-200">
                  <span className="font-semibold text-gray-700">
                    {team.name}
                  </span>
                </td>

                {/* Notes */}
                <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                  <p
                    className="text-gray-600 text-sm truncate max-w-[200px]"
                    title={team.notes}
                  >
                    {team.notes}
                  </p>
                </td>

                <td className="px-4 py-4 text-center border-y border-transparent group-hover:border-gray-200">
                  <a
                    href={team.chatLink}
                    className="inline-flex items-center justify-center p-2 rounded-full bg-blue-50 text-blue-600 hover:bg-blue-600 hover:text-white transition-all"
                  >
                    <svg
                      xmlns="http://www.w3.org/2000/svg"
                      className="h-5 w-5"
                      viewBox="0 0 20 20"
                      fill="currentColor"
                    >
                      <path
                        fillRule="evenodd"
                        d="M18 10c0 3.866-3.582 7-8 7a8.841 8.841 0 01-4.083-.98L2 17l1.338-3.123C2.493 12.767 2 11.434 2 10c0-3.866 3.582-7 8-7s8 3.134 8 7zM7 9H5v2h2V9zm8 0h-2v2h2V9zM9 9h2v2H9V9z"
                        clipRule="evenodd"
                      />
                    </svg>
                  </a>
                </td>

                {/* Conditional Cells based on Role */}
                {isCreator && (
                  <>
                    {/* Invite Code */}
                    <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                      <div className="inline-flex items-center gap-2 bg-white border border-gray-200 px-3 py-1.5 rounded-lg shadow-sm">
                        <span className="font-mono text-xs font-bold tracking-wider text-gray-600">
                          {team.inviteCode}
                        </span>
                      </div>

                      {/* Added ml-3 here for margin-left */}
                      <button className="ml-3 bg-gray-200 hover:bg-gray-300 text-gray-700 px-4 py-2 rounded-xl text-sm font-medium transition-colors shadow-sm">
                        New Code
                      </button>
                    </td>
                    {/* <td className="px-4 py-2 font-medium">
                      <button className="bg-gray-200 hover:bg-gray-300 text-gray-700 px-4 py-2 rounded-xl text-sm font-medium transition-colors shadow-sm">
                        New Code
                      </button>
                    </td> */}

                    {/* Members Link */}
                    <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                      <a
                        href={team.membersLink}
                        className="inline-flex items-center gap-1 text-sm font-medium text-blue-600 hover:text-blue-800 hover:underline transition-colors"
                      >
                        View Members
                        <span className="bg-blue-100 text-blue-700 py-0.5 px-2 rounded-full text-xs no-underline">
                          {team.memberCount}
                        </span>
                      </a>
                    </td>
                  </>
                )}

                {
                  //: (
                  //   /* Chat Link for Regular Members */
                  //   <td className="px-4 py-4 text-center border-y border-transparent group-hover:border-gray-200">
                  //     <a
                  //       href={team.chatLink}
                  //       className="inline-flex items-center justify-center p-2 rounded-full bg-blue-50 text-blue-600 hover:bg-blue-600 hover:text-white transition-all"
                  //     >
                  //       <svg
                  //         xmlns="http://www.w3.org/2000/svg"
                  //         className="h-5 w-5"
                  //         viewBox="0 0 20 20"
                  //         fill="currentColor"
                  //       >
                  //         <path
                  //           fillRule="evenodd"
                  //           d="M18 10c0 3.866-3.582 7-8 7a8.841 8.841 0 01-4.083-.98L2 17l1.338-3.123C2.493 12.767 2 11.434 2 10c0-3.866 3.582-7 8-7s8 3.134 8 7zM7 9H5v2h2V9zm8 0h-2v2h2V9zM9 9h2v2H9V9z"
                  //           clipRule="evenodd"
                  //         />
                  //       </svg>
                  //     </a>
                  //   </td>
                  // )
                }
                {/* Conditional Action Button */}
                <td className="px-4 py-4 text-right rounded-r-2xl border-y border-r border-transparent group-hover:border-gray-200">
                  {isCreator ? (
                    <button
                      onClick={() => handleDeleteTeam(team.id, team.name)}
                      className="text-red-600 hover:text-white font-medium text-sm px-4 py-2 rounded-xl border border-red-200 hover:bg-red-500 transition-all hover:shadow-sm"
                    >
                      Delete
                    </button>
                  ) : (
                    <button
                      onClick={() => handleLeaveTeam(team.id)}
                      className="text-red-500 hover:text-red-700 font-medium text-sm px-4 py-2 rounded-xl hover:bg-red-50 transition-colors"
                    >
                      Leave
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {/* Empty States */}
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
