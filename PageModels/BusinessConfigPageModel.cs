using ComandaApp.Models;
using ComandaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ComandaApp.PageModels;

public partial class BusinessConfigPageModel : ObservableObject
{
    private readonly ConfigService _configService;

    [ObservableProperty]
    private ConfiguracionNegocio config = new();

    [ObservableProperty]
    private bool isBusy;

    public BusinessConfigPageModel(ConfigService configService)
    {
        _configService = configService;
    }

    // Carga la configuracion desde Supabase. Se llama desde OnAppearing.
    public async Task CargarAsync()
    {
        IsBusy = true;
        try
        {
            Config = await _configService.GetAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo cargar la configuracion: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Guarda la configuracion en Supabase y regresa a la pantalla anterior.
    [RelayCommand]
    private async Task GuardarConfiguracion()
    {
        if (string.IsNullOrWhiteSpace(Config.NombreRestaurante))
        {
            await Shell.Current.DisplayAlert("Aviso", "El nombre del restaurante es obligatorio.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            await _configService.SaveAsync(Config);
            await Shell.Current.DisplayAlert("Exito", "Configuracion guardada correctamente.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo guardar: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}