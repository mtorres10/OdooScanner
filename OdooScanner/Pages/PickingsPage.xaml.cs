using OdooScanner.Services;
using OdooScanner.Models;

namespace OdooScanner.Pages
{
    public partial class PickingsPage : ContentPage
    {
        private readonly OdooApiService _odooApiService;

        public PickingsPage(OdooApiService odooApiService)
        {
            InitializeComponent();
            _odooApiService = odooApiService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadPickingsAsync();
        }

        private async Task LoadPickingsAsync()
        {
            LoadingIndicator.IsRunning = true;
            LoadingIndicator.IsVisible = true;

            try
            {
                var pickings = await _odooApiService.GetStockPickingsAsync("assigned");
                PickingsCollectionView.ItemsSource = pickings;

                if (pickings.Count == 0)
                {
                    await DisplayAlert("Info", "No stock pickings found", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load pickings: {ex.Message}", "OK");
            }
            finally
            {
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
            }
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadPickingsAsync();
        }
    }
}
