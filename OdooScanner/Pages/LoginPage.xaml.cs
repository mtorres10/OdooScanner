using OdooScanner.Models;
using OdooScanner.Services;

namespace OdooScanner.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly OdooApiService _odooApiService;

        public LoginPage(OdooApiService odooApiService)
        {
            InitializeComponent();
            _odooApiService = odooApiService;
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            // Hide previous error
            ErrorLabel.IsVisible = false;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(UrlEntry.Text) ||
                string.IsNullOrWhiteSpace(DatabaseEntry.Text) ||
                string.IsNullOrWhiteSpace(UsernameEntry.Text) ||
                string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlertAsync("Error", "Please fill in all fields", "OK");
                return;
            }

            // Show loading indicator
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;
            LoginButton.IsEnabled = false;

            var credentials = new OdooCredentials
            {
                Url = UrlEntry.Text.Trim(),
                Database = DatabaseEntry.Text.Trim(),
                Username = UsernameEntry.Text.Trim(),
                Password = PasswordEntry.Text
            };

            var response = await _odooApiService.AuthenticateAsync(credentials);

            // Hide loading indicator
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
            LoginButton.IsEnabled = true;

            if (response.IsSuccess)
            {
                // Navigate to main page
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                ErrorLabel.Text = response.ErrorMessage ?? "Login failed";
                ErrorLabel.IsVisible = true;
            }
        }
    }
}
