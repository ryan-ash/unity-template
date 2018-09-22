#if !UNITY_EDITOR && (UNITY_WSA || UNITY_METRO) && !ENABLE_IL2CPP

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UTNotifications.WSA;

namespace UTNotifications
{
    public class ManagerImpl : Manager, IInitializeHandler
    {
    //public
        public override bool Initialize(bool willHandleReceivedNotifications, int startId, bool incrementalId)
        {
            NotificationTools.Initialize(willHandleReceivedNotifications, startId, incrementalId, this, Settings.Instance.PushNotificationsEnabledWindows && pushEnabled, Settings.Instance.WindowsDontShowWhenRunning, Settings.Instance.PushPayloadTitleFieldName, Settings.Instance.PushPayloadTextFieldName, Settings.Instance.PushPayloadUserDataParentFieldName, Settings.Instance.PushPayloadNotificationProfileFieldName, Settings.Instance.PushPayloadIdFieldName, Settings.Instance.PushPayloadBadgeFieldName);
            Initialized = true;
            return true;
        }

        public override void PostLocalNotification(string title, string text, int id, IDictionary<string, string> userData, string notificationProfile, int badgeNumber, ICollection<Button> buttons)
        {
            if (CheckInitialized())
            {
                NotificationTools.PostLocalNotification(title, text, id, userData, notificationProfile);
            }
        }

        public override void ScheduleNotification(int triggerInSeconds, string title, string text, int id, IDictionary<string, string> userData, string notificationProfile, int badgeNumber, ICollection<Button> buttons)
        {
            if (CheckInitialized())
            {
                NotificationTools.ScheduleNotification(triggerInSeconds, title, text, id, userData, notificationProfile);
            }
        }

        public override void ScheduleNotificationRepeating(int firstTriggerInSeconds, int intervalSeconds, string title, string text, int id, IDictionary<string, string> userData, string notificationProfile, int badgeNumber, ICollection<Button> buttons)
        {
            if (CheckInitialized())
            {
                NotificationTools.ScheduleNotificationRepeating(firstTriggerInSeconds, intervalSeconds, title, text, id, userData, notificationProfile);
            }
        }

        public override bool NotificationsEnabled()
        {
            return NotificationTools.NotificationsEnabled();
        }

        public override void SetNotificationsEnabled(bool enabled)
        {
            NotificationTools.SetNotificationsEnabled(enabled);
        }

        public override bool PushNotificationsEnabled()
        {
            return Settings.Instance.PushNotificationsEnabledWindows && NotificationsEnabled() && pushEnabled;
        }

        public override bool NotificationsAllowed()
        {
            //Always allowed on WSA
            return true;
        }

        public override bool SetPushNotificationsEnabled(bool enabled)
        {
            if (enabled != pushEnabled)
            {
                pushEnabled = enabled;

                if (enabled)
                {
                    if (Initialized && Settings.Instance.PushNotificationsEnabledWindows)
                    {
                        NotificationTools.EnablePushNotifications(this);
                    }
                }
                else
                {
                    NotificationTools.DisablePushNotifications();
                }
            }

            return PushNotificationsEnabled();
        }

        public override void CancelNotification(int id)
        {
            if (CheckInitialized())
            {
                NotificationTools.CancelNotification(id);
            }
        }

        public override void CancelAllNotifications()
        {
            if (CheckInitialized())
            {
                NotificationTools.CancelAllNotifications();
            }
        }

        public override void HideNotification(int id)
        {
            NotSupported();
        }

        public override void HideAllNotifications()
        {
            NotSupported();
        }

        public override int GetBadge()
        {
            //Not implemented yet
            NotSupported("badges");
            return 0;
        }

        public override void SetBadge(int bandgeNumber)
        {
            //Not implemented yet
            NotSupported("badges");
        }

        public void OnInitializationComplete(string registrationId)
        {
            m_registrationId = registrationId;
        }

        public void OnInitializationError(string error, bool wnsIssue)
        {
            Debug.LogError(string.Format("UTNotifications: error initializing {0}: {1}", wnsIssue ? "WNS" : "notifications", error));
        }

    //protected
        protected void Update()
        {
            if (!Initialized)
            {
                return;
            }

            if (m_registrationId != null && OnSendRegistrationIdHasSubscribers())
            {
                _OnSendRegistrationId(m_providerName, m_registrationId);
                m_registrationId = null;
            }

            if (Time.time - m_lastTimeUpdated >= m_updateEverySeconds)
            {
                IList<WSA.ReceivedNotification> received;
                WSA.ReceivedNotification clicked;
                NotificationTools.HandleReceivedNotifications(UnityEngine.WSA.Application.arguments, out received, out clicked);
                if (clicked != null && OnNotificationClickedHasSubscribers())
                {
                    _OnNotificationClicked(WSAReceivedNotificationToReceivedNotification(clicked));
                }

                if (received != null && received.Count > 0 && OnNotificationsReceivedHasSubscribers())
                {
                    List<UTNotifications.ReceivedNotification> receivedNotifications = new List<UTNotifications.ReceivedNotification>();
                    foreach (var it in received)
                    {
                        receivedNotifications.Add(WSAReceivedNotificationToReceivedNotification(it));
                    }

                    _OnNotificationsReceived(receivedNotifications);
                }

                NotificationTools.UpdateWhenRunning();
                m_lastTimeUpdated = Time.time;
            }
        }

        private static ReceivedNotification WSAReceivedNotificationToReceivedNotification(WSA.ReceivedNotification it)
        {
            return new UTNotifications.ReceivedNotification(it.Title, it.Text, it.Id, it.UserData, it.NotificationProfile, it.BadgeNumber);
        }

    //private
        private bool pushEnabled
        {
            get
            {
                return PlayerPrefs.GetInt(m_pushEnabledKey, 1) != 0;
            }

            set
            {
                PlayerPrefs.SetInt(m_pushEnabledKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private volatile string m_registrationId = null;
        private float m_lastTimeUpdated = 0;
        private const float m_updateEverySeconds = 2.0f;
        private const string m_providerName = "WNS";
        private const string m_pushEnabledKey = "_UT_NOTIFICATIONS_PUSH_ENABLED";
    }
}
#endif