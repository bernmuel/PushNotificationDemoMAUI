using Android.App;
using Android.Content;
using AndroidX.Core.App;
using CommunityToolkit.Mvvm.Messaging;
using Firebase.Messaging;
using Javax.Annotation.Meta;
using PushNotificationDemoMAUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PushNotificationDemoMAUI.Platforms.Android.Services
{
    /// <summary>
    /// A service that extends FirebaseMessagingService to handle the receipt of messages from Firebase Cloud Messaging (FCM).
    /// </summary>
    /// <remarks>
    /// The two attributes decorating the FirebaseService class are used to configure how the service interacts<br/>
    /// with the Android system and other applications.<br/>
    /// This service is responsible for handling incoming push notifications from FCM. It overrides the <c>OnNewToken</c> <br/>
    /// method to deal with token generation or refresh scenarios and the <c>OnMessageReceived</c> method to handle the<br/> 
    /// receipt of messages. The service ensures that messages are appropriately processed and displayed as notifications <br/>
    /// within the Android system.
    /// </remarks>
    [Service(Exported = true)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseService : FirebaseMessagingService
    {
        public FirebaseService()
        {
                
        }

        /// <summary>
        /// Invoked when Firebase Cloud Messaging generates a new token for the instance or when an existing token is refreshed.<br/>
        /// A new token is generated when the app is installed on a new device or when the user uninstalls and reinstalls the app.<br/>
        /// </summary>
        /// <param name="token">The new token for the instance.</param>
        /// <remarks>
        /// This method stores the new token in the application's preferences, allowing it to be accessed across the app. 
        /// The token is essential for sending messages to the device from your server.
        /// </remarks>
        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);
            //Console.WriteLine(token);

            if (Preferences.ContainsKey("DeviceToken"))
            {
                Preferences.Remove("DeviceToken");
            }
            Preferences.Set("DeviceToken", token);
        }

        /// <summary>
        /// Called when a message is received from Firebase Cloud Messaging.
        /// </summary>
        /// <param name="message">The received message, containing both notification and data payloads.</param>
        /// <remarks>
        /// This method processes the received message, extracting both notification and data components if available, 
        /// and triggers a local notification to display the message content to the user.
        /// </remarks>
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            //Console.WriteLine(message.GetNotification().Body);

            // The following Notification object represents the Notification payload received from Firebase Cloud Messaging (FCM).
            // When your app receives a message from FCM that contains both notiication and Data payloads,
            // the 'RemoteMessage' object encapuslates this information
            // and the 'message.GetNotification()' method extracts the Notification part of the message.
            RemoteMessage.Notification notification = message.GetNotification();

            SendNotification(notification.Body, notification.Title, message.Data);

            // Send a PushNotificationReceived message
            var pushNotificationReceivedMessage = new PushNotificationReceived(notification.Body);

            //WeakReferenceMessenger.Default.Send(pushNotificationReceivedMessage);
        }




        /// <summary>
        /// Sends a push Notification to the device using specified message body, title, and additional Data.
        /// </summary>
        /// <param name="messageBody">The body text of the Notification. This is the main content/message shown to the user.</param>
        /// <param name="title">The title of the Notification. Appears in bold at the top of the Notification and summarizes the content.</param>
        /// <param name="data">Additional Data for the Notification. An IDictionary&lt;string, string&gt;<br/>
        /// that can be used by the app once the Notification is interacted with, but is not directly displayed in the Notification.<br/> 
        /// This Data is added as extras to the intent that launches the MainActivity,<br/> 
        /// allowing the application to respond accordingly when the Notification is tapped.</param>
        /// <remarks>
        /// Initializes a NotificationCompat.Builder object with the current context and a specific channel ID (set in MainActivity class).<br/><br/>
        /// Sets the Notification's title, small icon, content text, channel ID, and priority utilizing method chaining as a fluent interface.<br/>
        /// This approach allows for setting multiple properties on the Notification builder in a single, readable sequence of method calls.<br/>
        /// Each method call in the "chain", modifies the builder object and returns a reference to the same builder,<br/> 
        /// enabling the next property to be set without needing to reference the builder again.<br/><br/>
        /// Utilizes NotificationManagerCompat for compatibility across different versions of Android to display the Notification.<br/><br/>
        /// Prepares an Intent for MainActivity, attaching any additional Data from the IDictionary parameter as extras.<br/> 
        /// This Data can then be retrieved by MainActivity when the Notification is tapped,<br/> 
        /// facilitating context-aware app behavior based on the Notification interaction.<br/><br/>
        /// A PendingIntent is created and set on the Notification,<br/> 
        /// allowing the Notification to open MainActivity with the included extras when the Notification is tapped.<br/><br/>
        /// It may be more appropriate to use a NotificationCompat.Builder object to create a Notification,<br/> 
        /// rather than a Notification.Builder object, as the NotificationCompat.Builder class provides compatibility for older versions of Android.<br/><br/>
        /// The method name "ShowNotification" may be more appropriate than "SendNotification" as the Notification is displayed to the user,<br/> 
        /// not sent to the device. However, this naming could depend on the context in which the method is used,<br/> 
        /// especially if the intent is to emphasize the action of sending the Notification from a backend service.<br/><br/>
        /// </remarks>
        private void SendNotification(string messageBody, string title, IDictionary<string, string> data)
        {

            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop | ActivityFlags.SingleTop);   // from GitHub copilot

            //intent.AddFlags(ActivityFlags.ClearTop);
            
            foreach (var key in data.Keys)
            {
                string value = data[key];
                intent.PutExtra(key, value);
                Preferences.Set(key, value);
            }

            //  A PendingIntent provides a way to pass a pre-configured Intent to another application or components of the Android system,
            //  allowing that Intent to be executed as if your application itself was performing the action,
            //  even if your app is not running at the moment the Intent is executed.
            // The next line of code creates a PendingIntent that is used to execute an Intent at a later time,
            // in this case, when a user interacts with the Notification.
            //PendingIntent? pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.OneShot);
            // The previous line caused the following error: 
            /*
                Java.Lang.IllegalArgumentException
                Message=com.companyname.pushnotificationdemomaui: Targeting S+ (version 31 and above) requires that one of FLAG_IMMUTABLE or FLAG_MUTABLE be specified when creating a PendingIntent.
                Strongly consider using FLAG_IMMUTABLE, only use FLAG_MUTABLE if some functionality depends on the PendingIntent being mutable, e.g. if it needs to be used with inline replies or bubbles.
            */


            //        var pendingIntent = PendingIntent.GetActivity(this,
            //MainActivity.NotificationID, intent, PendingIntentFlags.OneShot);  // original in Pranesh video
            // replace the previous line with the following lines of code ??

            //PendingIntentFlags.Immutable is required to target Android 12 (API level 31) and above
            var pendingIntent = PendingIntent.GetActivity(this,
                MainActivity.NotificationID, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);


            //.SetSmallIcon(Resource.Mipmap.appicon)
            //.SetSmallIcon(Resource.Drawable.appiconfg)

            var notificationBuilder = new NotificationCompat.Builder(this, MainActivity.CHANNEL_ID)
                                .SetContentTitle(title)
                                .SetSmallIcon(Resource.Mipmap.appicon)
                                .SetContentText(messageBody)
                                .SetChannelId(MainActivity.CHANNEL_ID)
                                .SetContentIntent(pendingIntent)
                                .SetPriority((int)NotificationPriority.High); 
                                //.SetPriority(2);       // not required

            var notificationManager = NotificationManagerCompat.From(this);

            //the following Notification object is created locally within the Android app
            //and is meant to configure and display local notifications on the device.



            //notificationManager.Notify(MainActivity.NotificationID, notificationBuilder.Build());       //(video 19:06)
            // replace the previous line with the following lines of code ??

            Notification notification = notificationBuilder.Build();
            notificationManager.Notify(MainActivity.NotificationID, notification);
            //Results in a sound and a notification on the "Notification Panel" of the device
            //Note: Tapping on the notification results in the OnNewIntent method
            //being called in the MainActivity class


        }
    }
}
