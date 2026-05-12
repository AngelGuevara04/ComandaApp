using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using ComandaApp.Pages;
using ComandaApp.PageModels;

namespace ComandaApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<AdminDashboardPage>();
        builder.Services.AddSingleton<AdminDashboardPageModel>();

        builder.Services.AddTransient<TableManagementPage>();
        builder.Services.AddTransient<TableManagementPageModel>();

        builder.Services.AddTransient<BusinessConfigPage>();
        builder.Services.AddTransient<BusinessConfigPageModel>();

        builder.Services.AddTransient<KitchenDashboardPage>();
        builder.Services.AddTransient<KitchenDashboardPageModel>();

        // ¡Inyectamos el nuevo módulo de Menú!
        builder.Services.AddTransient<MenuManagementPage>();
        builder.Services.AddTransient<MenuManagementPageModel>();

        builder.Services.AddTransient<DeviceManagementPage>();
        builder.Services.AddTransient<DeviceManagementPageModel>();

        builder.Services.AddTransient<AddDevicePage>();
        builder.Services.AddTransient<AddDevicePageModel>();

        return builder.Build();
    }
}