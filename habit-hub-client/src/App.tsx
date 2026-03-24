import { MainLayout } from "./components/layout/MainLayout";
import { LoginForm } from "./features/auth/LoginForm";
import { Routes, Route, Navigate } from 'react-router-dom';
import { Register } from "./features/auth/Register";



export const Dashboard = () => {
 const name:string  = localStorage.getItem('userName') ?? 'User';
  return(
  <div>
    <h1 className="text-3xl font-bold text-gray-900 mb-4">Dashboard</h1>
    <p className="text-gray-600">{name}, welcome to HabitHub!</p>
  </div>
   );
  }

function App() {
 
  return (
  <Routes>
     
      <Route path="/" element={<Navigate to="/login" replace />} />
    
      <Route path="/login" element={<LoginForm />} />
      <Route path="/register" element={<Register />} />
      
     
        <Route path="/dashboard" element={<MainLayout />}>
          <Route index element={<Dashboard/>} />
          </Route>
    </Routes>
  );
}

export default App;
