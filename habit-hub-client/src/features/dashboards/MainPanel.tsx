import { useAuthStore } from '../../store/useAuthStore';

export const MainPanel = () => {
  const { userName } = useAuthStore();

  const today = new Date();
  const day = today.getDate();
  const month = today.toLocaleString('en-US', { month: 'short' });
  const year = today.getFullYear();
  const formattedDate = `${day} ${month}, ${year}`;

  return (
    <main className="flex flex-col px-8 py-6 h-full overflow-y-auto pr-4">
      <header className="flex justify-between items-end mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            Hello, {userName}
          </h1>
          <p className="text-gray-400 mt-1">
            Track team progress here. You almost reach a goal!
          </p>
        </div>
        <div className="font-medium text-gray-900">{formattedDate}</div>
      </header>
    </main>
  );
};
