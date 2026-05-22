import { useEffect, useRef } from 'react';
import { getUserReminders } from '../api/teamsApi';

export const useHabitReminders = () => {
  // We use a Set to remember which reminders we already fired today
  // so we don't spam the user 60 times during that one minute!
  const notifiedReminders = useRef<Set<string>>(new Set());

  useEffect(() => {
    // 1. Ask the browser for permission to send Push Notifications
    if ('Notification' in window && Notification.permission === 'default') {
      Notification.requestPermission();
    }

    const checkTimeAndNotify = async () => {
      try {
        // 2. Fetch the user's active reminders
        const reminders = await getUserReminders();
        const activeReminders = reminders.filter((r) => r.enabled);

        // 3. Get the real-world current time in "HH:mm" format (e.g., "14:30")
        const now = new Date();
        const currentHours = now.getHours().toString().padStart(2, '0');
        const currentMinutes = now.getMinutes().toString().padStart(2, '0');
        const currentTimeStr = `${currentHours}:${currentMinutes}`;

        // Reset our tracker at midnight so reminders work again tomorrow
        if (currentTimeStr === '00:00') {
          notifiedReminders.current.clear();
        }

        // 4. Compare real time to reminder times
        activeReminders.forEach((reminder) => {
          // NOTE: This assumes your backend sends reminderTime as "HH:mm" (e.g., "09:00" or "15:45")
          if (reminder.reminderTime === currentTimeStr) {
            // Create a unique key so we only notify once per day for this habit
            const notificationKey = `${reminder.habitId}-${currentTimeStr}`;

            if (!notifiedReminders.current.has(notificationKey)) {
              // 5. Fire the notification!
              if (
                'Notification' in window &&
                Notification.permission === 'granted'
              ) {
                // Native Browser Notification (works even if tab is in background)
                new Notification(`⏰ Time for: ${reminder.habitName}`, {
                  body: reminder.teamName
                    ? `Team: ${reminder.teamName}`
                    : 'Time to log your progress!',
                });
              } else {
                // Fallback: Standard browser alert
                alert(
                  `⏰ Reminder: It's time for your habit: ${reminder.habitName}!`
                );
              }

              // Mark it as fired so it doesn't ring again
              notifiedReminders.current.add(notificationKey);
            }
          }
        });
      } catch (err) {
        console.error('Background reminder check failed:', err);
      }
    };

    // Run once immediately on load, then run every 60,000 milliseconds (1 minute)
    checkTimeAndNotify();
    const interval = setInterval(checkTimeAndNotify, 60000);

    // Cleanup the timer if the user logs out or closes the app
    return () => clearInterval(interval);
  }, []);
};
