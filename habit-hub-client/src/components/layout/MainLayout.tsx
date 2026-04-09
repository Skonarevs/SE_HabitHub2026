import { Outlet } from 'react-router-dom';
import { RightPanel } from '../../features/dashboards/RightPanel';
import { Sidebar } from '../../features/dashboards/Sidebar';
import { MainPanel } from '../../features/dashboards/MainPanel';

export const MainLayout: React.FC = () => {
  return (
    <div className="h-screen w-full bg-white grid grid-cols-[300px_1fr_350px] overflow-hidden">
      <Sidebar />

      <main className="flex flex-col h-full overflow-y-auto bg-gray-50 p-6 min-h-0">
        <Outlet />
        <MainPanel />
      </main>

      <RightPanel />
    </div>
  );
};
