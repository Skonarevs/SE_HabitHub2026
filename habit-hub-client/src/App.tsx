import { MainLayout } from "./components/layout/MainLayout";
import { Login } from "./features/auth/Login";
import { Routes, Route, Navigate } from 'react-router-dom';
import { Register } from "./features/auth/Register";
import { Dashboard } from "./features/dashboard/Dashboard";



function App() {
 
  return (
  <Routes>
     
      <Route path="/" element={<Navigate to="/login" replace />} />
    
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
      
     
        <Route path="/dashboard" element={<MainLayout />}>
          <Route index element={<Dashboard/>} />
          </Route>
    </Routes>
  );
}

export default App;
