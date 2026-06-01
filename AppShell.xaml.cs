using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Font = Microsoft.Maui.Font;
using ComandaApp.Pages;

namespace ComandaApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(QrScannerPage), typeof(QrScannerPage));
        Routing.RegisterRoute(nameof(TableManagementPage), typeof(TableManagementPage));
        Routing.RegisterRoute(nameof(BusinessConfigPage), typeof(BusinessConfigPage));
        Routing.RegisterRoute(nameof(KitchenDashboardPage), typeof(KitchenDashboardPage));
        Routing.RegisterRoute(nameof(CajaDashboardPage), typeof(CajaDashboardPage));
        Routing.RegisterRoute(nameof(MenuManagementPage), typeof(MenuManagementPage));
        Routing.RegisterRoute(nameof(DeviceManagementPage), typeof(DeviceManagementPage));
        Routing.RegisterRoute(nameof(AddDevicePage), typeof(AddDevicePage));
        Routing.RegisterRoute(nameof(HistorialPedidosPage), typeof(HistorialPedidosPage));
        Routing.RegisterRoute(nameof(CorteCajaPage), typeof(CorteCajaPage));
    }

    public static async Task DisplaySnackbarAsync(string message)
    {
        var snackbarOptions = new SnackbarOptions
        {
            BackgroundColor = Color.FromArgb("#FF3300"),
            TextColor = Colors.White,
            CornerRadius = new CornerRadius(10),
            Font = Font.SystemFontOfSize(16)
        };

        await Snackbar.Make(message, visualOptions: snackbarOptions).Show();
    }

    public static async Task DisplayToastAsync(string message)
    {
        if (OperatingSystem.IsWindows())
        {
            return;
        }

        await Toast.Make(message, textSize: 16).Show();
    }
}