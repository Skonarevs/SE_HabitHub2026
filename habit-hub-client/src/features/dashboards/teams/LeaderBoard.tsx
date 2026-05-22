import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { getHabitLeaderboard, getHabitEntries } from '../../../api/teamsApi';
import type { HabitLeaderboardEntry } from '../../../types/teamsTypes';

interface EnrichedLeader extends HabitLeaderboardEntry {
  logCount: number;
}

export const LeaderBoard = () => {
  const { habitId } = useParams<{ habitId: string }>();
  const navigate = useNavigate();

  const [leaders, setLeaders] = useState<EnrichedLeader[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchLeaderboardData = async () => {
      if (!habitId) return;

      try {
        setLoading(true);

        const [leaderboardData, entriesData] = await Promise.all([
          getHabitLeaderboard(habitId),
          getHabitEntries(habitId),
        ]);

        const userLogCounts = entriesData.reduce(
          (acc: Record<string, number>, entry) => {
            if (entry.status === 'Logged') {
            
              const identifier = (entry as any).userId || entry.userName;
              if (identifier) {
                acc[identifier] = (acc[identifier] || 0) + 1;
              }
            }
            return acc;
          },
          {}
        );

        let rawLeaders: HabitLeaderboardEntry[] = [];
        if (Array.isArray(leaderboardData)) {
          rawLeaders = leaderboardData;
        } else if (
          leaderboardData &&
          Array.isArray((leaderboardData as any).data)
        ) {
          rawLeaders = (leaderboardData as any).data;
        } else if (
          leaderboardData &&
          Array.isArray((leaderboardData as any).items)
        ) {
          rawLeaders = (leaderboardData as any).items;
        }

     
        const enrichedLeaders = rawLeaders.map((user) => ({
          ...user,
  
          logCount:
            userLogCounts[user.userId] || userLogCounts[user.userName] || 0,
        }));

        setLeaders(enrichedLeaders);
      } catch (err) {
        console.error('Failed to load leaderboard:', err);
        setError('Could not load the leaderboard rankings.');
      } finally {
        setLoading(false);
      }
    };

    void fetchLeaderboardData();
  }, [habitId]);

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden">
      <div className="mb-8">
        <button
          onClick={() => navigate(-1)}
          className="inline-flex items-center gap-1 text-sm font-medium text-gray-400 hover:text-black transition-colors mb-4"
        >
          <span>←</span> Back to Habits
        </button>
        <h2 className="text-3xl font-bold text-gray-900">Habit Leaderboard</h2>
        <p className="text-gray-500 mt-2">See who is leading the pack!</p>
      </div>

      <div className="flex-1 overflow-y-auto max-w-3xl">
        {loading ? (
          <div className="text-gray-400 animate-pulse text-lg">
            Loading rankings...
          </div>
        ) : error ? (
          <div className="text-red-500 bg-red-50 p-4 rounded-xl">{error}</div>
        ) : leaders.length === 0 ? (
          <div className="text-gray-400 italic bg-gray-50 p-8 rounded-2xl text-center">
            No progress logged for this habit yet. Be the first!
          </div>
        ) : (
          <div className="flex flex-col gap-4">
            {leaders.map((user, index) => {
              const isFirst = index === 0;
              const isSecond = index === 1;
              const isThird = index === 2;

              return (
                <div
                  key={user.userId}
                  className={`flex items-center justify-between p-4 rounded-2xl border ${
                    isFirst
                      ? 'bg-yellow-50 border-yellow-200 shadow-sm'
                      : isSecond
                        ? 'bg-gray-50 border-gray-200 shadow-sm'
                        : isThird
                          ? 'bg-orange-50 border-orange-200 shadow-sm'
                          : 'bg-white border-gray-100'
                  } transition-colors`}
                >
                  <div className="flex items-center gap-6">
                    <div
                      className={`flex items-center justify-center w-10 h-10 rounded-full font-bold text-lg ${
                        isFirst
                          ? 'bg-yellow-400 text-white'
                          : isSecond
                            ? 'bg-gray-300 text-white'
                            : isThird
                              ? 'bg-orange-300 text-white'
                              : 'text-gray-500 bg-gray-100'
                      }`}
                    >
                      {index + 1}
                    </div>

                    <div>
                      <div className="font-semibold text-gray-800 text-lg">
                        {user.userName}
           
                        {(user as any).isCurrentUser && (
                          <span className="ml-3 text-xs bg-blue-100 text-blue-700 px-2 py-1 rounded-full">
                            You
                          </span>
                        )}
                      </div>

                    
                      <div className="text-sm text-gray-500 font-medium">
                        Logged {user.logCount}{' '}
                        {user.logCount === 1 ? 'time' : 'times'}
                      </div>
                    </div>
                  </div>

                 
                </div>
              );
            })}
          </div>
        )}
      </div>
    </div>
  );
};
