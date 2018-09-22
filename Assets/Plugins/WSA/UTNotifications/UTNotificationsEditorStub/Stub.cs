#if WSA_PLUGIN

using System;
using System.Collections.Generic;

namespace UTNotifications.WSA
{
    public interface IInitializeHandler
    {
        void OnInitializationComplete(string registrationId);
        void OnInitializationError(string error, bool wnsIssue);
    }

    public sealed class NotificationTools
    {
        public static bool Initialize(bool willHandleReceivedNotifications, int startId, bool incrementalId, IInitializeHandler handler, bool pushEnabled, bool dontShowWhenRunning, string titleFieldName, string textFieldName, string userDataFieldName, string notificationProfileFieldName, string idFieldName, string badgeFieldName)
        {
            throw new NotImplementedException();
        }

        public static void PostLocalNotification(string title, string text, int id, IDictionary<string, string> userData, string notificationProfile)
        {
            throw new NotImplementedException();
        }

        public static void ScheduleNotification(int triggerInSeconds, string title, string text, int id, IDictionary<string, string> userData, string notificationProfile)
        {
            throw new NotImplementedException();
        }

        public static void ScheduleNotificationRepeating(int firstTriggerInSeconds, int intervalSeconds, string title, string text, int id, IDictionary<string, string> userData, string notificationProfile)
        {
            throw new NotImplementedException();
        }

        public static void RescheduleRepeating()
        {
            throw new NotImplementedException();
        }

        public static void CancelNotification(int id)
        {
            throw new NotImplementedException();
        }

        public static void CancelAllNotifications()
        {
            throw new NotImplementedException();
        }

        public static void SetNotificationsEnabled(bool enabled)
        {
            throw new NotImplementedException();
        }

        public static bool NotificationsEnabled()
        {
            throw new NotImplementedException();
        }

        public static void EnablePushNotifications(IInitializeHandler handler)
        {
            throw new NotImplementedException();
        }

        public static void DisablePushNotifications()
        {
            throw new NotImplementedException();
        }

        public static void UpdateWhenRunning()
        {
            throw new NotImplementedException();
        }

        public static void HandleReceivedNotifications(string appArguments, out IList<ReceivedNotification> allReceivedNotifications, out ReceivedNotification clickedNotification)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class ReceivedNotification
    {
        public ReceivedNotification(string title, string text, int id, IDictionary<string, string> userData, string notificationProfile, int badgeNumber)
        {
            this.title = title;
            this.text = text;
            this.id = id;
            this.userData = userData;
            this.notificationProfile = notificationProfile;
            this.badgeNumber = badgeNumber;
        }

        public string Title
        {
            get
            {
                return title;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
        }

        public int Id
        {
            get
            {
                return id;
            }
        }

        public IDictionary<string, string> UserData
        {
            get
            {
                return userData;
            }
        }

        public string NotificationProfile
        {
            get
            {
                return notificationProfile;
            }
        }

        public int BadgeNumber
        {
            get
            {
                return badgeNumber;
            }
        }

        private readonly string title;
        private readonly string text;
        private readonly int id;
        private readonly IDictionary<string, string> userData;
        private readonly string notificationProfile;
        private readonly int badgeNumber;
    }
}
#endif