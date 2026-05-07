import { useState, useEffect } from 'react';
import { getActiveSessions } from '../../../api/sessionApi';
import type { SessionInfo } from '../../auth/types';

export const ActiveSessionsPage = () => {
  const [sessions, setSessions] = useState<SessionInfo[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const loadSessions = async () => {
      try {
        const data = await getActiveSessions();
        setSessions(data);
      } catch (err) {
        setError(
          err instanceof Error ? err.message : 'Failed to load active sessions.'
        );
      } finally {
        setIsLoading(false);
      }
    };
    loadSessions();
  }, []);

  const formatDateTime = (date: Date) => {
    if (Number.isNaN(date.getTime())) return 'Invalid date';
    return date.toLocaleString();
  };

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100">
      <div className="mb-8 border-b border-gray-100 pb-4">
        <h2 className="text-2xl font-bold text-gray-900">Active Sessions</h2>
        <p className="text-gray-500 mt-1">
          Manage and view the current sessions on your account.
        </p>
      </div>

      {isLoading && (
        <p className="text-gray-500 font-medium">Loading sessions...</p>
      )}

      {error && (
        <p className="text-red-500 bg-red-50 p-4 rounded-xl">{error}</p>
      )}

      {!isLoading && !error && sessions.length === 0 ? (
        <p className="text-gray-500">No active sessions found.</p>
      ) : (
        <div className="space-y-4 overflow-y-auto pr-2">
          {sessions.map((session) => (
            <div
              key={session.sessionId}
              className="border border-gray-200 rounded-2xl p-6 bg-gray-50/50"
            >
              <p className="text-gray-900 font-bold mb-3 break-all">
                Session ID: {session.sessionId}
              </p>
              <div className="grid grid-cols-2 gap-y-2 text-sm">
                <p className="text-gray-500">Created:</p>
                <p className="text-gray-900 font-medium">
                  {formatDateTime(session.createdAt)}
                </p>

                <p className="text-gray-500">Last activity:</p>
                <p className="text-gray-900 font-medium">
                  {formatDateTime(session.lastActivity)}
                </p>

                <p className="text-gray-500">Expires:</p>
                <p className="text-gray-900 font-medium">
                  {formatDateTime(session.expiryDate)}
                </p>

                <p className="text-gray-500 mt-2">Status:</p>
                <p className="text-green-600 font-bold mt-2 uppercase text-xs tracking-wider">
                  {session.status}
                </p>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};


