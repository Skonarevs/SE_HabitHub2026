import { Navigate } from 'react-router-dom';

export const PublicRoute = ({ children }: { children: React.ReactNode }) => {
  const isAuth = !!localStorage.getItem('sessionId');
  return isAuth ? <Navigate to="/dashboard" replace /> : <>{children}</>;
};
