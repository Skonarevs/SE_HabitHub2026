import { useState } from 'react';
import { changePassword } from '../../../api/authApi';
import { useNotificationStore } from '../../../store/useNotificationStore';

export const ChangePasswordPage = () => {
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [isSuccess, setIsSuccess] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const addNotification = useNotificationStore(
    (state) => state.addNotification
  );

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();

    if (newPassword.length < 8 || !/\d/.test(newPassword)) {
      setError(
        'New password must be at least 8 characters and contain a number.'
      );
      return;
    }

    setIsLoading(true);
    setError(null);

    try {
      await changePassword({ currentPassword, newPassword });
      setIsSuccess(true);
      setCurrentPassword('');
      setNewPassword('');
      addNotification({
        type: 'success',
        title: 'Security Alert',
        message: 'Your password was successfully changed just now.',
      });
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Failed to change password.'
      );
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm border border-gray-100">
      <div className="mb-8 border-b border-gray-100 pb-4">
        <h2 className="text-2xl font-bold text-gray-900">Change Password</h2>
        <p className="text-gray-500 mt-1">
          Ensure your account is using a long, random password to stay secure.
        </p>
      </div>

      <div className="max-w-md">
        {isSuccess ? (
          <div className="bg-green-50 border border-green-200 text-green-700 p-6 rounded-2xl">
            <p className="font-bold text-lg mb-2">
              Password changed successfully!
            </p>
            <p className="text-sm">
              You can now use your new password to log in to your account.
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
                Current Password <span className="text-red-500">*</span>
              </label>
              <input
                type="password"
                value={currentPassword}
                onChange={(e) => setCurrentPassword(e.target.value)}
                required
                placeholder="Enter current password"
                className="w-full px-4 py-3 border border-gray-200 rounded-xl outline-none bg-gray-50 focus:bg-white focus:ring-2 focus:ring-gray-900 focus:border-transparent transition-all"
              />
            </div>

            <div className="flex flex-col">
              <label className="text-sm font-bold text-gray-700 mb-2">
                New Password <span className="text-red-500">*</span>
              </label>
              <input
                type="password"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
                placeholder="Min 8 characters, at least 1 number"
                className="w-full px-4 py-3 border border-gray-200 rounded-xl outline-none bg-gray-50 focus:bg-white focus:ring-2 focus:ring-gray-900 focus:border-transparent transition-all"
              />
            </div>

            {error && (
              <p className="text-sm text-red-500 font-medium">{error}</p>
            )}

            <button
              type="submit"
              disabled={isLoading}
              className="w-full py-3 mt-4 bg-gray-900 text-white rounded-xl font-bold hover:bg-gray-800 transition-all disabled:opacity-60 disabled:cursor-not-allowed"
            >
              {isLoading ? 'Saving changes...' : 'Save New Password'}
            </button>
          </form>
        )}
      </div>
    </div>
  );
};
