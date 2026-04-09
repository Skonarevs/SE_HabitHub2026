import { Navigate } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';

export const PublicRoute = ({ children }: { children: React.ReactNode }) => {
  const { isAuth, role } = useAuthStore();

  if (isAuth) {
    if (role === 'Creator') {
      return <Navigate to="/main-creator" replace />;
    } else {
      return <Navigate to="/main-member" replace />;
    }
  }

  return <>{children}</>;
};
