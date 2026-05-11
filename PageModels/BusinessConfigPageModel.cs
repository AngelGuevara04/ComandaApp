using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComandaApp.Models;

namespace ComandaApp.PageModels;

public partial class BusinessConfigPageModel : ObservableObject
{
    [ObservableProperty]
    private ConfiguracionNegocio config = new();

    [RelayCommand]
    private async Task guardarConfiguracion()
    {
        // Aquí conectarás a tu DB o API después
        await Shell.Current.DisplayAlert("Éxito", "Configuración guardada correctamente.", "OK");

        // Regresa a la pantalla anterior (Centro de Mando)
        await Shell.Current.GoToAsync("..");
    }
}