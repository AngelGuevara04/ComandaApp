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

        // ========================================================
        // INYECCIÓN DE DEPENDENCIAS (DI)
        // ========================================================

        // 1. Registro de Dashboard Principal (Singleton: solo se crea una vez en memoria)
        builder.Services.AddSingleton<AdminDashboardPage>();
        builder.Services.AddSingleton<AdminDashboardPageModel>();

        // 2. Registro de Páginas Secundarias del Administrador (Transient: se recrean al abrir)
        builder.Services.AddTransient<TableManagementPage>();
        builder.Services.AddTransient<TableManagementPageModel>();

        builder.Services.AddTransient<BusinessConfigPage>();
        builder.Services.AddTransient<BusinessConfigPageModel>();

        // 3. Registro del módulo de Cocina (que construimos hace un momento)
        builder.Services.AddTransient<KitchenDashboardPage>();
        builder.Services.AddTransient<KitchenDashboardPageModel>();

        return builder.Build();
    }
}