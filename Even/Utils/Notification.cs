using MonkeNotificationLib;
namespace Even.Utils;

public static class Notification
{
    private static bool NotificationsEnabled => Settings.IsEnabled(Settings.Keys.Notifications);

    /// <summary>
    /// A wrapper to show a notification via MonkeNotificationLib
    /// </summary>
    /// <param name="message">Message to display</param>
    /// <param name="durationSeconds">How long the notification stays on screen</param>
    /// <param name="playSound">Whether to play a notification sound</param>
    /// <param name="vibrate">Whether to trigger hand vibrations</param>
    ///
    public static void Show(string message, float durationSeconds = 0.6f, bool playSound = false, bool vibrate = false)
    {
        if (!NotificationsEnabled)
            return;
        
        NotificationController.AppendMessage(Plugin.Alias, message, false, durationSeconds);

        if (playSound)
        {
            Audio.PlaySound("notification.wav", 1.3f);
        }

        if (!vibrate) return;
        GorillaTagger.Instance.StartVibration(true, 0.1f, 0.1f);
        GorillaTagger.Instance.StartVibration(false, 0.1f, 0.1f);
    }
}