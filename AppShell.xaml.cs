using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Font = Microsoft.Maui.Font;
using ComandaApp.Pages; // Agregamos este using para que reconozca nuestras vistas

namespace ComandaApp;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // ==========================================
        // REGISTRO DE RUTAS DE NAVEGACIÓN
        // ==========================================

        // Rutas de las páginas que ya están construidas
        Routing.RegisterRoute("table_management", typeof(TableManagementPage));
        Routing.RegisterRoute("business_config", typeof(BusinessConfigPage));

        // Si tienes la de cocina en este proyecto de MAUI:
        // Routing.RegisterRoute("kitchen_dashboard", typeof(KitchenDashboardPage)); 

        // Rutas de los próximos módulos (comentadas para evitar errores de compilación
        // hasta que creemos los archivos .xaml correspondientes)
        // Routing.RegisterRoute("menu_management", typeof(MenuManagementPage));
        // Routing.RegisterRoute("device_management", typeof(DeviceManagementPage));
        // Routing.RegisterRoute("corte_caja", typeof(CorteCajaPage));
        // Routing.RegisterRoute("order_history", typeof(OrderHistoryPage));


        // Verificación de seguridad reforzada
        if (Application.Current != null && ThemeSegmentedControl != null)
        {
            try
            {
                var currentTheme = Application.Current.RequestedTheme;
                ThemeSegmentedControl.SelectedIndex = currentTheme == AppTheme.Light ? 0 : 1;
            }
            catch
            {
                // Si falla la detección del tema, por defecto iniciamos en el primero
                ThemeSegmentedControl.SelectedIndex = 0;
            }
        }
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

        var snackbar = Snackbar.Make(message, visualOptions: snackbarOptions);
        await snackbar.Show();
    }

    public static async Task DisplayToastAsync(string message)
    {
        if (OperatingSystem.IsWindows()) return;
        var toast = Toast.Make(message, textSize: 16);
        await toast.Show();
    }

    private void SfSegmentedControl_SelectionChanged(object? sender, Syncfusion.Maui.Toolkit.SegmentedControl.SelectionChangedEventArgs e)
    {
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = e.NewIndex == 0 ? AppTheme.Light : AppTheme.Dark;
        }
    }
}