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
    private ObservableCollection<MesaCajaGroup> mesasActivas = new();

    [ObservableProperty]
    private string nombreClienteTemporal = string.Empty;

    [ObservableProperty]
    private string numeroMesaTemporal = string.Empty;

    [ObservableProperty]
    private string qrTemporal = string.Empty;

    [ObservableProperty]
    private ImageSource? qrTemporalImageSource;

    [ObservableProperty]
    private bool mostrandoCobro;

    [ObservableProperty]
    private MesaCajaGroup? mesaACobrar;

    [ObservableProperty]
    private string montoRecibidoText = string.Empty;

    [ObservableProperty]
    private double cambio;

    public bool HayOrdenes => MesasActivas.Count > 0;
    public bool NoHayOrdenes => !HayOrdenes;
    public bool TieneQrTemporal => !string.IsNullOrWhiteSpace(QrTemporal);
    public bool PuedeFinalizarPago => !string.IsNullOrWhiteSpace(MontoRecibidoText) && 
                                      double.TryParse(MontoRecibidoText, out var m) && 
                                      MesaACobrar != null && 
                                      m >= MesaACobrar.TotalCuenta;

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

    partial void OnMontoRecibidoTextChanged(string value)
    {
        if (double.TryParse(value, out var monto) && MesaACobrar != null)
        {
            Cambio = monto - MesaACobrar.TotalCuenta;
            if (Cambio < 0) Cambio = 0;
        }
        else
        {
            Cambio = 0;
        }
        OnPropertyChanged(nameof(PuedeFinalizarPago));
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

            var grupos = ordenes.GroupBy(o => o.MesaAsignada.NumeroMesa)
                .Select(g => new MesaCajaGroup
                {
                    NumeroMesa = g.Key,
                    NombresClientes = string.Join(", ", g.Select(o => o.NombreCliente).Distinct()),
                    TotalCuenta = g.Sum(o => o.TotalCuenta),
                    Platillos = new ObservableCollection<DetallePedido>(g.SelectMany(o => o.Platillos)),
                    IdsOrdenes = g.Select(o => o.IdOrden).ToList()
                }).ToList();

            MesasActivas = new ObservableCollection<MesaCajaGroup>(grupos);

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
    private void PrepararCobro(MesaCajaGroup mesa)
    {
        if (mesa == null) return;
        MesaACobrar = mesa;
        MontoRecibidoText = string.Empty;
        Cambio = 0;
        MostrandoCobro = true;
    }

    [RelayCommand]
    private void CancelarCobro()
    {
        MostrandoCobro = false;
        MesaACobrar = null;
    }

    [RelayCommand]
    private async Task FinalizarPago()
    {
        if (MesaACobrar == null) return;

        IsBusy = true;

        try
        {
            foreach (var idOrden in MesaACobrar.IdsOrdenes)
            {
                await _ordenService.CerrarOrdenAsync(idOrden);
            }
            await _mesaService.ActualizarEstadoAsync(MesaACobrar.NumeroMesa, false);

            MesasActivas.Remove(MesaACobrar);
            RefrescarEstadoLista();

            MostrandoCobro = false;
            var numMesa = MesaACobrar.NumeroMesa;
            MesaACobrar = null;

            await Shell.Current.DisplayAlertAsync("Pago confirmado", $"La orden de la mesa {numMesa} fue pagada y quedó libre.", "OK");
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

public partial class MesaCajaGroup : ObservableObject
{
    [ObservableProperty]
    private string numeroMesa = string.Empty;

    [ObservableProperty]
    private string nombresClientes = string.Empty;

    [ObservableProperty]
    private double totalCuenta;

    [ObservableProperty]
    private ObservableCollection<DetallePedido> platillos = new();

    public List<string> IdsOrdenes { get; set; } = new();
}