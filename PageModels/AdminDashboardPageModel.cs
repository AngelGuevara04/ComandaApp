using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComandaApp.Pages;

namespace ComandaApp.PageModels;

public partial class AdminDashboardPageModel : ObservableObject
{
    [RelayCommand]
    private async Task AbrirConfig()
    {
        try { await Shell.Current.GoToAsync(nameof(BusinessConfigPage)); }
        catch (Exception ex) { await Shell.Current.DisplayAlert("Error", $"Fallo: {ex.Message}", "OK"); }
    }

    [RelayCommand]
    private async Task AbrirMenu()
    {
        try { await Shell.Current.GoToAsync(nameof(MenuManagementPage)); }
        catch (Exception ex) { await Shell.Current.DisplayAlert("Error", $"Fallo: {ex.Message}", "OK"); }
    }

    [RelayCommand]
    private async Task AbrirDispositivos()
    {
        // ¡Corregido! Ahora navega a la página de dispositivos reales
        try { await Shell.Current.GoToAsync(nameof(DeviceManagementPage)); }
        catch (Exception ex) { await Shell.Current.DisplayAlert("Error", $"Fallo: {ex.Message}", "OK"); }
    }

    [RelayCommand]
    private async Task AbrirCorte()
    {
        await Shell.Current.DisplayAlert("Próximamente", "El módulo Corte de Caja aún no está creado.", "OK");
    }

    [RelayCommand]
    private async Task AbrirHistorial()
    {
        await Shell.Current.DisplayAlert("Próximamente", "El módulo Historial aún no está creado.", "OK");
    }
}