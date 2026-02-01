using Microsoft.Extensions.DependencyInjection;
using OdooScanner.Pages;
using OdooScanner.Services;

namespace OdooScanner;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var odooService = Handler?.MauiContext?.Services.GetService<OdooApiService>();
		
		if (odooService?.IsAuthenticated == true)
		{
			return new Window(new AppShell());
		}
		else
		{
			var loginPage = Handler?.MauiContext?.Services.GetService<LoginPage>();
			return new Window(loginPage ?? new LoginPage(new OdooApiService()));
		}
	}
}