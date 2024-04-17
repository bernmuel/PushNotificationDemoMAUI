using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common.Api.Internal;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using CommunityToolkit.Mvvm.Messaging;
using Google.Android.Material.Color.Utilities;
using Java.Sql;
using PushNotificationDemoMAUI.Models;
using System.Threading.Tasks.Dataflow;
using static Android.Icu.Text.Edits;
using static JetBrains.Annotations.Async;
using Xamarin.Google.Android.DataTransport.Runtime.Retries;

namespace PushNotificationDemoMAUI;

/// <summary>
/// Represents the main activity of the Android application.
/// </summary>
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    /// <summary>
    /// The channel ID for push notifications.
    /// </summary>
    internal static readonly string CHANNEL_ID = "TestChannel";

    /// <summary>
    /// The notification ID for push notifications.
    /// </summary>
    internal static readonly int NotificationID = 101;


    /// <summary>
    /// Called when the activity is starting. This is where most initialization should go.
    /// </summary>
    /// <param name="savedInstanceState">
    /// <para>If the activity is being re-initialized after previously being shut down</para>
    /// <para>then this Bundle contains the data it most recently supplied in OnSaveInstanceState(Bundle).</para>
    /// <para>Note: Otherwise it is null.</para
    ///</param>
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            RequestPostNotificationPermission();
        }

        // Intent is property of the Activity class

        //TODO: Place the following if block into a separate method called HandleIntentExtras(Intent intent) and call it from OnCreate() method.
        /*               
        Explain next code block
            1.	if (Intent.Extras != null): This line checks if the Intent has any extras. If it doesn't, the code inside the if statement is skipped.
            2.	foreach (var key in Intent.Extras.KeySet()): This line starts a loop that iterates over each key in the extras.
            3.	if (key == "NavigationID"): This line checks if the current key is "NavigationID". If it is, then the code inside the if statement is executed.
            4.	string idValue = Intent.Extras.GetString(key);: This line retrieves the value associated with the "NavigationID" key from the extras and stores it in the idValue variable. This value is a string.
            5.	if (Preferences.ContainsKey("NavigationID")) Preferences.Remove("NavigationId");: This line checks if there is a preference stored with the key "NavigationID". If there is, it removes this preference.
            6.	Preferences.Set("NotificationId", "");: This line sets a preference with the key "NotificationId" and an empty string as the value.
            This code seems to be intended to handle a navigation ID that is passed as an extra in the Intent. However, it doesn't actually use the idValue variable that it retrieves from the extras. 
                    Also, it sets the "NotificationId" preference to an empty string, which might not be what you want. You might want to revise this code to correctly handle the navigation ID.
        */
        if (Intent.Extras != null)
        {
            foreach (var key in Intent.Extras.KeySet())
            {
                if (key == "NavigationID")
                {
                    string idValue = Intent.Extras.GetString(key);
                    if (Preferences.ContainsKey("NavigationID"))
                        Preferences.Remove("NavigationId");
                    Preferences.Set("NotificationId", idValue);         // Set Preferences to NotificationId
                }
            }
        }


        CreateNotificationChannel();

        Firebase.FirebaseApp.InitializeApp(ApplicationContext);
    }

    /// <summary>
    /// Requests the PostNotification permission from the user.
    /// </summary>
    private void RequestPostNotificationPermission()
    {
        if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.PostNotifications) != Permission.Granted)
        {
            ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.PostNotifications }, 1001);
        }
    }

    /// <summary>
    /// Handles the result of the permission request.
    /// </summary>
    /// <param name="requestCode">The request code.</param>
    /// <param name="permissions">The requested permissions.</param>
    /// <param name="grantResults">The grant results.</param>
    public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
    {
        base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        // Handle permission request response here if needed
    }

    /// <summary>
    /// Creates the notification channel for push notifications.
    /// </summary>
    private void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channelName = "Test Notification Channel";
            var channelDescription = "Description of Test Notification Channel";
            var channelId = CHANNEL_ID;

            var channel = new NotificationChannel(channelId, channelName, NotificationImportance.High)
            {
                Description = channelDescription
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }

    /// <summary>
    /// Called when a new intent is received.
    /// </summary>
    /// <param name="intent">The new intent.</param>
    protected override void OnNewIntent(Intent intent)
    {
        base.OnNewIntent(intent);

        var messageBody = intent.GetStringExtra("messageBody");

        var pushNotificationReceivedMessage = new PushNotificationReceived(messageBody);
        WeakReferenceMessenger.Default.Send(pushNotificationReceivedMessage);
    }
}
