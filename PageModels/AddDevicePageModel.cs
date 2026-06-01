using ComandaApp.Models;
using ComandaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;

namespace ComandaApp.PageModels;

[QueryProperty(nameof(RolStr), "rol")]
public partial class AddDevicePageModel : ObservableObject
{
    private readonly DeviceService _deviceService;

    [ObservableProperty]
    private string rolStr = string.Empty;

    [ObservableProperty]
    private string tituloFormulario = string.Empty;

    [ObservableProperty]
    private string promptNombre = string.Empty;

    [ObservableProperty]
    private string promptDetalle = string.Empty;

    [ObservableProperty]
    private string nombreInput = string.Empty;

    [ObservableProperty]
    private string detalleInput = string.Empty;

    [ObservableProperty]
    private string qrCodeData = string.Empty;

    [ObservableProperty]
    private ImageSource? qrImageSource;

    [ObservableProperty]
    private bool qrGenerado;

    [ObservableProperty]
    private bool formularioVisible = true;

    [ObservableProperty]
    private bool isBusy;

    public AddDevicePageModel(DeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    partial void OnRolStrChanged(string value)
    {
        if (value == "Mesa")
        {
            TituloFormulario = "Configurar nueva mesa";
            PromptNombre = "Identificador de mesa";
            PromptDetalle = "Capacidad o descripción";
        }
        else if (value == "Cocina")
        {
            TituloFormulario = "Vincular pantalla de cocina";
            PromptNombre = "Nombre de estación";
            PromptDetalle = "Encargado o descripción";
        }
        else if (value == "Caja")
        {
            TituloFormulario = "Vincular terminal de caja";
            PromptNombre = "Identificador de caja";
            PromptDetalle = "Turno u operador";
        }
    }

    [RelayCommand]
    private async Task GenerarQR()
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(NombreInput))
        {
            await Shell.Current.DisplayAlertAsync("Error", "El campo principal es obligatorio.", "OK");
            return;
        }

        if (!Enum.TryParse<RolDispositivo>(RolStr, out var rol))
        {
            await Shell.Current.DisplayAlertAsync("Error", "Rol no válido.", "OK");
            return;
        }

        IsBusy = true;

        try
        {
            var nombreLimpio = NombreInput.Trim();
            var textoParaQr = CrearTextoQr(rol, nombreLimpio);

            QrCodeData = textoParaQr;
            QrImageSource = CrearImagenQr(textoParaQr);

            var nuevoDispositivo = new Dispositivo
            {
                Rol = rol,
                Nombre = nombreLimpio,
                DetalleExtra = DetalleInput.Trim(),
                QrCodeData = textoParaQr,
                FechaVinculacion = DateTime.Now
            };

            await _deviceService.AddAsync(nuevoDispositivo);

            FormularioVisible = false;
            QrGenerado = true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo guardar el dispositivo: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Volver()
    {
        await Shell.Current.GoToAsync("..");
    }

    private static string CrearTextoQr(RolDispositivo rol, string nombre)
    {
        var nombreQr = nombre
            .Trim()
            .Replace(" ", "_")
            .ToLowerInvariant();

        var codigo = Guid.NewGuid().ToString("N")[..5];

        return $"comanda_{rol.ToString().ToLowerInvariant()}_{nombreQr}_{codigo}";
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