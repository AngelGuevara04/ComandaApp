namespace ComandaApp.PageModels;

public partial class BusinessConfigPageModel : ObservableObject
{
    [ObservableProperty]
    private Models.ConfiguracionNegocio config = new();

    [RelayCommand]
    private async Task GuardarConfiguracion()
    {
        // Aquí conectarás a tu DB o API después
        await Shell.Current.DisplayAlert("Éxito", "Configuración guardada correctamente", "OK");
        await Shell.Current.GoToAsync("..");
    }
}