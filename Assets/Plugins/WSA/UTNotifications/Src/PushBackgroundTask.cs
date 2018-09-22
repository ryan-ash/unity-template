#if WSA_PLUGIN

using System.Collections.Generic;
using System;
using Windows.ApplicationModel.Background;
using Windows.Networking.PushNotifications;

namespace UTNotifications.WSA
{
    public sealed class PushBackgroundTask : IBackgroundTask
    {
    //public
        public static void Register()
        {
            string taskName = typeof(PushBackgroundTask).FullName.ToLower();

            if (NotificationTools.IsBackgroundTaskRegistered(taskName))
            {
                return;
            }

            BackgroundTaskBuilder builder = new BackgroundTaskBuilder();

            builder.TaskEntryPoint = typeof(PushBackgroundTask).FullName;
            builder.Name = taskName;
            builder.SetTrigger(new PushNotificationTrigger());

            builder.Register();
        }

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            RawNotification notification = (RawNotification)taskInstance.TriggerDetails;
            NotificationTools.PushNotificationReceived(notification.Content);
        }
    }
}
#endif
