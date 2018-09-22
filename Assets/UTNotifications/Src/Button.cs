using System.Collections.Generic;

namespace UTNotifications
{
    /// <summary>
    /// A custom notification button.
    /// </summary>
    public class Button
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UTNotifications.Button"/> class.
        /// </summary>
        /// <param name="title">The button title.</param>
        /// <param name="userData">(Optional) Custom user data to be stored in the argument of a Manager.OnNotificationClicked event handler when the button is clicked.</param>
        public Button(string title, IDictionary<string, string> userData = null)
        {
            this.title = title;
            this.userData = userData;
        }

        /// <summary>
        /// The button title.
        /// </summary>
        public string title;

        /// <summary>
        /// (Optional) Custom user data to be stored in the argument of a Manager.OnNotificationClicked event handler when the button is clicked.
        /// <seealso cref="UTNotifications.ReceivedNotification"/>
        /// <seealso cref="UTNotifications.Manager.OnNotificationClicked"/>
        /// </summary>
        public IDictionary<string, string> userData;
    }
}