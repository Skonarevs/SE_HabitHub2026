import { useEffect, useState } from 'react';
import { Link, useLocation, useParams } from 'react-router-dom';
import { useAuthStore } from '../../../store/useAuthStore';
import { getHabitEntries, logProgress } from '../../../api/teamsApi';
import type { HabitEntryResponseDto } from '../../../types/teamsTypes';

interface LocationState {
  habitType?: string;
  habitName?: string;
}

export const HabitLogs = () => {
  const { role } = useAuthStore();
  const isCreator = role === 'Creator';
  const { teamId, habitId } = useParams<{ teamId: string; habitId: string }>();
  const location = useLocation();
  const { habitType, habitName } = (location.state as LocationState) ?? {};
  const isQuantitative = habitType === 'Quantitative';

  const [entries, setEntries] = useState<HabitEntryResponseDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Log form state
  const [logValue, setLogValue] = useState('');
  const [logNotes, setLogNotes] = useState('');
  const [logLoading, setLogLoading] = useState(false);
  const [logError, setLogError] = useState<string | null>(null);

  const backPath = isCreator
    ? `/main-creator/teams-creator/habits-creator/${teamId}`
    : `/main-member/teams-member/habits-member/${teamId}`;

  const todayStr = new Date().toISOString().slice(0, 10);
  const loggedToday = entries.some(
    (e) => e.date.slice(0, 10) === todayStr
  );

  const loadEntries = async () => {
    if (!habitId) {
      setError('Habit id is missing.');
      setLoading(false);
      return;
    }
    try {
      setLoading(true);
      setError(null);
      const data = await getHabitEntries(habitId);
      setEntries(data);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load entries.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadEntries();
  }, [habitId]);

  const handleLog = async (status: 'Logged' | 'Skipped') => {
    if (!habitId) return;
    if (isQuantitative && status === 'Logged' && !logValue.trim()) {
      setLogError('Please enter a value.');
      return;
    }
    try {
      setLogLoading(true);
      setLogError(null);
      await logProgress(habitId, {
        status,
        value: isQuantitative && status === 'Logged' ? parseFloat(logValue) : undefined,
        notes: logNotes.trim() || undefined,
      });
      setLogValue('');
      setLogNotes('');
      await loadEntries();
    } catch (err) {
      setLogError(err instanceof Error ? err.message : 'Failed to log progress.');
    } finally {
      setLogLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100">
        Loading logs...
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100">
        <p className="text-red-600 mb-4">{error}</p>
        <Link to={backPath} className="text-sm text-gray-500 hover:text-black">
          ← Back to Habits
        </Link>
      </div>
    );
  }

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100 overflow-hidden">
      {/* Header */}
      <div className="mb-6">
        <Link
          to={backPath}
          className="inline-flex items-center gap-1 text-sm font-medium text-gray-400 hover:text-black transition-colors mb-2"
        >
          <span>←</span> Back to Habits
        </Link>
        <h2 className="text-2xl font-bold text-gray-800">
          {habitName ?? 'Habit'} — Logs
        </h2>
        <p className="text-gray-500 text-sm mt-1">
          {habitType ? `${habitType} habit` : ''} · All recorded entries
        </p>
      </div>

      {/* Log Today — members only */}
      {!isCreator && (
        <div className="mb-8 border border-gray-200 rounded-2xl p-5 bg-gray-50">
          <h3 className="text-sm font-semibold text-gray-700 mb-3">
            {loggedToday ? '✓ Already logged today' : "Log Today's Progress"}
          </h3>

          {loggedToday ? (
            <p className="text-sm text-gray-400">
              You've already submitted an entry for today.
            </p>
          ) : (
            <div className="flex flex-col gap-3">
              {isQuantitative && (
                <div className="flex items-center gap-3">
                  <label className="text-sm text-gray-600 w-14 shrink-0">Value</label>
                  <input
                    type="number"
                    value={logValue}
                    onChange={(e) => setLogValue(e.target.value)}
                    placeholder="e.g. 5"
                    className="border border-gray-300 rounded-xl px-3 py-2 text-sm w-32 focus:outline-none focus:ring-1 focus:ring-black"
                  />
                </div>
              )}
              <div className="flex items-center gap-3">
                <label className="text-sm text-gray-600 w-14 shrink-0">Notes</label>
                <input
                  type="text"
                  value={logNotes}
                  onChange={(e) => setLogNotes(e.target.value)}
                  placeholder="Optional notes..."
                  className="border border-gray-300 rounded-xl px-3 py-2 text-sm flex-1 focus:outline-none focus:ring-1 focus:ring-black"
                />
              </div>

              {logError && (
                <p className="text-sm text-red-600">{logError}</p>
              )}

              <div className="flex gap-3 mt-1">
                <button
                  onClick={() => void handleLog('Logged')}
                  disabled={logLoading}
                  className="bg-black hover:bg-gray-800 text-white px-5 py-2 rounded-xl text-sm font-semibold disabled:opacity-50 transition-all"
                >
                  {logLoading ? 'Saving...' : '✓ Mark as Done'}
                </button>
                <button
                  onClick={() => void handleLog('Skipped')}
                  disabled={logLoading}
                  className="bg-white border border-gray-300 hover:bg-gray-100 text-gray-600 px-5 py-2 rounded-xl text-sm font-semibold disabled:opacity-50 transition-all"
                >
                  Skip Today
                </button>
              </div>
            </div>
          )}
        </div>
      )}

      {/* Entries Table */}
      <div className="overflow-x-auto">
        {entries.length === 0 ? (
          <div className="text-center py-12 text-gray-400 italic">
            No log entries yet.
          </div>
        ) : (
          <table className="w-full text-left border-separate border-spacing-y-3">
            <thead>
              <tr className="text-gray-400 text-sm uppercase tracking-wider">
                <th className="px-4 py-2 font-medium">Date</th>
                <th className="px-4 py-2 font-medium">Member</th>
                <th className="px-4 py-2 font-medium">Status</th>
                <th className="px-4 py-2 font-medium">Value</th>
                <th className="px-4 py-2 font-medium">Notes</th>
              </tr>
            </thead>
            <tbody>
              {entries.map((entry) => (
                <tr
                  key={entry.id}
                  className="bg-gray-50 hover:bg-gray-100 transition-colors group"
                >
                  <td className="px-4 py-4 rounded-l-2xl border-y border-l border-transparent group-hover:border-gray-200">
                    <span className="text-gray-700 font-medium">
                      {new Date(entry.date).toLocaleDateString()}
                    </span>
                  </td>
                  <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                    <span className="text-gray-600 text-sm">
                      {entry.userName ?? '—'}
                    </span>
                  </td>
                  <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                    <span
                      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                        entry.status === 'Logged'
                          ? 'bg-green-100 text-green-800'
                          : 'bg-gray-100 text-gray-600'
                      }`}
                    >
                      {entry.status}
                    </span>
                  </td>
                  <td className="px-4 py-4 border-y border-transparent group-hover:border-gray-200">
                    <span className="text-gray-500 text-sm">
                      {entry.value != null ? entry.value : '—'}
                    </span>
                  </td>
                  <td className="px-4 py-4 rounded-r-2xl border-y border-r border-transparent group-hover:border-gray-200">
                    <span className="text-gray-500 text-sm">
                      {entry.notes ?? '—'}
                    </span>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
};
