using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ComandaApp.PageModels;

public partial class AdminDashboardPageModel : ObservableObject
{
    [RelayCommand]
    private async Task GoToMesas()
    {
        await Shell.Current.DisplayAlert("Gestión", "Abriendo configuración de mesas y QR...", "OK");
    }

    [RelayCommand]
    private async Task GoToMenu()
    {
        await Shell.Current.DisplayAlert("Gestión", "Abriendo configuración de menú y precios...", "OK");
    }
}