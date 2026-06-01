using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using ComandaApp.Pages;
using ComandaApp.PageModels;
using ComandaApp.Services;
using ZXing.Net.Maui.Controls;

namespace ComandaApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseBarcodeReader()
            .ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<SupabaseService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<DeviceService>();
        builder.Services.AddSingleton<MenuService>();
        builder.Services.AddSingleton<MesaService>();
        builder.Services.AddSingleton<ConfigService>();
        builder.Services.AddSingleton<OrdenService>();
        builder.Services.AddSingleton<RealtimeService>();

        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<LoginPageModel>();

        builder.Services.AddTransient<QrScannerPage>();

        builder.Services.AddSingleton<AdminDashboardPage>();
        builder.Services.AddSingleton<AdminDashboardPageModel>();

        builder.Services.AddTransient<BusinessConfigPage>();
        builder.Services.AddTransient<BusinessConfigPageModel>();

        builder.Services.AddTransient<MenuManagementPage>();
        builder.Services.AddTransient<MenuManagementPageModel>();

        builder.Services.AddTransient<TableManagementPage>();
        builder.Services.AddTransient<TableManagementPageModel>();

        builder.Services.AddSingleton<DeviceManagementPage>();
        builder.Services.AddSingleton<DeviceManagementPageModel>();

        builder.Services.AddTransient<AddDevicePage>();
        builder.Services.AddTransient<AddDevicePageModel>();

        builder.Services.AddTransient<KitchenDashboardPage>();
        builder.Services.AddTransient<KitchenDashboardPageModel>();

        builder.Services.AddTransient<CajaDashboardPage>();
        builder.Services.AddTransient<CajaDashboardPageModel>();

        builder.Services.AddTransient<ClienteMenuPage>();
        builder.Services.AddTransient<ClienteMenuPageModel>();

        builder.Services.AddTransient<HistorialPedidosPage>();
        builder.Services.AddTransient<HistorialPedidosPageModel>();

        builder.Services.AddTransient<CorteCajaPage>();
        builder.Services.AddTransient<CorteCajaPageModel>();

        return builder.Build();
    }
}