using OdooScanner.Pages;

namespace OdooScanner;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		// Register routes for navigation
		Routing.RegisterRoute("LoginPage", typeof(LoginPage));
		Routing.RegisterRoute("ScannerPage", typeof(ScannerPage));
		Routing.RegisterRoute("PickingsPage", typeof(PickingsPage));
	}
}
