using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ComandaApp.PageModels; // <-- Verifica que esto coincida con tu carpeta

public partial class AdminDashboardPageModel : ObservableObject
{
    [RelayCommand]
    private async Task GoToMesas()
    {
        await Shell.Current.DisplayAlert("Navegación", "Próximamente: Gestión de Mesas", "OK");
    }

    [RelayCommand]
    private async Task GoToMenu()
    {
        await Shell.Current.DisplayAlert("Navegación", "Próximamente: Configuración de Menú", "OK");
    }
}