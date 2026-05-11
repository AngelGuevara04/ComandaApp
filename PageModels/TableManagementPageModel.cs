using System.Collections.ObjectModel;
using ComandaApp.Models;

namespace ComandaApp.PageModels;

public partial class TableManagementPageModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Mesa> mesas = new();

    [RelayCommand]
    private async Task AgregarMesa()
    {
        // 1. Pedimos el número de mesa
        string numero = await Shell.Current.DisplayPromptAsync("Nueva Mesa", "Ingrese el número o nombre de la mesa:");

        if (!string.IsNullOrWhiteSpace(numero))
        {
            // 2. Creamos la mesa con un identificador único para el QR
            var nuevaMesa = new Mesa
            {
                NumeroMesa = numero,
                Capacidad = 4,
                QrCodeData = $"comanda_mesa_{numero}_{Guid.NewGuid().ToString().Substring(0, 8)}"
            };

            Mesas.Add(nuevaMesa);

            await AppShell.DisplayToastAsync($"Mesa {numero} agregada con éxito.");
        }
    }

    [RelayCommand]
    private async Task VerQrMesa(Mesa mesa)
    {
        // Lógica para mostrar el QR en una ventana emergente o navegar a detalle
        await Shell.Current.DisplayAlert("Código QR", $"Datos del QR: {mesa.QrCodeData}", "OK");
    }
}