using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushNotificationDemoMAUI.Models
{


    /// <summary>
    /// Represents a request to send a push Notification to a set of devices.
    /// </summary>
    /// <remarks>
    /// This class encapsulates the necessary information required to send a Notification<br/>
    /// through a push Notification service, such as Firebase Cloud Messaging (FCM).<br/>
    /// It includes the target devices, the Notification message details, and any additional<br/>
    /// Data to be sent with the Notification.
    /// </remarks>
    public class PushNotificationRequest
    {
        /// <summary>
        /// Gets or sets a list of device tokens identifying the target devices for the Notification.
        /// </summary>
        public List<string> Registration_ids { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the content of the Notification message.
        /// </summary>
        public NotificationMessageBody? Notification { get; set; }

        /// <summary>
        /// Gets or sets additional Data accompanying the Notification.
        /// </summary>
        public object? Data { get; set; }
    }

    /// <summary>
    /// Represents the body of a Notification message, including its title and textual content.
    /// </summary>
    /// <remarks>
    /// This class is used to specify the main content of a push Notification, which is displayed
    /// to the user. It's part of the <see cref="PushNotificationRequest"/> class.
    /// </remarks>
    public class NotificationMessageBody
    {
        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        /// <value>A string representing the title of the notification.<br/> 
        /// This is typically shown in bold above the body text.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the body text of the notification.
        /// </summary>
        /// <value>A string representing the main content of the notification. This is the message text shown to the user.</value>
        public string Body { get; set; }
    }
}
