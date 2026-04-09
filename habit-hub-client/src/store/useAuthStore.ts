import { create } from 'zustand';

export type UserRole = 'Member' | 'Creator';

interface AuthState {
  userName: string | null;
  sessionId: string | null;
  role: UserRole | null;
  isAuth: boolean;
  login: (name: string, sid: string, role: UserRole) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  userName: localStorage.getItem('userName'),
  sessionId: localStorage.getItem('sessionId'),
  role: (localStorage.getItem('role') as UserRole) || null,
  isAuth: !!localStorage.getItem('sessionId'),

  login: (name, sid, role) => {
    localStorage.setItem('userName', name);
    localStorage.setItem('sessionId', sid);
    localStorage.setItem('role', role);
    set({ userName: name, sessionId: sid, role, isAuth: true });
  },

  logout: () => {
    localStorage.removeItem('userName');
    localStorage.removeItem('sessionId');
    localStorage.removeItem('role');
    set({ userName: null, sessionId: null, role: null, isAuth: false });
  },
}));
