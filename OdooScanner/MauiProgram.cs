using Microsoft.Extensions.Logging;
using OdooScanner.Services;
using OdooScanner.Pages;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace OdooScanner;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseBarcodeReader()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Register services
		builder.Services.AddSingleton<OdooApiService>();

		// Register pages
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<ScannerPage>();
		builder.Services.AddTransient<PickingsPage>();
		builder.Services.AddTransient<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
