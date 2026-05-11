using System.Collections.ObjectModel;

namespace ComandaApp.PageModels;

public partial class TableManagementPageModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Models.Mesa> mesas = new();

    public TableManagementPageModel()
    {
        // Datos de prueba
        Mesas.Add(new Models.Mesa { NumeroMesa = "1", Capacidad = 4, Area = "Terraza" });
        Mesas.Add(new Models.Mesa { NumeroMesa = "2", Capacidad = 2, Area = "General" });
    }

    [RelayCommand]
    private async Task AgregarMesa()
    {
        string result = await Shell.Current.DisplayPromptAsync("Nueva Mesa", "Número de mesa:");
        if (!string.IsNullOrWhiteSpace(result))
        {
            Mesas.Add(new Models.Mesa { NumeroMesa = result, Capacidad = 4 });
        }
    }
}