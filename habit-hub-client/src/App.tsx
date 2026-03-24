import { MainLayout } from "./components/layout/MainLayout";
import { Login } from "./features/auth/Login";
import { Routes, Route, Navigate } from 'react-router-dom';
import { Register } from "./features/auth/Register";
import { Dashboard } from "./features/dashboard/Dashboard";
import { ProtectedRoute } from "./features/auth/ProtectedRoute";
import { PublicRoute } from "./features/auth/PublicRoute";



function App() {
 
  return (
  <Routes>
     
      <Route path="/" element={<Navigate to="/login" replace />} />
    
      <Route path="/login" element={<PublicRoute><Login /></PublicRoute>} />
      <Route path="/register" element={<PublicRoute><Register /></PublicRoute>} />
      
     
        <Route path="/dashboard" element={<ProtectedRoute><MainLayout /></ProtectedRoute>} >
          <Route index element={<Dashboard/>} />
          </Route>
    </Routes>
  );
}

export default App;
