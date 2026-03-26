import { create } from 'zustand';

interface AuthState {
  userName: string | null;
  sessionId: string | null;
  isAuth: boolean;
  login: (name: string, sid: string) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  userName: localStorage.getItem('userName'),
  sessionId: localStorage.getItem('sessionId'),
  isAuth: !!localStorage.getItem('sessionId'),

  login: (name, sid) => {
    localStorage.setItem('userName', name);
    localStorage.setItem('sessionId', sid);
    set({ userName: name, sessionId: sid, isAuth: true });
  },

  logout: () => {
    localStorage.removeItem('userName');
    localStorage.removeItem('sessionId');
    set({ userName: null, sessionId: null, isAuth: false });
  },
}));
