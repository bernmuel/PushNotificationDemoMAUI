namespace PushNotificationDemoMAUI;

public partial class NewPage1 : ContentPage
{
	public NewPage1()
	{
		InitializeComponent();
	}

    private void OnGoToMainPageButtonClicked(object sender, EventArgs e)
    {
        // Navigate back to MainPage
        Shell.Current.GoToAsync("//MainPage");
    }
}