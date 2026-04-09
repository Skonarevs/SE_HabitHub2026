import { NavLink, Link } from 'react-router-dom';
import { LogOutButton } from '../../components/ui/LogOutButton';
import { useAuthStore } from '../../store/useAuthStore';

export const Sidebar = () => {
  const { role } = useAuthStore();
  return (
    // <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm max-w-4xl border border-gray-100"></div>
    <aside className="flex flex-col pl-8 py-12 justify-between h-full pr-2 border border-gray-100 rounded-3xl shadow-sm">
      <div>
        <nav className="flex flex-col gap-4 text-gray-500 font-medium">
          <Link
            to="/main-member"
            className="text-3xl font-black text-gray-900 tracking-tight mb-2"
          >
            HabitHub
          </Link>

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
        </nav>
      </div>

      <div>
        <LogOutButton />
      </div>
    </aside>
  );
};
