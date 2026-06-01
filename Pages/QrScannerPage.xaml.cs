using ComandaApp.Services;
using ZXing.Net.Maui;

namespace ComandaApp.Pages;

public partial class QrScannerPage : ContentPage
{
    private readonly AuthService _authService;
    private bool _procesando;

    public QrScannerPage(AuthService authService)
    {
        InitializeComponent();

        _authService = authService;

        QrReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.TwoDimensional,
            AutoRotate = true,
            Multiple = false
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var permitido = await SolicitarPermisoCamaraAsync();

        if (!permitido)
        {
            await DisplayAlert("Permiso requerido", "Se necesita acceso a la cámara para escanear códigos QR.", "OK");
            await Shell.Current.GoToAsync("..");
            return;
        }

        _procesando = false;
        QrReader.IsDetecting = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        QrReader.IsDetecting = false;
    }

    private async Task<bool> SolicitarPermisoCamaraAsync()
    {
        var estado = await Permissions.CheckStatusAsync<Permissions.Camera>();

        if (estado == PermissionStatus.Granted)
        {
            return true;
        }

        estado = await Permissions.RequestAsync<Permissions.Camera>();

        return estado == PermissionStatus.Granted;
    }

    private void QrReader_BarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        if (_procesando)
        {
            return;
        }

        var resultado = e.Results.FirstOrDefault();

        if (resultado == null || string.IsNullOrWhiteSpace(resultado.Value))
        {
            return;
        }

        _procesando = true;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            QrReader.IsDetecting = false;
            await ProcesarCodigoAsync(resultado.Value);
        });
    }

    private async Task ProcesarCodigoAsync(string codigo)
    {
        var result = await _authService.LoginWithQrAsync(codigo);

        if (!result.Success)
        {
            await DisplayAlert("Código inválido", result.Error, "OK");
            _procesando = false;
            QrReader.IsDetecting = true;
            return;
        }

        var user = _authService.CurrentUser;
        var rol = user?.Role.Trim().ToLowerInvariant();

        switch (rol)
        {
            case "cliente":
                await Shell.Current.GoToAsync($"//cliente_menu?numeroMesa={Uri.EscapeDataString(user?.Extra ?? string.Empty)}");
                break;

            case "cocina":
                await Shell.Current.GoToAsync("//kitchen_dashboard");
                break;

            case "caja":
                await Shell.Current.GoToAsync("//caja_dashboard");
                break;

            default:
                await Shell.Current.GoToAsync("//admin_dashboard");
                break;
        }
    }

    private async void Cancelar_Clicked(object? sender, EventArgs e)
    {
        QrReader.IsDetecting = false;
        await Shell.Current.GoToAsync("..");
    }
}