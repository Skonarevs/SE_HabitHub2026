// 1. Добавили иконку Check для кнопки
import { CheckCircle2, AlertCircle, Info, Check } from 'lucide-react';
import { useNotificationStore } from '../../../store/useNotificationStore';

export const Notifications = () => {
  const { notifications, markAllAsRead, markAsRead, clearNotifications } =
    useNotificationStore();

  const getIcon = (type: string) => {
    switch (type) {
      case 'success':
        return <CheckCircle2 className="text-green-500" size={24} />;
      case 'warning':
        return <AlertCircle className="text-amber-500" size={24} />;
      default:
        return <Info className="text-blue-500" size={24} />;
    }
  };

  return (
    <div className="flex flex-col h-full bg-white rounded-3xl p-8 shadow-sm w-full border border-gray-100">
      <div className="mb-8 border-b border-gray-100 pb-4 flex justify-between items-end">
        <div>
          <h2 className="text-2xl font-bold text-gray-900 flex items-center gap-3">
            Notifications
          </h2>
          <p className="text-gray-500 mt-1">
            Stay updated with your team's progress and system alerts.
          </p>
        </div>

        <button
          onClick={markAllAsRead}
          className="text-sm font-semibold text-gray-500 hover:text-gray-900 transition-colors"
        >
          Mark all as read
        </button>
        <button
          onClick={clearNotifications}
          className="text-sm font-semibold text-gray-500 hover:text-gray-900 transition-colors"
        >
          Clear all
        </button>
      </div>

      <div className="space-y-4 overflow-y-auto pr-2">
        {notifications.length === 0 ? (
          <p className="text-gray-500 text-center py-10">
            You're all caught up!
          </p>
        ) : (
          notifications.map((notif) => (
            <div
              key={notif.id}
              className={`group relative flex items-start gap-4 p-5 rounded-2xl border transition-all ${
                notif.isRead
                  ? 'bg-white border-gray-100 opacity-70'
                  : 'bg-gray-50/50 border-gray-200 shadow-sm'
              }`}
            >
              <div className="mt-1">{getIcon(notif.type)}</div>

              <div className="flex-1 pr-20">
                <div className="flex justify-between items-start">
                  <h3
                    className={`font-bold ${notif.isRead ? 'text-gray-700' : 'text-gray-900'}`}
                  >
                    {notif.title}
                  </h3>
                  <span className="text-xs font-medium text-gray-400 whitespace-nowrap ml-4">
                    {notif.date}
                  </span>
                </div>
                <p className="text-sm text-gray-500 mt-1">{notif.message}</p>
              </div>

              {!notif.isRead && (
                <>
                  <div className="w-2.5 h-2.5 bg-blue-500 rounded-full mt-2 group-hover:opacity-0 transition-opacity" />

                  <button
                    onClick={() => markAsRead(notif.id)}
                    className="absolute right-4 top-1/2 -translate-y-1/2 opacity-0 group-hover:opacity-100 transform translate-x-2 group-hover:translate-x-0 bg-white shadow-md border border-gray-100 px-3 py-1.5 rounded-lg text-sm font-semibold text-gray-600 hover:text-green-600 flex items-center gap-2 transition-all duration-200"
                  >
                    <Check size={16} />
                    Mark as read
                  </button>
                </>
              )}
            </div>
          ))
        )}
      </div>
    </div>
  );
};
