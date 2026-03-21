import { LayoutDashboard, LogOut } from 'lucide-react'; 
import { useNavigate } from 'react-router-dom';



export const Header = () => {
     const navigate = useNavigate();
     
    const handleLogout = () => {
        navigate('/login');
    };

    return (
        <header className="bg-white shadow-sm border-b border-gray-200 px-6 py-4 flex justify-between items-center sticky top-0 z-10">
            
            
            <div className="flex items-center space-x-2 text-blue-600 cursor-pointer">
                <LayoutDashboard className="w-6 h-6" />
                <h1 className="text-xl font-bold tracking-tight text-gray-900">
                    Habit<span className="text-blue-600">Hub</span>
                </h1>
            </div>

       
            <div className="flex items-center space-x-6">
                
                <div className="hidden sm:flex items-center space-x-1">
                    <span className="text-xs text-gray-500 uppercase tracking-wider font-semibold">Role:</span>
                    <span className="text-sm text-blue-700 bg-blue-50 px-2 py-1 rounded-md font-medium">
                        Creator
                    </span>
                </div>

                <button 
                    onClick={handleLogout}
                    className="flex items-center space-x-1 text-gray-500 hover:text-red-600 transition-colors"
                    title="Выйти"
                >
                    <LogOut className="w-5 h-5" />
                    <span className="text-sm font-medium hidden sm:block">LogOut</span>
                </button>
            </div>
            
        </header>
    );
};