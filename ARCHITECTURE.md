# OdooScanner Architecture

## Application Flow

```
┌─────────────────────────────────────────────────────────────┐
│                        User Launch App                       │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           ▼
                  ┌────────────────┐
                  │   App.xaml.cs  │
                  │  Check Auth    │
                  └────────┬───────┘
                           │
              ┌────────────┴────────────┐
              │                         │
              ▼                         ▼
     ┌────────────────┐      ┌─────────────────┐
     │   LoginPage    │      │    AppShell     │
     │ (Not Auth'd)   │      │  (Authenticated)│
     └────────┬───────┘      └────────┬────────┘
              │                       │
              │ Login Success         │
              └───────────┬───────────┘
                          │
                          ▼
                ┌──────────────────┐
                │     AppShell     │
                │   (TabBar Nav)   │
                └────────┬─────────┘
                         │
           ┌─────────────┼─────────────┐
           │             │             │
           ▼             ▼             ▼
    ┌──────────┐  ┌─────────────┐  ┌──────────────┐
    │MainPage  │  │ ScannerPage │  │ PickingsPage │
    │(Welcome) │  │   (Camera)  │  │    (List)    │
    └──────────┘  └──────┬──────┘  └──────┬───────┘
                         │                 │
                         │                 │
                         └────────┬────────┘
                                  │
                                  ▼
                        ┌──────────────────┐
                        │ OdooApiService   │
                        │  (Singleton)     │
                        └─────────┬────────┘
                                  │
                                  ▼
                         ┌─────────────────┐
                         │   Odoo Server   │
                         │   (JSON-RPC)    │
                         └─────────────────┘
```

## Component Interactions

```
┌───────────────────────────────────────────────────────────────┐
│                        ScannerPage                             │
│  ┌──────────────────────────────────────────────────────┐    │
│  │         ZXing CameraBarcodeReaderView                │    │
│  │         (Barcode Detection)                          │    │
│  └───────────────────────┬──────────────────────────────┘    │
│                          │ OnBarcodesDetected                 │
│                          ▼                                     │
│  ┌──────────────────────────────────────────────────────┐    │
│  │         Process Barcode Value                        │    │
│  └───────────────────────┬──────────────────────────────┘    │
│                          │                                     │
│                          ▼                                     │
│  ┌──────────────────────────────────────────────────────┐    │
│  │    OdooApiService.ValidateProductBarcodeAsync()      │    │
│  └───────────────────────┬──────────────────────────────┘    │
│                          │                                     │
│              ┌───────────┴───────────┐                        │
│              ▼                       ▼                        │
│      ┌──────────────┐        ┌──────────────┐               │
│      │ Valid (✓)    │        │ Invalid (✗)  │               │
│      │ Green Text   │        │  Red Text    │               │
│      └──────────────┘        └──────────────┘               │
└───────────────────────────────────────────────────────────────┘
```

## Data Models

```
┌──────────────────────┐
│  OdooCredentials     │
├──────────────────────┤
│ + Url: string        │
│ + Database: string   │
│ + Username: string   │
│ + Password: string   │
└──────────┬───────────┘
           │
           │ Used by
           ▼
┌──────────────────────────┐
│   OdooApiService         │
├──────────────────────────┤
│ - _httpClient            │
│ - _sessionId             │
│ - _userId                │
│ - _credentials           │
├──────────────────────────┤
│ + AuthenticateAsync()    │
│ + GetStockPickingsAsync()│
│ + ValidateProductBarcode()│
│ + UpdateStockMoveQty()   │
│ + IsAuthenticated        │
└──────────┬───────────────┘
           │
           │ Returns
           ▼
┌──────────────────────┐     ┌──────────────────────┐
│ OdooAuthResponse     │     │   StockPicking       │
├──────────────────────┤     ├──────────────────────┤
│ + UserId             │     │ + Id                 │
│ + SessionId          │     │ + Name               │
│ + IsSuccess          │     │ + State              │
│ + ErrorMessage       │     │ + PickingTypeId      │
└──────────────────────┘     │ + ScheduledDate      │
                              │ + Origin             │
                              └──────────────────────┘
```

## Odoo API Communication Flow

```
┌────────────────┐
│  MAUI App      │
└────────┬───────┘
         │
         │ 1. AuthenticateAsync
         ▼
┌────────────────────────────┐
│  POST /web/session/        │
│       authenticate         │
├────────────────────────────┤
│  Body: {                   │
│    jsonrpc: "2.0",         │
│    method: "call",         │
│    params: {               │
│      db: "...",            │
│      login: "...",         │
│      password: "..."       │
│    }                       │
│  }                         │
└───────────┬────────────────┘
            │
            │ Response
            ▼
┌────────────────────────────┐
│  { result: {               │
│      uid: 123,             │
│      session_id: "..."     │
│    }                       │
│  }                         │
└────────────────────────────┘
            │
            │ 2. GetStockPickingsAsync
            ▼
┌────────────────────────────┐
│  POST /web/dataset/        │
│       search_read          │
├────────────────────────────┤
│  Body: {                   │
│    model: "stock.picking", │
│    fields: [...],          │
│    domain: [               │
│      ["state", "=",        │
│       "assigned"]          │
│    ]                       │
│  }                         │
└───────────┬────────────────┘
            │
            │ Response
            ▼
┌────────────────────────────┐
│  { result: {               │
│      records: [            │
│        { id: 1, name: ...},│
│        { id: 2, name: ...} │
│      ]                     │
│    }                       │
│  }                         │
└────────────────────────────┘
            │
            │ 3. ValidateProductBarcodeAsync
            ▼
┌────────────────────────────┐
│  POST /web/dataset/        │
│       search_read          │
├────────────────────────────┤
│  Body: {                   │
│    model: "product.product"│
│    fields: ["id", "name"], │
│    domain: [               │
│      ["barcode", "=",      │
│       "123456"]            │
│    ]                       │
│  }                         │
└───────────┬────────────────┘
            │
            │ Response
            ▼
┌────────────────────────────┐
│  { result: {               │
│      records: [            │
│        { id: 1,            │
│          name: "Product",  │
│          barcode: "123456" │
│        }                   │
│      ]                     │
│    }                       │
│  }                         │
└────────────────────────────┘
```

## Dependency Injection Setup

```
┌─────────────────────────┐
│   MauiProgram.cs        │
├─────────────────────────┤
│ CreateMauiApp()         │
│   .UseMauiApp<App>()    │
│   .UseBarcodeReader()   │
│                         │
│ Services:               │
│   AddSingleton<         │
│     OdooApiService>     │
│                         │
│ Pages:                  │
│   AddTransient<         │
│     LoginPage>          │
│   AddTransient<         │
│     ScannerPage>        │
│   AddTransient<         │
│     PickingsPage>       │
│   AddTransient<         │
│     MainPage>           │
└─────────────────────────┘
         │
         │ Injected into
         ▼
┌─────────────────────────┐
│  Page Constructors      │
├─────────────────────────┤
│ LoginPage(              │
│   OdooApiService)       │
│                         │
│ ScannerPage(            │
│   OdooApiService)       │
│                         │
│ PickingsPage(           │
│   OdooApiService)       │
└─────────────────────────┘
```

## Security & Permissions

```
┌──────────────────────────────┐
│   AndroidManifest.xml        │
├──────────────────────────────┤
│ Permissions:                 │
│   • CAMERA                   │
│   • FLASHLIGHT               │
│   • INTERNET                 │
│   • ACCESS_NETWORK_STATE     │
│                              │
│ Features:                    │
│   • camera (optional)        │
│   • camera.autofocus         │
│     (optional)               │
└──────────────────────────────┘
         │
         │ Runtime Check
         ▼
┌──────────────────────────────┐
│   ScannerPage.OnAppearing()  │
├──────────────────────────────┤
│ 1. CheckStatusAsync<         │
│      Permissions.Camera>     │
│                              │
│ 2. If not granted:           │
│    RequestAsync<             │
│      Permissions.Camera>     │
│                              │
│ 3. If still denied:          │
│    Show error & navigate     │
│    back                      │
└──────────────────────────────┘
```

## Build & Deployment Pipeline

```
┌──────────────────┐
│  Source Code     │
└────────┬─────────┘
         │
         │ dotnet restore
         ▼
┌──────────────────┐
│ Restore NuGet    │
│  Packages        │
└────────┬─────────┘
         │
         │ dotnet build
         ▼
┌──────────────────┐
│  Compile C#      │
│  Process XAML    │
└────────┬─────────┘
         │
         │ Link Dependencies
         ▼
┌──────────────────┐
│  Generate APK    │
└────────┬─────────┘
         │
         │ Sign (if Release)
         ▼
┌──────────────────┐
│ Deploy to Device │
└──────────────────┘
```
