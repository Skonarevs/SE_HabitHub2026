import { useNavigate } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';

export const AboutPage = () => {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen bg-white/90 flex items-center justify-center p-4">
      <div className="absolute inset-0 bg-black/10" />
      <div className="bg-white rounded-3xl shadow-xl max-w-2xl w-full p-10 relative">
        <button
          onClick={() => navigate(-1)}
          className="absolute top-8 left-8 flex items-center gap-2 text-gray-400 hover:text-gray-900 transition-colors font-medium text-sm"
        >
          <ArrowLeft size={18} />
          Back
        </button>

        <div className="text-center mt-8">
          <h1 className="text-4xl font-black text-gray-900 tracking-tight mb-4">
            About HabitHub
          </h1>
          <p className="text-lg text-gray-500 mb-8 max-w-lg mx-auto">
            HabitHub is your ultimate companion for building good habits and
            breaking bad ones. Track your progress, collaborate with your team,
            and achieve your goals together.
          </p>
        </div>

        <div className="grid grid-cols-2 gap-6 mt-10">
          <div className="bg-gray-50 p-6 rounded-2xl border border-gray-100">
            <h3 className="font-bold text-gray-900 mb-2">For Members</h3>
            <p className="text-sm text-gray-500">
              Track daily tasks, monitor your streaks, and stay motivated with
              personal reminders.
            </p>
          </div>
          <div className="bg-gray-50 p-6 rounded-2xl border border-gray-100">
            <h3 className="font-bold text-gray-900 mb-2">For Creators</h3>
            <p className="text-sm text-gray-500">
              Manage your team, view global analytics, and set collective goals
              for everyone.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};


