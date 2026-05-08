import { useEffect, useState } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useAuthStore } from '../../../store/useAuthStore';
import { getTeamMembers, type TeamMemberResponseDto } from '../../../api/teamsApi';

// Define a type for your members
type MemberUI = {
  id: string;
  name: string;
  email: string;
  joinedAt: string;
  status: string;
};

export const MembersList = () => {
  // Use useParams instead of useLocation to safely get the ID from the route
  const { teamId } = useParams<{ teamId: string }>();
  const { role } = useAuthStore();
  const isCreator = role === 'Creator';

  const [members, setMembers] = useState<MemberUI[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Dynamic back path based on role
  const backToTeamsPath = isCreator
    ? '/main-creator/teams-creator'
    : '/main-member/teams-member';

  useEffect(() => {
    const fetchMembers = async () => {
      if (!teamId) {
        setError('Team ID is missing.');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        // Assuming getTeamMembers returns an array of strings (names or IDs)
        const data: TeamMemberResponseDto[] = await getTeamMembers(teamId);

        // Map the DTOs to our MemberUI objects
        const mapped: MemberUI[] = data.map((m) => ({
          id: m.userId,
          name: m.name,
          email: m.email,
          joinedAt: m.joinedAt,
          status: m.status,
        }));

        setMembers(mapped);
      } catch (err) {
        console.error('Failed to fetch members:', err);
        setError('Failed to load team members.');
      } finally {
        setLoading(false);
      }
    };

    void fetchMembers();
  }, [teamId]);

  if (loading) {
    return (
      <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden">
        <p className="text-gray-500">Loading members...</p>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden">
      {/* Header Area */}
      <div className="mb-6">
        <Link
          to={backToTeamsPath}
          className="inline-flex items-center gap-1 text-sm font-medium text-gray-400 hover:text-black transition-colors mb-4"
        >
          <span>←</span> Back to Teams
        </Link>
        <h2 className="text-2xl font-bold text-gray-800">Team Members</h2>
        <p className="text-gray-500 text-sm mt-1">
          View everyone who is part of this workspace.
        </p>
      </div>

      {error && <p className="text-red-600 mb-4">{error}</p>}

      {/* Members List Area */}
      <div className="flex-1 overflow-y-auto border border-gray-100 rounded-2xl p-4 bg-gray-50">
        {members.length === 0 ? (
          <p className="text-gray-400 italic">No members found.</p>
        ) : (
          <ul className="space-y-3">
            {members.map((member) => (
              <li
                key={member.id}
                className="flex items-center justify-between bg-white border border-gray-200 rounded-xl p-4 hover:shadow-sm transition-shadow"
              >
                <div className="flex items-center gap-4">
                  {/* Avatar Placeholder */}
                  <div className="w-10 h-10 rounded-full bg-gray-200 flex items-center justify-center text-gray-600 font-bold">
                    {member.name.charAt(0).toUpperCase()}
                  </div>
                  <span className="font-semibold text-gray-700">
                    {member.name}
                  </span>
                </div>

                {/* Optional: Add a remove button for Creators */}
                {isCreator && (
                  <button
                    className="text-sm text-red-500 hover:text-red-700 font-medium px-3 py-1.5 bg-red-50 hover:bg-red-100 rounded-lg transition-colors"
                    onClick={() => console.log(`Remove ${member.id}`)}
                  >
                    Remove
                  </button>
                )}
              </li>
            ))}
          </ul>
        )}
      </div>
    </div>
  );
};
