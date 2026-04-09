import { create } from 'zustand';

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
  addNotification: (
    notif: Omit<NotificationItem, 'id' | 'date' | 'isRead'>
  ) => void;
  markAllAsRead: () => void;

  markAsRead: (id: string) => void;
  clearNotifications: () => void;
}

export const useNotificationStore = create<NotificationStore>((set) => ({
  notifications: [
    {
      id: 'welcome-1',
      type: 'info',
      title: 'Welcome to HabitHub!',
      message: 'Keep your account secure by checking your active sessions.',
      date: 'Just now',
      isRead: false,
    },
  ],

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
      notifications: state.notifications.map((n) => ({ ...n, isRead: true })),
    })),

  clearNotifications: () => set({ notifications: [] }),
}));
