using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComandaApp.Pages;
using ComandaApp.Services;

namespace ComandaApp.PageModels;

public partial class AdminDashboardPageModel : ObservableObject
{
    private readonly AuthService _authService;

    public AdminDashboardPageModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task AbrirConfig()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(BusinessConfigPage));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Fallo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task AbrirMenu()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(MenuManagementPage));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Fallo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task AbrirDispositivos()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(DeviceManagementPage));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Fallo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task AbrirCorte()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(CorteCajaPage));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Fallo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task AbrirHistorial()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(HistorialPedidosPage));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Fallo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SalirSesion()
    {
        bool confirmar = await Shell.Current.DisplayAlertAsync(
            "Salir de sesión",
            "¿Deseas cerrar la sesión actual?",
            "Sí",
            "No");

        if (!confirmar)
        {
            return;
        }

        await _authService.LogoutAsync();

        await Shell.Current.GoToAsync("//login");
    }
}