import { useState } from 'react';
import { LogOut, LayoutDashboard, AlertCircle } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';

export const Header = () => {
  const [showConfirm, setShowConfirm] = useState(false);
  const navigate = useNavigate();
  const logout = useAuthStore((state) => state.logout);
  const handleLogout = () => {
    logout();
    setShowConfirm(false);
    navigate('/login');
  };

  return (
    <>
      <header className="bg-white border-b border-gray-200 px-6 py-4 flex justify-between items-center sticky top-0 z-10">
        <div className="flex items-center space-x-2 text-blue-600">
          <LayoutDashboard className="w-6 h-6" />
          <h1 className="text-xl font-bold text-gray-900 tracking-tight">
            Habit<span className="text-blue-600">Hub</span>
          </h1>
        </div>

        <button
          onClick={() => setShowConfirm(true)}
          className="flex items-center space-x-2 text-gray-500 hover:text-red-600 transition-colors group"
        >
          <LogOut className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
          <span className="text-sm font-semibold hidden sm:block">
            Sign Out
          </span>
        </button>
      </header>

      {showConfirm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div
            className="absolute inset-0 bg-gray-900/60 backdrop-blur-sm"
            onClick={() => setShowConfirm(false)}
          ></div>

          <div className="relative bg-white rounded-2xl shadow-2xl max-w-sm w-full p-8 flex flex-col items-center text-center">
            <div className="w-14 h-14 bg-red-50 rounded-full flex items-center justify-center mb-5">
              <AlertCircle className="w-8 h-8 text-red-500" />
            </div>

            <h3 className="text-xl font-bold text-gray-900">Sign Out?</h3>
            <p className="text-gray-500 mt-2 text-sm leading-relaxed">
              Are you sure you want to log out? You will need to enter your
              credentials again to access your habits.
            </p>

            <div className="flex flex-col sm:flex-row space-y-3 sm:space-y-0 sm:space-x-3 mt-8 w-full">
              <button
                onClick={() => setShowConfirm(false)}
                className="flex-1 px-4 py-2.5 border border-gray-300 rounded-xl text-gray-700 font-semibold hover:bg-gray-50 transition-all"
              >
                Cancel
              </button>
              <button
                onClick={handleLogout}
                className="flex-1 px-4 py-2.5 bg-red-600 text-white rounded-xl font-semibold hover:bg-red-700 shadow-lg shadow-red-200 transition-all"
              >
                Yes, Sign Out
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
};
