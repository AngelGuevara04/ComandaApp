using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ComandaApp.PageModels;

public partial class AdminDashboardPageModel : ObservableObject
{
    [RelayCommand]
    private async Task GoToMesas()
    {
        await Shell.Current.DisplayAlert("Navegación", "Abriendo gestión de mesas...", "OK");
    }

    [RelayCommand]
    private async Task GoToMenu()
    {
        await Shell.Current.DisplayAlert("Navegación", "Abriendo configuración de menú...", "OK");
    }
}