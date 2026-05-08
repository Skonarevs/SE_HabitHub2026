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
import { TeamsPanel } from './features/dashboards/teams/TeamsPanel';
import { JoinTeams } from './features/dashboards/teams/JoinTeams';
import { CreateTeam } from './features/dashboards/teams/CreateTeam';
import { HabitsList } from './features/dashboards/teams/HabitsList';
import { HabitLogs } from './features/dashboards/teams/HabitLogs';
import { CreateHabit } from './features/dashboards/teams/CreateHabit';
import { MembersList } from './features/dashboards/teams/MembersList';

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
        <Route path="teams-creator/create-team" element={<CreateTeam />} />
        <Route
          path="teams-creator/habits-creator/:teamId"
          element={<HabitsList />}
        />
        <Route
          path="teams-creator/habits-creator/:teamId/create"
          element={<CreateHabit />}
        />
        <Route
          path="teams-creator/habits-creator/:teamId/logs/:habitId"
          element={<HabitLogs />}
        />
        <Route path="teams-creator/chat/:teamId" element={<TeamsPanel />} />
        <Route
          path="teams-creator/teams/:teamId/members"
          element={<MembersList />}
        />
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
        <Route path="teams-member/join-team" element={<JoinTeams />} />
        <Route
          path="teams-member/habits-member/:teamId"
          element={<HabitsList />}
        />
        <Route
          path="teams-member/habits-member/:teamId/logs/:habitId"
          element={<HabitLogs />}
        />
        <Route
          path="teams-member/teams/:teamId/members"
          element={<MembersList />}
        />
        <Route path="teams-member/chat/:teamId" element={<TeamsPanel />} />
      </Route>
    </Routes>
  );
}

export default App;
