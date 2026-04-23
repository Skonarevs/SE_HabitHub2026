import { useEffect } from 'react';
import { Navigate, Route, Routes } from 'react-router-dom';
import { MainLayout } from './components/layout/MainLayout';
import { AboutPage } from './features/auth/AboutPage';
import { Login } from './features/auth/Login';
import { ProtectedRoute } from './features/auth/ProtectedRoute';
import { PublicRoute } from './features/auth/PublicRoute';
import { Register } from './features/auth/Register';
import { MainPanel } from './features/dashboards/MainPanel';
import { ActiveSessionsPage } from './features/dashboards/settings/ActiveSessionsPage';
import { ChangeEmailPage } from './features/dashboards/settings/ChangeEmailPage';
import { ChangePasswordPage } from './features/dashboards/settings/ChangePasswordPage';
import { Notifications } from './features/dashboards/settings/Notifications';
import { Reminders } from './features/dashboards/settings/Reminders';
import { useNotificationStore } from './store/useNotificationStore';
import { TeamsPanel } from './features/dashboards/settings/TeamsPanel';

function App() {
  const initWelcome = useNotificationStore((state) => state.initWelcome);

  useEffect(() => {
    initWelcome();
  }, [initWelcome]);

  return (
    <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />
      <Route path="/about" element={<AboutPage />} />
      <Route
        path="/login"
        element={
          <PublicRoute>
            <Login />
          </PublicRoute>
        }
      />
      <Route
        path="/register"
        element={
          <PublicRoute>
            <Register />
          </PublicRoute>
        }
      />

      <Route
        path="/main-creator"
        element={
          <ProtectedRoute allowedRoles={'Creator'}>
            <MainLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<MainPanel />} />
        <Route path="sessions" element={<ActiveSessionsPage />} />
        <Route path="change-password" element={<ChangePasswordPage />} />
        <Route path="change-email" element={<ChangeEmailPage />} />
        <Route path="notifications" element={<Notifications />} />
        <Route path="teams-creator" element={<TeamsPanel />} />
      </Route>

      <Route
        path="/main-member"
        element={
          <ProtectedRoute allowedRoles={'Member'}>
            <MainLayout />
          </ProtectedRoute>
        }
      >
        <Route index element={<MainPanel />} />
        <Route path="sessions" element={<ActiveSessionsPage />} />
        <Route path="change-password" element={<ChangePasswordPage />} />
        <Route path="change-email" element={<ChangeEmailPage />} />
        <Route path="notifications" element={<Notifications />} />
        <Route path="reminders" element={<Reminders />} />
        <Route path="teams-member" element={<TeamsPanel />} />
      </Route>
    </Routes>
  );
}

export default App;
