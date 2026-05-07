import { useState } from 'react';
import { changeEmail } from '../../../api/authApi';
import { useNotificationStore } from '../../../store/useNotificationStore';

export const ChangeEmailPage = () => {
  const [newEmail, setNewEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSuccess, setIsSuccess] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const addNotification = useNotificationStore(
    (state) => state.addNotification
  );

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();

    if (!/^\S+@\S+\.\S+$/.test(newEmail)) {
      setError('Please enter a valid email address.');
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      await changeEmail({ newEmail, password });
      setIsSuccess(true);
      setNewEmail('');
      setPassword('');
      addNotification({
        type: 'success',
        title: 'Security Alert',
        message: 'Your email was successfully changed just now.',
      });
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to change email.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm border border-gray-100">
      <div className="mb-8 border-b border-gray-100 pb-4">
        <h2 className="text-2xl font-bold text-gray-900">
          Change Email Address
        </h2>
        <p className="text-gray-500 mt-1">
          Update the email address associated with your HabitHub account.
        </p>
      </div>

      <div className="max-w-md">
        {isSuccess ? (
          <div className="bg-green-50 border border-green-200 text-green-700 p-6 rounded-2xl">
            <p className="font-bold text-lg mb-2">
              Email updated successfully!
            </p>
            <p className="text-sm">
              We've sent a verification link to your new email address.
            </p>
            <button
              onClick={() => setIsSuccess(false)}
              className="mt-4 px-4 py-2 bg-white border border-green-300 rounded-lg text-sm font-semibold hover:bg-green-50 transition-all"
            >
              Change again
            </button>
          </div>
        ) : (
          <form onSubmit={handleSubmit} className="flex flex-col space-y-5">
            <div className="flex flex-col">
              <label className="text-sm font-bold text-gray-700 mb-2">
                New Email Address <span className="text-red-500">*</span>
              </label>
              <input
                type="email"
                value={newEmail}
                onChange={(e) => setNewEmail(e.target.value)}
                required
                placeholder="e.g. name@company.com"
                className="w-full px-4 py-3 border border-gray-200 rounded-xl outline-none bg-gray-50 focus:bg-white focus:ring-2 focus:ring-gray-900 focus:border-transparent transition-all"
              />
            </div>

            <div className="flex flex-col">
              <label className="text-sm font-bold text-gray-700 mb-2">
                Confirm with Password <span className="text-red-500">*</span>
              </label>
              <input
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                placeholder="Enter your current password"
                className="w-full px-4 py-3 border border-gray-200 rounded-xl outline-none bg-gray-50 focus:bg-white focus:ring-2 focus:ring-gray-900 focus:border-transparent transition-all"
              />
              <p className="text-xs text-gray-400 mt-2">
                We need your password to verify this change.
              </p>
            </div>

            {error && (
              <p className="text-sm text-red-500 font-medium">{error}</p>
            )}

            <button
              type="submit"
              disabled={isLoading}
              className="w-full py-3 mt-4 bg-gray-900 text-white rounded-xl font-bold hover:bg-gray-800 transition-all disabled:opacity-60 disabled:cursor-not-allowed"
            >
              {isLoading ? 'Updating...' : 'Update Email'}
            </button>
          </form>
        )}
      </div>
    </div>
  );
};


