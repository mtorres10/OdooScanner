using OdooScanner.Services;

namespace OdooScanner;

public partial class MainPage : ContentPage
{
	private readonly OdooApiService _odooApiService;

	public MainPage(OdooApiService odooApiService)
	{
		InitializeComponent();
		_odooApiService = odooApiService;
	}
}
