import { useAuthStore } from '../../store/useAuthStore';

export const MainPanel = () => {
  const { userName, role } = useAuthStore();

  const today = new Date();
  const day = today.getDate();
  const month = today.toLocaleString('en-US', { month: 'short' });
  const year = today.getFullYear();
  const formattedDate = `${day} ${month}, ${year}`;

  return (
    <main className="flex flex-col p-8 justify-between h-full border bg-white border-gray-100 rounded-3xl shadow-sm">
      <header className="flex justify-between items-end mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">
            {role === 'Creator' && `Hello, ${userName}!`}
            {role === 'Member' && `Hey, ${userName}!`}
          </h1>

          <p className="text-gray-400 mt-2">
            Track team progress here. You almost reach a goal!
          </p>
        </div>

        <div className="font-medium text-gray-900">{formattedDate}</div>
      </header>

      <div className="flex-1"></div>
    </main>
  );
};
