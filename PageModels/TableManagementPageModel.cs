using ComandaApp.Models;
using ComandaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;
using System.Collections.ObjectModel;

namespace ComandaApp.PageModels;

public partial class TableManagementPageModel : ObservableObject
{
    private readonly MesaService _mesaService;

    [ObservableProperty]
    private ObservableCollection<Mesa> mesas = new();

    [ObservableProperty]
    private bool isBusy;

    public TableManagementPageModel(MesaService mesaService)
    {
        _mesaService = mesaService;
    }

    // Carga las mesas desde Supabase. Se llama desde OnAppearing.
    public async Task CargarAsync()
    {
        IsBusy = true;
        try
        {
            var lista = await _mesaService.GetAllAsync();
            Mesas = new ObservableCollection<Mesa>(lista);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo cargar las mesas: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Agrega una nueva mesa generando su codigo QR y guardando en Supabase.
    [RelayCommand]
    private async Task AgregarMesa()
    {
        string numeroIngresado = await Shell.Current.DisplayPromptAsync(
            "Nueva Mesa", "Numero o nombre de la mesa:");

        if (string.IsNullOrWhiteSpace(numeroIngresado)) return;

        // Verificar que no exista ya una mesa con ese numero.
        if (Mesas.Any(m => m.NumeroMesa == numeroIngresado.Trim()))
        {
            await Shell.Current.DisplayAlert("Aviso", "Ya existe una mesa con ese numero.", "OK");
            return;
        }

        string capacidadStr = await Shell.Current.DisplayPromptAsync(
            "Capacidad", "Numero de personas:", keyboard: Keyboard.Numeric, initialValue: "4");
        int.TryParse(capacidadStr, out int capacidad);
        if (capacidad <= 0) capacidad = 4;

        string area = await Shell.Current.DisplayActionSheet(
            "Area", "Cancelar", null, "General", "Terraza", "VIP", "Bar");
        if (area == "Cancelar" || string.IsNullOrEmpty(area)) area = "General";

        // Generar el token unico del QR permanente.
        string qrData = $"comanda_mesa_{numeroIngresado.Trim().Replace(" ", "_")}_{Guid.NewGuid().ToString()[..8]}";

        var nuevaMesa = new Mesa
        {
            NumeroMesa = numeroIngresado.Trim(),
            Capacidad = capacidad,
            Area = area,
            QrCodeData = qrData,
            EstaOcupada = false
        };

        IsBusy = true;
        try
        {
            await _mesaService.AddAsync(nuevaMesa);
            Mesas.Add(nuevaMesa);
            await Shell.Current.DisplayAlert(
                "Mesa agregada",
                $"Mesa {numeroIngresado} creada.\nCodigo QR: {qrData}",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo guardar la mesa: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Genera y muestra la imagen QR de la mesa seleccionada.
    [RelayCommand]
    private async Task VerQrMesa(Mesa mesa)
    {
        // Generamos la imagen QR con QRCoder para mostrarla al admin.
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeInfo = qrGenerator.CreateQrCode(mesa.QrCodeData, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeInfo);
        byte[] qrBytes = qrCode.GetGraphic(10);

        // Por ahora mostramos el codigo como texto.
        // La imagen se puede mostrar en una pagina de detalle mas adelante.
        await Shell.Current.DisplayAlert(
            $"QR Mesa {mesa.NumeroMesa}",
            $"Codigo: {mesa.QrCodeData}\n\nArea: {mesa.Area} | Capacidad: {mesa.Capacidad}",
            "Cerrar");
    }

    // Elimina una mesa de Supabase y de la lista local.
    [RelayCommand]
    private async Task EliminarMesa(Mesa mesa)
    {
        bool confirmar = await Shell.Current.DisplayAlert(
            "Eliminar Mesa",
            $"Eliminar la mesa {mesa.NumeroMesa}?",
            "Si, Eliminar", "Cancelar");

        if (!confirmar) return;

        IsBusy = true;
        try
        {
            await _mesaService.DeleteAsync(mesa.NumeroMesa);
            Mesas.Remove(mesa);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo eliminar: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Cambia el estado de ocupacion de la mesa y lo persiste en Supabase.
    [RelayCommand]
    private async Task CambiarEstadoMesa(Mesa mesa)
    {
        bool nuevoEstado = !mesa.EstaOcupada;
        try
        {
            await _mesaService.ActualizarEstadoAsync(mesa.NumeroMesa, nuevoEstado);
            mesa.EstaOcupada = nuevoEstado;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"No se pudo actualizar el estado: {ex.Message}", "OK");
        }
    }
}