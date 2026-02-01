using ZXing.Net.Maui;
using OdooScanner.Services;

namespace OdooScanner.Pages
{
    public partial class ScannerPage : ContentPage
    {
        private readonly OdooApiService _odooApiService;
        private bool _isProcessing = false;

        public ScannerPage(OdooApiService odooApiService)
        {
            InitializeComponent();
            _odooApiService = odooApiService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Check camera permissions
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission Denied", 
                        "Camera permission is required for barcode scanning", 
                        "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }
            }

            CameraView.IsDetecting = true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            CameraView.IsDetecting = false;
        }

        private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            if (_isProcessing || e.Results.Length == 0)
                return;

            _isProcessing = true;

            var barcode = e.Results[0].Value;

            // Update UI on main thread
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                BarcodeLabel.Text = barcode;
                ValidationLabel.IsVisible = true;
                ValidationLabel.Text = "Validating...";

                // Validate barcode with Odoo API
                var isValid = await _odooApiService.ValidateProductBarcodeAsync(barcode);

                if (isValid)
                {
                    ValidationLabel.Text = "✓ Valid Product";
                    ValidationLabel.TextColor = Colors.LightGreen;
                }
                else
                {
                    ValidationLabel.Text = "✗ Product Not Found";
                    ValidationLabel.TextColor = Colors.Red;
                }

                // Wait a bit before allowing next scan
                await Task.Delay(2000);
                _isProcessing = false;
            });
        }

        private void OnToggleLightClicked(object sender, EventArgs e)
        {
            CameraView.IsTorchOn = !CameraView.IsTorchOn;
        }

        private async void OnViewPickingsClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//PickingsPage");
        }
    }
}
