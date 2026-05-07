import { Outlet, useLocation } from 'react-router-dom';
import { RightPanel } from '../../features/dashboards/RightPanel';
import { Sidebar } from '../../features/dashboards/Sidebar';

export const MainLayout: React.FC = () => {
  const location = useLocation();

  const isChatOpen = location.pathname.includes('/chat');

  return (
    <div
      className={`h-screen w-full bg-gray-50 py-6 px-4 grid overflow-hidden gap-6 transition-all duration-300 ${
        isChatOpen ? 'grid-cols-[300px_1fr_350px]' : 'grid-cols-[300px_1fr]'
      }`}
    >
      <div className="bg-white rounded-3xl shadow-sm h-full overflow-hidden">
        <Sidebar />
      </div>

      <main className="flex flex-col h-full overflow-y-auto min-h-0">
        <Outlet />
      </main>

      {isChatOpen && (
        <div className="bg-white rounded-3xl shadow-sm h-full overflow-hidden">
          <RightPanel />
        </div>
      )}
    </div>
  );
};
