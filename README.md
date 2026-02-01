# OdooScanner

A .NET MAUI application for Android that mimics the functionality of the Odoo barcode scanning module. The app allows users to scan barcodes, authenticate with Odoo, and manage stock operations.

## Features

- **Login Authentication**: Secure login to your Odoo instance using URL, database, username, and password
- **Barcode Scanner**: Real-time barcode scanning using the device camera with ZXing.Net.Maui
- **Product Validation**: Validates scanned barcodes against products in your Odoo database
- **Stock Pickings Management**: View and manage stock picking operations
- **Odoo API Integration**: Full integration with Odoo's JSON-RPC API

## Requirements

- .NET 10.0 SDK or later
- Android device or emulator with camera support
- Odoo server (v14 or later recommended)

## Installation

1. Clone the repository:
```bash
git clone https://github.com/mtorres10/OdooScanner.git
cd OdooScanner
```

2. Install the MAUI workload:
```bash
dotnet workload install maui-android
```

3. Restore NuGet packages:
```bash
cd OdooScanner
dotnet restore
```

4. Build the project:
```bash
dotnet build -f net10.0-android
```

5. Deploy to Android device:
```bash
dotnet build -f net10.0-android -t:Run
```

## Usage

### Login
1. Launch the app
2. Enter your Odoo server details:
   - **Server URL**: Your Odoo instance URL (e.g., https://your-odoo.com)
   - **Database**: Your Odoo database name
   - **Username**: Your Odoo username
   - **Password**: Your Odoo password
3. Tap "Login"

### Scanning Barcodes
1. Navigate to the "Scanner" tab
2. Point the camera at a barcode
3. The app will automatically detect and validate the barcode
4. Validation status is displayed:
   - ✓ Green: Valid product found in Odoo
   - ✗ Red: Product not found

### Managing Stock Pickings
1. Navigate to the "Pickings" tab
2. View all assigned stock pickings
3. Tap "Refresh" to reload the list

## Project Structure

```
OdooScanner/
├── Models/                 # Data models
│   ├── OdooAuthResponse.cs
│   ├── OdooCredentials.cs
│   └── StockPicking.cs
├── Services/              # Business logic
│   └── OdooApiService.cs  # Odoo API integration
├── Pages/                 # UI pages
│   ├── LoginPage.xaml/cs
│   ├── ScannerPage.xaml/cs
│   └── PickingsPage.xaml/cs
├── Platforms/             # Platform-specific code
│   └── Android/
│       └── AndroidManifest.xml
└── Resources/             # Images, fonts, etc.
```

## Dependencies

- **Microsoft.Maui.Controls**: .NET MAUI framework
- **ZXing.Net.Maui**: Barcode scanning library
- **ZXing.Net.Maui.Controls**: Barcode scanning UI controls
- **Newtonsoft.Json**: JSON serialization for API communication

## Permissions

The app requires the following Android permissions:
- `CAMERA`: For barcode scanning
- `FLASHLIGHT`: For camera flash/torch control
- `INTERNET`: For API communication with Odoo
- `ACCESS_NETWORK_STATE`: For network status checking

## API Integration

The app uses Odoo's JSON-RPC API to:
- Authenticate users
- Retrieve stock picking information
- Validate product barcodes
- Update stock move quantities

### Supported Odoo Endpoints
- `/web/session/authenticate`: User authentication
- `/web/dataset/search_read`: Read records from models
- `/web/dataset/call_kw`: Call Odoo model methods

## Development

### Building for Android
```bash
dotnet build -f net10.0-android
```

### Running on Android Device
```bash
dotnet build -f net10.0-android -t:Run
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is open source and available under the MIT License.

## Support

For issues, questions, or contributions, please open an issue on the GitHub repository.