using System.Collections.ObjectModel;
using ComandaApp.Models;
using ComandaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;

namespace ComandaApp.PageModels;

public partial class CajaDashboardPageModel : ObservableObject
{
    private readonly OrdenService _ordenService;
    private readonly MesaService _mesaService;
    private readonly AuthService _authService;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private ObservableCollection<OrdenMesa> ordenesActivas = new();

    [ObservableProperty]
    private string nombreClienteTemporal = string.Empty;

    [ObservableProperty]
    private string numeroMesaTemporal = string.Empty;

    [ObservableProperty]
    private string qrTemporal = string.Empty;

    [ObservableProperty]
    private ImageSource? qrTemporalImageSource;

    public bool HayOrdenes => OrdenesActivas.Count > 0;
    public bool NoHayOrdenes => !HayOrdenes;
    public bool TieneQrTemporal => !string.IsNullOrWhiteSpace(QrTemporal);

    public CajaDashboardPageModel(
        OrdenService ordenService,
        MesaService mesaService,
        AuthService authService)
    {
        _ordenService = ordenService;
        _mesaService = mesaService;
        _authService = authService;
    }

    partial void OnQrTemporalChanged(string value)
    {
        OnPropertyChanged(nameof(TieneQrTemporal));
    }

    [RelayCommand]
    public async Task CargarOrdenesAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var ordenes = await _ordenService.GetOrdenesActivasAsync();

            OrdenesActivas = new ObservableCollection<OrdenMesa>(ordenes);

            RefrescarEstadoLista();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudieron cargar las órdenes: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GenerarQrTemporal()
    {
        if (string.IsNullOrWhiteSpace(NombreClienteTemporal))
        {
            await Shell.Current.DisplayAlertAsync("Dato faltante", "Ingresa el nombre del cliente.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(NumeroMesaTemporal))
        {
            await Shell.Current.DisplayAlertAsync("Dato faltante", "Ingresa el número de mesa.", "OK");
            return;
        }

        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var negocioId = _authService.NegocioIdActual;
            var numeroMesa = NumeroMesaTemporal.Trim();
            var nombreCliente = NombreClienteTemporal.Trim();

            await _ordenService.CrearOrdenParaCajaAsync(numeroMesa, nombreCliente);
            await _mesaService.ActualizarEstadoAsync(numeroMesa, true);

            QrTemporal = $"https://comanda-web-app.onrender.com/?mesa={numeroMesa}&negocio={negocioId}";
            QrTemporalImageSource = CrearImagenQr(QrTemporal);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo generar el acceso: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }

        await CargarOrdenesAsync();
    }

    [RelayCommand]
    private async Task ConfirmarPago(OrdenMesa orden)
    {
        if (orden == null)
        {
            return;
        }

        var confirmar = await Shell.Current.DisplayAlertAsync(
            "Confirmar pago",
            $"¿Confirmas el pago de la mesa {orden.MesaAsignada.NumeroMesa} por ${orden.TotalCuenta:F2}?",
            "Sí",
            "No");

        if (!confirmar)
        {
            return;
        }

        IsBusy = true;

        try
        {
            await _ordenService.CerrarOrdenAsync(orden.IdOrden);
            await _mesaService.ActualizarEstadoAsync(orden.MesaAsignada.NumeroMesa, false);

            OrdenesActivas.Remove(orden);
            RefrescarEstadoLista();

            await Shell.Current.DisplayAlertAsync("Pago confirmado", "La orden fue pagada y la mesa quedó libre.", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo confirmar el pago: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SalirRol()
    {
        var confirmar = await Shell.Current.DisplayAlertAsync(
            "Salir de caja",
            "¿Deseas salir del panel de caja?",
            "Sí",
            "No");

        if (!confirmar)
        {
            return;
        }

        _authService.Logout();

        await Shell.Current.GoToAsync("//login");
    }

    private void RefrescarEstadoLista()
    {
        OnPropertyChanged(nameof(HayOrdenes));
        OnPropertyChanged(nameof(NoHayOrdenes));
    }

    private static ImageSource CrearImagenQr(string texto)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeInfo = qrGenerator.CreateQrCode(texto, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeInfo);
        var qrBytes = qrCode.GetGraphic(10);

        return ImageSource.FromStream(() => new MemoryStream(qrBytes));
    }
}