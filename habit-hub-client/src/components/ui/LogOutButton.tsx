import { useState } from 'react';
import { LogOut } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';

export const LogOutButton: React.FC = () => {
  const [showConfirm, setShowConfirm] = useState(false);
  const navigate = useNavigate();
  const { logout } = useAuthStore();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <>
      <button
        onClick={() => setShowConfirm(true)}
        className="flex items-center space-x-2 text-gray-500 hover:text-black transition-colors group"
      >
        <LogOut className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
        <span className="text-sm font-semibold hidden sm:block">Sign Out</span>
      </button>

      {showConfirm && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div
            className="absolute inset-0 bg-gray-900/60 backdrop-blur-sm"
            onClick={() => setShowConfirm(false)}
          ></div>

          <div className="relative bg-white rounded-2xl shadow-2xl max-w-sm w-full p-8 flex flex-col items-center text-center">
            {/* <div className="w-14 h-14 bg-red-50 rounded-full flex items-center justify-center mb-5">
              <AlertCircle className="w-8 h-8 text-red-500" />
            </div> */}

            <h3 className="text-xl font-bold text-gray-900">Sign Out?</h3>
            <p className="text-gray-500 mt-2 text-sm leading-relaxed">
              Are you sure you want to log out? You will need to enter your
              credentials again to access your habits.
            </p>

            <div className="flex flex-col sm:flex-row space-y-3 sm:space-y-0 sm:space-x-3 mt-8 w-full">
              <button
                onClick={() => setShowConfirm(false)}
                className="flex-1 px-4 py-2.5 border border-gray-300 rounded-xl text-gray-900 font-semibold hover:bg-gray-50 transition-all"
              >
                Cancel
              </button>
              <button
                onClick={handleLogout}
                className="flex-1 px-4 py-2.5 bg-black text-white rounded-xl font-semibold hover:bg-gray-800 shadow-lg shadow-gray-200 transition-all"
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
