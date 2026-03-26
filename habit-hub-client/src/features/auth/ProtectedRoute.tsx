import { Navigate } from 'react-router-dom';

type Props = {
  children: React.ReactNode;
};

export const ProtectedRoute = ({ children }: Props) => {
  const isAuth = !!localStorage.getItem('sessionId');

  if (!isAuth) {
    return <Navigate to="/login" replace />;
  }

  return <>{children}</>;
};
