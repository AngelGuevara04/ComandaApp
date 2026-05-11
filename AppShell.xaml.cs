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

        // ==========================================
        // REGISTRO DE RUTAS 
        // ==========================================
        Routing.RegisterRoute(nameof(TableManagementPage), typeof(TableManagementPage));
        Routing.RegisterRoute(nameof(BusinessConfigPage), typeof(BusinessConfigPage));
        Routing.RegisterRoute(nameof(KitchenDashboardPage), typeof(KitchenDashboardPage));

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