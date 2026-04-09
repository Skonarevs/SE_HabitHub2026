import { MainLayout } from './components/layout/MainLayout';
import { Login } from './features/auth/Login';
import { Routes, Route, Navigate } from 'react-router-dom';
import { Register } from './features/auth/Register';
import { ProtectedRoute } from './features/auth/ProtectedRoute';
import { PublicRoute } from './features/auth/PublicRoute';
import { ChangeEmailPage } from './features/dashboards/settings/ChangeEmailPage';
import { ChangePasswordPage } from './features/dashboards/settings/ChangePasswordPage';
import { ActiveSessionsPage } from './features/dashboards/settings/ActiveSessionsPage';
import { MainPanel } from './features/dashboards/MainPanel';

function App() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/login" replace />} />

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
      </Route>
    </Routes>
  );
}

export default App;
