
// Pragnesh tutorial on push notifications - original using FCM Legacy API
// https://www.youtube.com/watch?v=rhGM0jxraA8&t=199s
// NOT WORKING

using Android.Gms.Common.Apis;
using Android.Gms.Extensions;
using CommunityToolkit.Mvvm.Messaging;
using Firebase.Messaging;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using PushNotificationDemoMAUI.Models;
using System.Text;
using static Android.InputMethodServices.Keyboard;

namespace PushNotificationDemoMAUI
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private string _deviceToken = "";

        bool _isTokenTestEnabled = false;


        public MainPage()
        {
            InitializeComponent();


            if (Preferences.ContainsKey("DeviceToken"))
            {
                _deviceToken = Preferences.Get("DeviceToken", "");
            }

            // Pragnesh sets this up in Part 2 of the video (before he talks about iOS)
            //This registration with WeakReferenceMessenger for push notification messages 
            //is part of setting up the application to respond to incoming notifications
            //by navigating to specific pages.	
            //In this context, the WeakReferenceMessenger is used to send a message containing the notification data
            //from the FirebaseService to the MainPage when a push notification is received;
            // and when such a message is tapped, the MainPage navigates to the appropriate page.
            WeakReferenceMessenger.Default.Register<PushNotificationReceived>(this, (r, m) =>
            {
                string msg = m.Value;

                Device.BeginInvokeOnMainThread(() =>
                {
                    NavigateToPage();
                });

            });


            //NavigateToPage();
             
            ReadFireBaseAdminSdk();
        }



        private async void ReadFireBaseAdminSdk()
        {
            Stream stream = await FileSystem.OpenAppPackageFileAsync("admin_sdk.json");
            StreamReader reader = new StreamReader(stream);

            string jsonContent = reader.ReadToEnd();


            if (FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromJson(jsonContent)
                });
            }
        }

        private static void NavigateToPage()
        {
            if (Preferences.ContainsKey("NavigationID"))
            {
                string id = Preferences.Get("NavigationID", "");
                if (id == "1")
                {
                    AppShell.Current.GoToAsync(nameof(NewPage1));
                }
                if (id == "2")
                {
                    AppShell.Current.GoToAsync(nameof(NewPage2));
                }
                Preferences.Remove("NavigationID");
            }
        }

        /// <summary>
        /// Sends a push Notification to a specified device using Firebase Cloud Messaging (FCM).
        /// using the FCM Legacy API to send a Notification message to the FCM server.
        /// The Legacy API is not longer supported by Firebase after 20240620 and is replaced by the FCM v1 API.
        /// </summary>
        /// <remarks>
        /// <para>This method creates and sends a Notification message to the FCM server by constructing a Notification request<br/>
        /// which is an object of type PushNotificationDemoMAUI.Models.PushNotificationRequest<br/>
        /// containing the Notification title, body, and the target device token(s).</para>
        /// 
        /// <para>Authorization Header (with Legacy API): The method sets an authorization header on the <see cref="System.Net.Http.HttpClient"/> 
        /// instance.<br/> 
        /// This header is crucial for authenticating the request with the FCM server using a server key obtained 
        /// from the Firebase console.<br/> 
        /// The presence of this header ensures that the request comes from a verified source
        /// and prevents unauthorized entities from sending notifications.</para>
        /// 
        /// <para>Note: Instantiating a new <see cref="System.Net.Http.HttpClient"/> for each request is highlighted as a 
        /// potential inefficiency and a source of socket exhaustion.<br/> 
        /// It is advisable to utilize <see cref="System.Net.Http.IHttpClientFactory"/> 
        /// in production environments for efficient management and reuse of <see cref="System.Net.Http.HttpClient"/> instances.</para>
        /// </remarks>
        /// <param name="sender">The source of the event. This parameter is not used.</param>
        /// <param name="e">An <see cref="System.EventArgs"/> that contains the event Data. This parameter is not used.</param>
        /// <example>
        /// The following code shows how to attach this method to a button click event handler.
        /// <code>
        /// myButton.Click += OnClicked;
        /// </code>
        /// </example>
        /// <returns>
        /// This method does not return a value.
        /// </returns>
        /// <exception cref="System.Net.Http.HttpRequestException">Thrown when the request fails due to an underlying issue such as network connectivity, DNS failure, server certificate validation, or timeout.</exception>
        private async void OnCounterClicked(object sender, EventArgs e)
        {
            if (Preferences.ContainsKey("DeviceToken"))
            {
                _deviceToken = Preferences.Get("DeviceToken", "");

               //// for testing purposes only
               if (_isTokenTestEnabled)
               {
                    var deviceToken2 = (string)await Firebase.Messaging.FirebaseMessaging.Instance.GetToken();
                    if (_deviceToken != deviceToken2)
                    {
                        await DisplayAlert("Test Failed", "The device tokens do NOT match", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Test Passed", "The device tokens match", "OK");
                    }
               }


            }
            else
            {

                // Obtain the device token from Firebase Messaging manually
                _deviceToken = (string)await Firebase.Messaging.FirebaseMessaging.Instance.GetToken();


                //OR
                // Display an alert to inform the user that we didn't get the device token from Firebase Messaging
                // quickly enough, and that we should try again.
                //await DisplayAlert("Server did not respond quickly enough", "Please try again", "OK");
                //return;


            }

            var androidNotificationObject = new Dictionary<string, string>();

            //SET THE NAVIGATION ID HERE!!
            androidNotificationObject.Add("NavigationID", "1");
            //TODO Find where this is set in the video
            //Preferences.Set("NavigationID", "1");


            // time (20:45)
            var pushNotificationRequest = new PushNotificationRequest()
            {
                Notification = new NotificationMessageBody()
                {
                    Title = "Notification Title",
                    Body = "Notification Body"
                },
                Data = androidNotificationObject,
                Registration_ids = new List<string>() { _deviceToken } // a list of device tokens to which the Notification will be sent
            };

            var obj = new Message
            {
                Token = _deviceToken,
                Notification = new Notification
                {
                    Title = "My Title",
                    Body = "My Message Body"
                },
                Data = androidNotificationObject,
            };

            try
            {


                var response = await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAsync(obj);
            }
            catch (Exception ex)
            {    
                //Make sure that _deviceToken is not empty
                string message = ex.Message;
               // Console.WriteLine(ex.Message);
            }
            







            //string url = "https://fcm.googleapis.com/fcm/send";
            //////string url = https://fcm.googleapis.com/v1/projects/push-notification-maui-demo/messages:send";   // FCM v1 API URL

            //BAD PRACTICE: HttpClient object should be created once and reused.
            // but this might be a good example for a simple demo.
            //SUGGESTION: Use HttpClientFactory to create HttpClient objects.
            //using (var client = new HttpClient())
            //{
            //    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("key", "AAAAcgVxic8:APA91bHB33rJrLI8e4mLcNPU_mOcy00d5dVX0c2hMpepPxZ1CaCOdELd_Kkpt2ckJSdO9rMHtPNw0zp9IyJlcqpaRM0ElnM7kPl6rGP2J5lnazqe37s9yd6VeJDGbWqOhgxeoJCo151v");

            //    string serilizedRequest = JsonConvert.SerializeObject(pushNotificationRequest);
            //    HttpResponseMessage response = await client.PostAsync(url, new StringContent(serilizedRequest, Encoding.UTF8, "application/json"));
            //    if (response.StatusCode == System.Net.HttpStatusCode.OK)
            //    {
            //        await DisplayAlert("Success", "Notification sent successfully", "OK");   // Pragnesh uses slightly different message
            //    }
            //    else
            //    {
            //        await DisplayAlert("Error", "Error sending Notification", "OK");
            //    }
            //}

        }


        private async void OnCounterClicked_wLegacyAPI(object sender, EventArgs e)
        {
            var androidNotificationObject = new Dictionary<string, string>();
            androidNotificationObject.Add("NavigationID", "1");

            // time (20:45)
            var pushNotificationRequest = new PushNotificationRequest()
            {
                Notification = new NotificationMessageBody()
                {
                    Title = "Notification Title",
                    Body = "Notification Body"
                },
                Registration_ids = new List<string>() { _deviceToken } // could be more than one _deviceToken (a list of device tokens to which the Notification will be sent)
            };

            string url = "https://fcm.googleapis.com/fcm/send";   // FCM Legacy API URL

            //BAD PRACTICE: HttpClient object should be created once and reused.
            // but this might be a good example for a simple demo.
            //SUGGESTION: Use HttpClientFactory to create HttpClient objects.
            using (var client = new HttpClient())
            {
                // Authorization header is required for the FCM Legacy API
                // The server key is obtained from the Firebase console (in the Cloud Messaging tab)   
                // Note: The server key should be stored securely and not hard-coded in the app.
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("key", "<SERVER KEY>");

                string serializedRequest = JsonConvert.SerializeObject(pushNotificationRequest);
                HttpResponseMessage response = await client.PostAsync(url, new StringContent(serializedRequest, Encoding.UTF8, "application/json"));
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    await DisplayAlert("Success", "Notification sent successfully", "OK");   // Pragnesh uses slightly different message
                }
                else
                {
                    await DisplayAlert("Error", "Error sending Notification", "OK");
                }
            }

        }



        // The following code is for testing purposes only

        //private void OnButtonClicked(object sender, EventArgs e)
        //{
        //    _ = DisplayAlert("Alert", "This button does nothing because\nobtaining a new token from FCM\nis not recommended.", "OK");

        //    // Not recommended to obtain a new token from FCM 
        //    // becuase this new token needs to be synced with the server.
        //    // only implement if necessary.
        //    //GetNewTokenAsync();  // To be called in Android environment
        //}
        private void OnTokenTestSwitchToggled(object sender, ToggledEventArgs e)
        {
            // Store the state of the switch in a field
            _isTokenTestEnabled = e.Value;
        }



    }

}
