import { NavLink, Link } from 'react-router-dom';
import { LogOutButton } from '../../components/ui/LogOutButton';
import { useAuthStore } from '../../store/useAuthStore';
import { useNotificationStore } from '../../store/useNotificationStore';

export const Sidebar = () => {
  const { role } = useAuthStore();
  const notifications = useNotificationStore((state) => state.notifications);
  const unreadCount = notifications.filter((n) => !n.isRead).length;
  return (
    // <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm max-w-4xl border border-gray-100"></div>
    <aside className="flex flex-col pl-8 py-12 justify-between h-full pr-2 border border-gray-100 rounded-3xl shadow-sm">
      <div>
        <nav className="flex flex-col gap-4 text-gray-500 font-medium">
          <Link to="/main-member" className="text-3xl font-bold text-gray-900">
            HabitHub
          </Link>

          <div className="flex items-center gap-6">
            <NavLink
              to="notifications"
              className={({ isActive }) =>
                `flex items-center gap-3 ${isActive ? 'text-black font-bold' : 'hover:text-black'}`
              }
            >
              Notifications
              {unreadCount > 0 && <span> ({unreadCount})</span>}
            </NavLink>
            {role === 'Member' && (
              <NavLink
                to="reminders"
                className={({ isActive }) =>
                  `flex items-center gap-3 ${isActive ? 'text-black font-bold' : 'hover:text-black'}`
                }
              >
                Reminders
              </NavLink>
            )}
          </div>
        </nav>
      </div>

      <div>
        <nav className="flex flex-col gap-4 text-gray-500 font-medium">
          <NavLink
            to="sessions"
            className={({ isActive }) =>
              `flex items-center gap-3 ${isActive ? 'text-black font-bold' : 'hover:text-black'}`
            }
          >
            View active sessions
          </NavLink>

          <NavLink
            to="change-password"
            className={({ isActive }) =>
              `flex items-center gap-3 ${isActive ? 'text-black font-bold' : 'hover:text-black'}`
            }
          >
            Change password
          </NavLink>

          <NavLink
            to="change-email"
            className={({ isActive }) =>
              `flex items-center gap-3 ${isActive ? 'text-black font-bold' : 'hover:text-black'}`
            }
          >
            Change email
          </NavLink>
          {role === 'Member' && (
            <NavLink
              to="teams-member"
              className={({ isActive }) =>
                `flex items-center gap-3 ${isActive ? 'text-black font-bold' : 'hover:text-black'}`
              }
            >
              Your membership
            </NavLink>
          )}
          {role === 'Creator' && (
            <NavLink
              to="teams-creator"
              className={({ isActive }) =>
                `flex items-center gap-3 ${isActive ? 'text-black font-bold' : 'hover:text-black'}`
              }
            >
              Manage your team
            </NavLink>
          )}
        </nav>
      </div>

      <div>
        <LogOutButton />
      </div>
    </aside>
  );
};
