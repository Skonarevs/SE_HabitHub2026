import { create } from 'zustand';
import { persist } from 'zustand/middleware';

export interface NotificationItem {
  id: string;
  type: 'info' | 'success' | 'warning';
  title: string;
  message: string;
  date: string;
  isRead: boolean;
}

interface NotificationStore {
  notifications: NotificationItem[];
  hasInitialized: boolean;

  addNotification: (
    notif: Omit<NotificationItem, 'id' | 'date' | 'isRead'>
  ) => void;
  markAllAsRead: () => void;
  deleteNotification: (id: string) => void;
  markAsRead: (id: string) => void;
  clearNotifications: () => void;
  initWelcome: () => void;
}

export const useNotificationStore = create<NotificationStore>()(
  persist(
    (set, get) => ({
      notifications: [],
      hasInitialized: false,

      initWelcome: () => {
        const { hasInitialized } = get();

        if (hasInitialized) return;

        set((state) => ({
          notifications: [
            {
              id: crypto.randomUUID(),
              type: 'info',
              title: 'Welcome to HabitHub!',
              message:
                'Keep your account secure by checking your active sessions.',
              date: new Date().toLocaleTimeString([], {
                hour: '2-digit',
                minute: '2-digit',
              }),
              isRead: false,
            },
            ...state.notifications,
          ],
          hasInitialized: true,
        }));
      },

      addNotification: (notif) =>
        set((state) => ({
          notifications: [
            {
              ...notif,
              id: crypto.randomUUID(),
              date: new Date().toLocaleTimeString([], {
                hour: '2-digit',
                minute: '2-digit',
              }),
              isRead: false,
            },
            ...state.notifications,
          ],
        })),

      markAsRead: (id) =>
        set((state) => ({
          notifications: state.notifications.map((n) =>
            n.id === id ? { ...n, isRead: true } : n
          ),
        })),

      markAllAsRead: () =>
        set((state) => ({
          notifications: state.notifications.map((n) => ({
            ...n,
            isRead: true,
          })),
        })),

      clearNotifications: () => set({ notifications: [] }),

      deleteNotification: (id) =>
        set((state) => ({
          notifications: state.notifications.filter((n) => n.id !== id),
        })),
    }),

    {
      name: 'habit-hub-notifications',
    }
  )
);
