import { LogOutButton } from '../../components/ui/LogOutButton';
import { useAuthStore } from '../../store/useAuthStore';

export const Sidebar = () => {
  const role = useAuthStore((state) => state.role);
  return (
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
          <a href="#" className="hover:text-black flex items-center gap-3">
            <span className="text-gray-400"></span> View active sessions
          </a>
          <a href="#" className="hover:text-black flex items-center gap-3">
            <span className="text-gray-400"></span> Change password
          </a>
          <a href="#" className="hover:text-black flex items-center gap-3">
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
  );
};
