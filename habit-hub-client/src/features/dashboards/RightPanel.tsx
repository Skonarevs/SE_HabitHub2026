import { useEffect, useMemo, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import {
  deleteTeamMessage,
  getTeamMessages,
  sendTeamMessage,
} from '../../api/chatApi';
import { useAuthStore } from '../../store/useAuthStore';
import type { ChatMessage } from '../../types/chatTypes';

export const RightPanel = () => {
  const { teamId } = useParams<{ teamId: string }>();
  const { role, userName } = useAuthStore();
  const isCreator = role === 'Creator';

  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [content, setContent] = useState('');
  const [loading, setLoading] = useState(true);
  const [sending, setSending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const backToTeamsPath = isCreator
    ? '/main-creator/teams-creator'
    : '/main-member/teams-member';

  const sortedMessages = useMemo(
    () => [...messages].sort((a, b) => a.sentAt.getTime() - b.sentAt.getTime()),
    [messages]
  );

  const loadMessages = async () => {
    if (!teamId) {
      setError('Team id is missing.');
      setLoading(false);
      return;
    }

    try {
      setLoading(true);
      setError(null);
      const data = await getTeamMessages(teamId);
      setMessages(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load messages.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadMessages();
  }, [teamId]);

  const handleSend = async () => {
    if (!teamId) {
      setError('Team id is missing.');
      return;
    }

    const trimmed = content.trim();
    if (!trimmed) return;

    try {
      setSending(true);
      const created = await sendTeamMessage(teamId, { content: trimmed });
      setMessages((prev) => [...prev, created]);
      setContent('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to send message.');
    } finally {
      setSending(false);
    }
  };

  const handleDelete = async (message: ChatMessage) => {
    if (!teamId) {
      setError('Team id is missing.');
      return;
    }

    if (!window.confirm('Delete this message?')) return;

    try {
      await deleteTeamMessage(teamId, message.id);
      setMessages((prev) => prev.filter((m) => m.id !== message.id));
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Failed to delete message.'
      );
    }
  };

  if (loading) {
    return (
      <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden">
        Loading chat...
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden">
      <div className="mb-6 flex items-end justify-between gap-3">
        <div>
          <Link
            to={backToTeamsPath}
            className="inline-flex items-center gap-1 text-sm font-medium text-gray-400 hover:text-black transition-colors mb-2"
          >
            <span>←</span> Back to Teams
          </Link>
          <h2 className="text-2xl font-bold text-gray-800">Team Chat</h2>
          <p className="text-gray-500 text-sm mt-1">
            Write and manage team messages.
          </p>
        </div>
        <button
          onClick={() => void loadMessages()}
          className="text-sm font-medium text-gray-500 hover:text-gray-800 transition-colors px-2"
        >
          Refresh
        </button>
      </div>

      {error && <p className="text-red-600 mb-4">{error}</p>}

      <div className="flex-1 overflow-y-auto border border-gray-100 rounded-2xl p-4 bg-gray-50">
        {sortedMessages.length === 0 ? (
          <p className="text-gray-400 italic">No messages yet.</p>
        ) : (
          <div className="space-y-3">
            {sortedMessages.map((message) => {
              const canDelete =
                isCreator ||
                (userName !== null && message.senderName === userName);

              return (
                <div
                  key={message.id}
                  className="bg-white border border-gray-200 rounded-xl p-3"
                >
                  <div className="flex items-start justify-between gap-3">
                    <div>
                      <p className="text-sm font-semibold text-gray-800">
                        {message.senderName}
                      </p>
                      <p className="text-xs text-gray-400 mt-0.5">
                        {message.sentAt.toLocaleString()}
                      </p>
                    </div>
                    {canDelete && (
                      <button
                        onClick={() => void handleDelete(message)}
                        className="text-xs text-red-500 hover:text-red-700 font-medium"
                      >
                        Delete
                      </button>
                    )}
                  </div>
                  <p className="text-sm text-gray-700 mt-2 whitespace-pre-wrap">
                    {message.content}
                  </p>
                </div>
              );
            })}
          </div>
        )}
      </div>

      <div className="mt-4 flex gap-3">
        <input
          value={content}
          onChange={(e) => setContent(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
              e.preventDefault();
              void handleSend();
            }
          }}
          type="text"
          placeholder="Write a message..."
          className="flex-1 border border-gray-300 rounded-xl px-4 py-2.5 focus:outline-none focus:ring-2 focus:ring-black"
        />
        <button
          onClick={() => void handleSend()}
          disabled={sending || !content.trim()}
          className="bg-black hover:bg-gray-800 text-white px-5 py-2.5 rounded-xl text-sm font-semibold disabled:opacity-50"
        >
          {sending ? 'Sending...' : 'Send'}
        </button>
      </div>
    </div>
  );
};


