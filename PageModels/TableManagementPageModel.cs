using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComandaApp.Models;

namespace ComandaApp.PageModels;

public partial class TableManagementPageModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Mesa> mesas = new();

    [RelayCommand]
    private async Task agregarMesa()
    {
        // 1. Pedimos el número de mesa utilizando variables en camelCase
        string numeroIngresado = await Shell.Current.DisplayPromptAsync("Nueva Mesa", "Ingrese el número o nombre de la mesa:");

        if (!string.IsNullOrWhiteSpace(numeroIngresado))
        {
            // 2. Creamos la mesa con un identificador único para el QR
            var nuevaMesa = new Mesa
            {
                NumeroMesa = numeroIngresado,
                Capacidad = 4,
                QrCodeData = $"comanda_mesa_{numeroIngresado}_{Guid.NewGuid().ToString().Substring(0, 8)}",
                EstaOcupada = false
            };

            Mesas.Add(nuevaMesa);

            // Reemplazado por DisplayAlert por seguridad estándar de MAUI, 
            // a menos que AppShell.DisplayToastAsync sea una extensión personalizada tuya.
            await Shell.Current.DisplayAlert("Éxito", $"Mesa {numeroIngresado} agregada con éxito.", "OK");
        }
    }

    [RelayCommand]
    private async Task verQrMesa(Mesa mesaSeleccionada)
    {
        // Lógica para mostrar el QR en una ventana emergente o navegar a detalle
        await Shell.Current.DisplayAlert("Código QR", $"Datos del QR: {mesaSeleccionada.QrCodeData}", "OK");
    }

    [RelayCommand]
    private async Task eliminarMesa(Mesa mesaSeleccionada)
    {
        bool confirmacionBorrado = await Shell.Current.DisplayAlert(
            "Eliminar Mesa",
            $"¿Está seguro de que desea eliminar la mesa {mesaSeleccionada.NumeroMesa}?",
            "Sí, Eliminar",
            "Cancelar");

        if (confirmacionBorrado)
        {
            Mesas.Remove(mesaSeleccionada);
        }
    }

    [RelayCommand]
    private void cambiarEstadoMesa(Mesa mesaSeleccionada)
    {
        // Al haber convertido Mesa en un ObservableObject, la UI detectará este cambio instantáneamente.
        mesaSeleccionada.EstaOcupada = !mesaSeleccionada.EstaOcupada;
    }
}