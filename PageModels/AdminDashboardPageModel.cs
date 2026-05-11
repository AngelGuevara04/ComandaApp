using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComandaApp.Pages; // Necesario para usar nameof()

namespace ComandaApp.PageModels;

public partial class AdminDashboardPageModel : ObservableObject
{
    [RelayCommand]
    private async Task goToMesas()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(TableManagementPage));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error Real", $"Fallo en Mesas: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task goToConfig()
    {
        try
        {
            await Shell.Current.GoToAsync(nameof(BusinessConfigPage));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error Real", $"Fallo en Config: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task goToMenu()
    {
        await Shell.Current.DisplayAlert("Aviso", "El módulo Menú aún no está creado.", "OK");
    }

    [RelayCommand]
    private async Task goToDevices()
    {
        await Shell.Current.DisplayAlert("Aviso", "El módulo Dispositivos aún no está creado.", "OK");
    }

    [RelayCommand]
    private async Task goToCorte()
    {
        await Shell.Current.DisplayAlert("Aviso", "El módulo Corte de Caja aún no está creado.", "OK");
    }

    [RelayCommand]
    private async Task goToHistory()
    {
        await Shell.Current.DisplayAlert("Aviso", "El módulo Historial aún no está creado.", "OK");
    }
}