import { useState } from 'react';
import { LogOutButton } from '../../components/ui/LogOutButton';
import { useAuthStore } from '../../store/useAuthStore';
import { getActiveSessions } from '../../api/sessionApi';
import { changePassword, changeEmail } from '../../api/authApi';
import type { SessionInfo } from '../auth/types';

export const Sidebar = () => {
  const role = useAuthStore((state) => state.role);

  // Sessions state
  const [isLoadingSessions, setIsLoadingSessions] = useState(false);
  const [sessionsError, setSessionsError] = useState<string | null>(null);
  const [sessions, setSessions] = useState<SessionInfo[]>([]);
  const [showSessionsModal, setShowSessionsModal] = useState(false);

  // Change password state
  const [showChangePasswordModal, setShowChangePasswordModal] = useState(false);
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [changePasswordError, setChangePasswordError] = useState<string | null>(null);
  const [changePasswordSuccess, setChangePasswordSuccess] = useState(false);
  const [isChangingPassword, setIsChangingPassword] = useState(false);

  // Change email state
  const [showChangeEmailModal, setShowChangeEmailModal] = useState(false);
  const [newEmail, setNewEmail] = useState('');
  const [emailPassword, setEmailPassword] = useState('');
  const [changeEmailError, setChangeEmailError] = useState<string | null>(null);
  const [changeEmailSuccess, setChangeEmailSuccess] = useState(false);
  const [isChangingEmail, setIsChangingEmail] = useState(false);

  const handleViewActiveSessions = async (
    event: React.MouseEvent<HTMLAnchorElement>
  ) => {
    event.preventDefault();
    if (isLoadingSessions) return;
    setIsLoadingSessions(true);
    setSessionsError(null);
    try {
      const sessionsResponse = await getActiveSessions();
      setSessions(sessionsResponse);
      setShowSessionsModal(true);
    } catch (error) {
      setSessionsError(
        error instanceof Error
          ? error.message
          : 'Failed to load active sessions. Please try again.'
      );
    } finally {
      setIsLoadingSessions(false);
    }
  };

  const handleOpenChangePassword = (event: React.MouseEvent<HTMLAnchorElement>) => {
    event.preventDefault();
    setCurrentPassword('');
    setNewPassword('');
    setChangePasswordError(null);
    setChangePasswordSuccess(false);
    setShowChangePasswordModal(true);
  };

  const handleChangePassword = async (event: React.FormEvent) => {
    event.preventDefault();
    if (newPassword.length < 8 || !/\d/.test(newPassword)) {
      setChangePasswordError('New password must be at least 8 characters and contain a number.');
      return;
    }
    setIsChangingPassword(true);
    setChangePasswordError(null);
    try {
      await changePassword({ currentPassword, newPassword });
      setChangePasswordSuccess(true);
      setCurrentPassword('');
      setNewPassword('');
    } catch (error) {
      setChangePasswordError(
        error instanceof Error ? error.message : 'Failed to change password.'
      );
    } finally {
      setIsChangingPassword(false);
    }
  };

  const handleOpenChangeEmail = (event: React.MouseEvent<HTMLAnchorElement>) => {
    event.preventDefault();
    setNewEmail('');
    setEmailPassword('');
    setChangeEmailError(null);
    setChangeEmailSuccess(false);
    setShowChangeEmailModal(true);
  };

  const handleChangeEmail = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!/^\S+@\S+\.\S+$/.test(newEmail)) {
      setChangeEmailError('Please enter a valid email address.');
      return;
    }
    setIsChangingEmail(true);
    setChangeEmailError(null);
    try {
      await changeEmail({ newEmail, password: emailPassword });
      setChangeEmailSuccess(true);
      setNewEmail('');
      setEmailPassword('');
    } catch (error) {
      setChangeEmailError(
        error instanceof Error ? error.message : 'Failed to change email.'
      );
    } finally {
      setIsChangingEmail(false);
    }
  };

  const formatDateTime = (date: Date) => {
    if (Number.isNaN(date.getTime())) return 'Invalid date';
    return date.toLocaleString();
  };

  return (
    <>
      <aside className="flex flex-col pl-8 py-8 justify-between h-full overflow-y-auto pr-2">
        <div>
          <nav className="flex flex-col gap-4 text-gray-500 font-medium">
            <h1 className="text-3xl font-bold text-gray-900 tracking-tight mb-2">
              HabitHub
            </h1>
            <div className="flex items-center gap-6">
              <a href="#" className="hover:text-black flex items-center gap-2">
                <span className="text-gray-400"></span> Notifications
              </a>
              {role === 'Member' && (
                <a href="#" className="hover:text-black flex items-center gap-2">
                  <span className="text-gray-400"></span> Reminders
                </a>
              )}
            </div>
          </nav>
        </div>

        <div>
          <nav className="flex flex-col gap-4 text-gray-500 font-medium">
            <a
              href="#"
              onClick={handleViewActiveSessions}
              className="hover:text-black flex items-center gap-3"
            >
              <span className="text-gray-400"></span>
              {isLoadingSessions ? 'Loading sessions...' : 'View active sessions'}
            </a>
            {sessionsError && <p className="text-sm text-red-500">{sessionsError}</p>}
            <a
              href="#"
              onClick={handleOpenChangePassword}
              className="hover:text-black flex items-center gap-3"
            >
              <span className="text-gray-400"></span> Change password
            </a>
            <a
              href="#"
              onClick={handleOpenChangeEmail}
              className="hover:text-black flex items-center gap-3"
            >
              <span className="text-gray-400"></span> Change email
            </a>
            {role === 'Member' && (
              <a href="#" className="hover:text-black flex items-center gap-3">
                <span className="text-gray-400"></span> Your membership
              </a>
            )}
            {role === 'Creator' && (
              <a href="#" className="hover:text-black flex items-center gap-3">
                <span className="text-gray-400"></span> Manage your team
              </a>
            )}
          </nav>
        </div>

        <div>
          <nav className="flex flex-col gap-4 text-gray-500 font-medium">
            <LogOutButton />
          </nav>
        </div>
      </aside>

      {showSessionsModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div
            className="absolute inset-0 bg-gray-900/60 backdrop-blur-sm"
            onClick={() => setShowSessionsModal(false)}
          ></div>
          <div className="relative bg-white rounded-2xl shadow-2xl max-w-2xl w-full p-6">
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-xl font-bold text-gray-900">Active Sessions</h3>
              <button onClick={() => setShowSessionsModal(false)} className="text-sm text-gray-500 hover:text-gray-900">Close</button>
            </div>
            {sessions.length === 0 ? (
              <p className="text-sm text-gray-500">No active sessions found.</p>
            ) : (
              <div className="space-y-3 max-h-96 overflow-y-auto pr-1">
                {sessions.map((session) => (
                  <div key={session.sessionId} className="border border-gray-200 rounded-xl p-4 text-sm">
                    <p className="text-gray-900 font-semibold break-all">Session: {session.sessionId}</p>
                    <p className="text-gray-600 mt-1">Created: {formatDateTime(session.createdAt)}</p>
                    <p className="text-gray-600">Last activity: {formatDateTime(session.lastActivity)}</p>
                    <p className="text-gray-600">Expires: {formatDateTime(session.expiryDate)}</p>
                    <p className="text-gray-700 mt-1">Status: {session.status}</p>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}

      {showChangePasswordModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-gray-900/60 backdrop-blur-sm" onClick={() => setShowChangePasswordModal(false)}></div>
          <div className="relative bg-white rounded-2xl shadow-2xl max-w-sm w-full p-8">
            <div className="flex items-center justify-between mb-6">
              <h3 className="text-xl font-bold text-gray-900">Change Password</h3>
              <button onClick={() => setShowChangePasswordModal(false)} className="text-sm text-gray-500 hover:text-gray-900">Close</button>
            </div>
            {changePasswordSuccess ? (
              <div className="text-center py-4">
                <p className="text-green-600 font-semibold">Password changed successfully!</p>
                <button onClick={() => setShowChangePasswordModal(false)} className="mt-4 px-4 py-2 bg-gray-900 text-white rounded-xl text-sm font-semibold hover:bg-gray-800 transition-all">Close</button>
              </div>
            ) : (
              <form onSubmit={handleChangePassword} className="flex flex-col space-y-4">
                <div className="flex flex-col">
                  <label className="text-sm font-medium text-gray-700 mb-1">Current Password <span className="text-red-500">*</span></label>
                  <input type="password" value={currentPassword} onChange={(e) => setCurrentPassword(e.target.value)} required placeholder="Enter current password" className="w-full px-4 py-2.5 border border-gray-200 rounded-lg outline-none bg-gray-50 focus:bg-white focus:ring-2 focus:ring-gray-900 focus:border-gray-900 transition-colors" />
                </div>
                <div className="flex flex-col">
                  <label className="text-sm font-medium text-gray-700 mb-1">New Password <span className="text-red-500">*</span></label>
                  <input type="password" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} required placeholder="Min 8 characters, at least 1 number" className="w-full px-4 py-2.5 border border-gray-200 rounded-lg outline-none bg-gray-50 focus:bg-white focus:ring-2 focus:ring-gray-900 focus:border-gray-900 transition-colors" />
                </div>
                {changePasswordError && <p className="text-sm text-red-500">{changePasswordError}</p>}
                <button type="submit" disabled={isChangingPassword} className="w-full py-2.5 bg-gray-900 text-white rounded-xl font-semibold text-sm hover:bg-gray-800 transition-all disabled:opacity-60 disabled:cursor-not-allowed">
                  {isChangingPassword ? 'Changing...' : 'Change Password'}
                </button>
              </form>
            )}
          </div>
        </div>
      )}

      {showChangeEmailModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-gray-900/60 backdrop-blur-sm" onClick={() => setShowChangeEmailModal(false)}></div>
          <div className="relative bg-white rounded-2xl shadow-2xl max-w-sm w-full p-8">
            <div className="flex items-center justify-between mb-6">
              <h3 className="text-xl font-bold text-gray-900">Change Email</h3>
              <button onClick={() => setShowChangeEmailModal(false)} className="text-sm text-gray-500 hover:text-gray-900">Close</button>
            </div>
            {changeEmailSuccess ? (
              <div className="text-center py-4">
                <p className="text-green-600 font-semibold">Email changed successfully!</p>
                <button onClick={() => setShowChangeEmailModal(false)} className="mt-4 px-4 py-2 bg-gray-900 text-white rounded-xl text-sm font-semibold hover:bg-gray-800 transition-all">Close</button>
              </div>
            ) : (
              <form onSubmit={handleChangeEmail} className="flex flex-col space-y-4">
                <div className="flex flex-col">
                  <label className="text-sm font-medium text-gray-700 mb-1">New Email <span className="text-red-500">*</span></label>
                  <input type="email" value={newEmail} onChange={(e) => setNewEmail(e.target.value)} required placeholder="Enter new email" className="w-full px-4 py-2.5 border border-gray-200 rounded-lg outline-none bg-gray-50 focus:bg-white focus:ring-2 focus:ring-gray-900 focus:border-gray-900 transition-colors" />
                </div>
                <div className="flex flex-col">
                  <label className="text-sm font-medium text-gray-700 mb-1">Password <span className="text-red-500">*</span></label>
                  <input type="password" value={emailPassword} onChange={(e) => setEmailPassword(e.target.value)} required placeholder="Enter your password to confirm" className="w-full px-4 py-2.5 border border-gray-200 rounded-lg outline-none bg-gray-50 focus:bg-white focus:ring-2 focus:ring-gray-900 focus:border-gray-900 transition-colors" />
                </div>
                {changeEmailError && <p className="text-sm text-red-500">{changeEmailError}</p>}
                <button type="submit" disabled={isChangingEmail} className="w-full py-2.5 bg-gray-900 text-white rounded-xl font-semibold text-sm hover:bg-gray-800 transition-all disabled:opacity-60 disabled:cursor-not-allowed">
                  {isChangingEmail ? 'Changing...' : 'Change Email'}
                </button>
              </form>
            )}
          </div>
        </div>
      )}
    </>
  );
};
