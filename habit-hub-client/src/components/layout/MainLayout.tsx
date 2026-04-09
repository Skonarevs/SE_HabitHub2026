import { Outlet } from 'react-router-dom';
import { RightPanel } from '../../features/dashboards/RightPanel';
import { Sidebar } from '../../features/dashboards/Sidebar';

export const MainLayout: React.FC = () => {
  return (
    <div className="h-screen w-full bg-gray-50 py-6 px-4 grid grid-cols-[300px_1fr_350px] overflow-hidden gap-6">
      <div className="bg-white rounded-3xl shadow-sm h-full overflow-hidden">
        <Sidebar />
      </div>

      <main className="flex flex-col h-full overflow-y-auto min-h-0">
        <Outlet />
      </main>

      <div className="bg-white rounded-3xl shadow-sm h-full overflow-hidden">
        <RightPanel />
      </div>
    </div>
  );
};
