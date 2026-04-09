import { Navigate } from 'react-router-dom';
import { useAuthStore } from '../../store/useAuthStore';

type Props = {
  children: React.ReactNode;
  allowedRoles: 'Creator' | 'Member';
};

export const ProtectedRoute = ({ children, allowedRoles }: Props) => {
  const { role, isAuth } = useAuthStore();

  if (!isAuth) {
    return <Navigate to="/login" replace />;
  }

  if (role && !allowedRoles.includes(role)) {
    return (
      <Navigate
        to={role === 'Creator' ? '/main-creator' : '/main-member'}
        replace
      />
    );
  }

  return <>{children}</>;
};
