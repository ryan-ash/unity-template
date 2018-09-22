#if !UNITY_EDITOR && UNITY_ANDROID

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UTNotifications
{
    public class ManagerImpl : Manager
    {
    //public
        public override bool Initialize(bool willHandleReceivedNotifications, int startId = 0, bool incrementalId = false)
        {
            m_willHandleReceivedNotifications = willHandleReceivedNotifications;

            bool allowUpdatingGooglePlayIfRequired = false;
            switch (Settings.Instance.AllowUpdatingGooglePlayIfRequired)
            {
            case Settings.GooglePlayUpdatingIfRequiredMode.DISABLED:
                allowUpdatingGooglePlayIfRequired = false;
                break;

            case Settings.GooglePlayUpdatingIfRequiredMode.EVERY_INITIALIZE:
                allowUpdatingGooglePlayIfRequired = true;
                break;

            case Settings.GooglePlayUpdatingIfRequiredMode.ONCE:
                const string prefKey = "_UT_NOTIFICATIONS_GP_UPDATING_WAS_ALLOWED";
                allowUpdatingGooglePlayIfRequired = (PlayerPrefs.GetInt(prefKey, 0) == 0);
                if (allowUpdatingGooglePlayIfRequired)
                {
                    PlayerPrefs.SetInt(prefKey, 1);
                }
                break;
            }

            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    bool success = manager.CallStatic<bool>("initialize", Settings.Instance.PushNotificationsEnabledFirebase, Settings.Instance.PushNotificationsEnabledAmazon, Settings.Instance.FirebaseSenderID, willHandleReceivedNotifications, startId, incrementalId, (int)Settings.Instance.AndroidShowNotificationsMode, Settings.Instance.AndroidRestoreScheduledNotificationsAfterReboot, (int)Settings.Instance.AndroidNotificationsGrouping, Settings.Instance.AndroidShowLatestNotificationOnly, Settings.Instance.PushPayloadTitleFieldName, Settings.Instance.PushPayloadTextFieldName, Settings.Instance.PushPayloadUserDataParentFieldName, Settings.Instance.PushPayloadNotificationProfileFieldName, Settings.Instance.PushPayloadIdFieldName, Settings.Instance.PushPayloadBadgeFieldName, Settings.Instance.PushPayloadButtonsParentName, ProfilesSettingsJson(), allowUpdatingGooglePlayIfRequired);
                    Initialized = success;
                    
                    return success;
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public override void PostLocalNotification(string title, string text, int id, IDictionary<string, string> userData, string notificationProfile, int badgeNumber, ICollection<Button> buttons)
        {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    manager.CallStatic("postNotification", ToBase64(title), ToBase64(text), id, ToBase64(ToString(JsonUtils.ToJson(userData))), notificationProfile, badgeNumber, ToBase64(ToString(JsonUtils.ToJson(buttons))));
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
            }
        }

        public override void ScheduleNotification(int triggerInSeconds, string title, string text, int id, IDictionary<string, string> userData, string notificationProfile, int badgeNumber, ICollection<Button> buttons)
        {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    manager.CallStatic("scheduleNotification", triggerInSeconds, ToBase64(title), ToBase64(text), id, ToBase64(ToString(JsonUtils.ToJson(userData))), notificationProfile, badgeNumber, ToBase64(ToString(JsonUtils.ToJson(buttons))));
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
            }
        }

        public override void ScheduleNotificationRepeating(int firstTriggerInSeconds, int intervalSeconds, string title, string text, int id, IDictionary<string, string> userData, string notificationProfile, int badgeNumber, ICollection<Button> buttons)
        {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    manager.CallStatic("scheduleNotificationRepeating", firstTriggerInSeconds, intervalSeconds, ToBase64(title), ToBase64(text), id, ToBase64(ToString(JsonUtils.ToJson(userData))), notificationProfile, badgeNumber, ToBase64(ToString(JsonUtils.ToJson(buttons))));
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
            }
        }

        public override bool NotificationsEnabled()
        {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    return manager.CallStatic<bool>("notificationsEnabled");
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public override bool NotificationsAllowed()
        {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    return manager.CallStatic<bool>("notificationsAllowed");
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
                return true;
            }
        }

        public override void SetNotificationsEnabled(bool enabled)
        {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    manager.CallStatic("setNotificationsEnabled", enabled);
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
            }
        }

        public override bool PushNotificationsEnabled()
        {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    return manager.CallStatic<bool>("pushNotificationsEnabled") &&
                            ((Settings.Instance.PushNotificationsEnabledFirebase && manager.CallStatic<bool>("fcmProviderAvailable", false)) ||
                            (Settings.Instance.PushNotificationsEnabledAmazon && manager.CallStatic<bool>("admProviderAvailable")));
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public override bool SetPushNotificationsEnabled(bool enabled)
        {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    manager.CallStatic("setPushNotificationsEnabled", enabled);
                    return PushNotificationsEnabled();
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
                return false;
            }
        }

        public override void CancelNotification(int id)
        {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    manager.CallStatic("cancelNotification", id);
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
            }

            HideNotification(id);
        }

        public override void HideNotification(int id)
        {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    manager.CallStatic("hideNotification", id);
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
            }
        }

        public override void HideAllNotifications()
        {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    manager.CallStatic("hideAllNotifications");
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
            }
        }

        public override void CancelAllNotifications()
        {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    manager.CallStatic("cancelAllNotifications");
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
            }
        }

        public override int GetBadge()
        {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    return manager.CallStatic<int>("getBadge");
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
                return 0;
            }
        }
        
        public override void SetBadge(int bandgeNumber)
        {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    manager.CallStatic("setBadge", bandgeNumber);
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
            }
        }

        public void _OnAndroidIdReceived(string providerAndId)
        {
            JSONNode json = JSON.Parse(providerAndId);
            
            if (OnSendRegistrationIdHasSubscribers())
            {
                _OnSendRegistrationId(json[0], json[1]);
            }
        }

    //protected
        protected void LateUpdate()
        {
            m_timeToCheckForIncomingNotifications -= Time.unscaledDeltaTime;
            if (m_timeToCheckForIncomingNotifications > 0)
            {
                return;
            }

            m_timeToCheckForIncomingNotifications = m_timeBetweenCheckingForIncomingNotifications;

            if (OnNotificationClickedHasSubscribers())
            {
                try
                {
                    using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                    {
                        HandleClickedNotification(manager.CallStatic<string>("getClickedNotificationPacked"));
                    }
                }
                catch (AndroidJavaException e)
                {
                    Debug.LogException(e);
                }
            }

            if (m_willHandleReceivedNotifications && OnNotificationsReceivedHasSubscribers())
            {
                try
                {
                    using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                    {
                        HandleReceivedNotifications(manager.CallStatic<string>("getReceivedNotificationsPacked"));
                    }
                }
                catch (AndroidJavaException e)
                {
                    Debug.LogException(e);
                }
            }
        }

        protected void OnApplicationPause(bool paused) {
            try
            {
                using (AndroidJavaClass manager = new AndroidJavaClass("universal.tools.notifications.Manager"))
                {
                    manager.CallStatic("setBackgroundMode", paused);
                }
            }
            catch (AndroidJavaException e)
            {
                Debug.LogException(e);
            }
        }

    //private
        private void HandleClickedNotification(string receivedNotificationPacked)
        {
            if (!string.IsNullOrEmpty(receivedNotificationPacked))
            {
                _OnNotificationClicked(ParseReceivedNotification(JSON.Parse(receivedNotificationPacked)));
            }
        }

        private void HandleReceivedNotifications(string receivedNotificationsPacked)
        {
            if (string.IsNullOrEmpty(receivedNotificationsPacked) || receivedNotificationsPacked == "[]")
            {
                return;
            }

            List<ReceivedNotification> receivedNotifications = new List<ReceivedNotification>();

            JSONNode notificationsList = JSON.Parse(receivedNotificationsPacked);
            for (int i = 0; i < notificationsList.Count; ++i)
            {
                JSONNode json = notificationsList[i];

                ReceivedNotification receivedNotification = ParseReceivedNotification(json);

                //Update out-of-date notifications
                bool updated = false;
                for (int j = 0; j < receivedNotifications.Count; ++j)
                {
                    if (receivedNotifications[j].id == receivedNotification.id)
                    {
                        receivedNotifications[j] = receivedNotification;
                        updated = true;
                        break;
                    }
                }
                if (!updated)
                {
                    receivedNotifications.Add(receivedNotification);
                }
            }

            _OnNotificationsReceived(receivedNotifications);
        }

        private static ReceivedNotification ParseReceivedNotification(JSONNode json)
        {
            string title = json["title"].Value;
            string text = json["text"].Value;
            int id = json["id"].AsInt;
            string notificationProfile = json["notificationProfile"].Value;
            int badgeNumber = json["badgeNumber"].AsInt;

            JSONNode userDataJson = json["userData"];
            if (!(json["buttonIndex"] is JSONLazyCreator) && !(json["buttons"] is JSONLazyCreator))
            {
                JSONArray buttons = json["buttons"].AsArray;
                int buttonIndex = json["buttonIndex"].AsInt;
                if (buttons != null && buttonIndex >= 0 && buttonIndex < buttons.Count)
                {
                    userDataJson = buttons[buttonIndex]["userData"];
                }
            }
            
            Dictionary<string, string> userData;
            if (userDataJson != null && userDataJson.Count > 0)
            {
                userData = new Dictionary<string, string>();
                foreach (KeyValuePair<string, JSONNode> it in (JSONClass)userDataJson)
                {
                    userData.Add(it.Key, it.Value.Value);
                }
            }
            else
            {
                userData = null;
            }
            
            return new ReceivedNotification(title, text, id, userData, notificationProfile, badgeNumber);
        }

        private static string ProfilesSettingsJson()
        {
            JSONArray json = new JSONArray();

            foreach (var it in Settings.Instance.NotificationProfiles)
            {
                JSONClass node = new JSONClass();
                node.Add("id", new JSONData(it.profileName != Settings.DEFAULT_PROFILE_NAME ? it.profileName : Settings.DEFAULT_PROFILE_NAME_INTERNAL));
                node.Add("name", !string.IsNullOrEmpty(it.androidChannelName) ? it.androidChannelName : it.profileName);
                node.Add("description", it.androidChannelDescription ?? "");
                node.Add("high_priority", new JSONData(it.androidHighPriority));
                if (it.colorSpecified)
                {
                    Color32 color32 = it.androidColor;
                    int intColor = color32.a << 24 | color32.r << 16 | color32.g << 8 | color32.b;
                    node.Add("color", new JSONData(intColor));
                }

                json.Add(node);
            }
            
            return json.ToString();
        }

        private static string ToString(object o)
        {
            if (o != null)
            {
                return o.ToString();
            }
            else
            {
                return null;
            }
        }

        private static string ToBase64(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            else
            {
                return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
            }
        }

        private bool m_willHandleReceivedNotifications;
        private const float m_timeBetweenCheckingForIncomingNotifications = 0.5f;
        private float m_timeToCheckForIncomingNotifications = 0;
    }
}
#endif //UNITY_ANDROID