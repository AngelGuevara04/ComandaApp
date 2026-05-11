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

        // 1. Registro de Dashboard Principal (Solo una vez)
        builder.Services.AddSingleton<AdminDashboardPage>();
        builder.Services.AddSingleton<AdminDashboardPageModel>();

        // 2. Registro de Páginas Secundarias de Administración
        // Asegúrate de crear los archivos correspondientes en Pages/ y PageModels/
        builder.Services.AddTransientWithShellRoute<TableManagementPage, TableManagementPageModel>("table_management");
        builder.Services.AddTransientWithShellRoute<BusinessConfigPage, BusinessConfigPageModel>("business_config");

        // Rutas adicionales para que los botones del Dashboard no den error al navegar
        // builder.Services.AddTransientWithShellRoute<MenuPage, MenuPageModel>("menu_management");
        // builder.Services.AddTransientWithShellRoute<DevicesPage, DevicesPageModel>("device_management");

        return builder.Build();
    }
}