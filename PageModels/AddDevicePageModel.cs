using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComandaApp.Models;
using QRCoder; // NUEVO: Librería de Códigos QR

namespace ComandaApp.PageModels;

[QueryProperty(nameof(RolStr), "rol")]
public partial class AddDevicePageModel : ObservableObject
{
    private readonly DeviceManagementPageModel _adminViewModel;

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

    // NUEVO: Aquí guardaremos la imagen generada del QR para mostrarla en pantalla
    [ObservableProperty]
    private ImageSource qrImageSource;

    [ObservableProperty]
    private bool qrGenerado = false;

    [ObservableProperty]
    private bool formularioVisible = true;

    public AddDevicePageModel(DeviceManagementPageModel adminViewModel)
    {
        _adminViewModel = adminViewModel;
    }

    partial void OnRolStrChanged(string value)
    {
        if (value == "Mesa")
        {
            TituloFormulario = "Configurar Nueva Mesa";
            PromptNombre = "Identificador de Mesa (Ej. Mesa 12)";
            PromptDetalle = "Capacidad (Ej. 4 personas)";
        }
        else if (value == "Cocina")
        {
            TituloFormulario = "Vincular Pantalla de Cocina";
            PromptNombre = "Nombre de Estación (Ej. Parrilla y Asados)";
            PromptDetalle = "Encargado de estación (Opcional)";
        }
        else if (value == "Caja")
        {
            TituloFormulario = "Vincular Terminal de Caja";
            PromptNombre = "Identificador de Caja (Ej. Caja Principal)";
            PromptDetalle = "Turno u Operador (Opcional)";
        }
    }

    [RelayCommand]
    private async Task GenerarQR()
    {
        if (string.IsNullOrWhiteSpace(NombreInput))
        {
            await Shell.Current.DisplayAlert("Error", "El campo principal es obligatorio", "OK");
            return;
        }

        // 1. Creamos el texto hash/único de tu dispositivo
        QrCodeData = $"comanda_{RolStr.ToLower()}_{NombreInput.Replace(" ", "_")}_{Guid.NewGuid().ToString().Substring(0, 5)}";

        // 2. MAGIA OFFLINE: Generamos el código QR real en imagen
        using (QRCodeGenerator qrGenerator = new QRCodeGenerator())
        {
            // Nivel de corrección de error 'Q' (25% de resistencia a daños/manchas)
            QRCodeData qrCodeInfo = qrGenerator.CreateQrCode(QrCodeData, QRCodeGenerator.ECCLevel.Q);
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeInfo);

            // Obtenemos los bytes de la imagen. El número 10 es la escala/tamaño de los píxeles.
            byte[] qrBytes = qrCode.GetGraphic(10);

            // Lo inyectamos directamente en la Vista sin necesidad de guardarlo en el disco duro del teléfono
            QrImageSource = ImageSource.FromStream(() => new MemoryStream(qrBytes));
        }

        // 3. Guardamos el dispositivo en el Gestor Central
        var nuevoDispositivo = new Dispositivo
        {
            Rol = Enum.Parse<RolDispositivo>(RolStr),
            Nombre = NombreInput,
            DetalleExtra = DetalleInput,
            QrCodeData = QrCodeData
        };

        _adminViewModel.AgregarDispositivoDesdeForm(nuevoDispositivo);

        // 4. Cambiamos de pantalla
        FormularioVisible = false;
        QrGenerado = true;
    }

    [RelayCommand]
    private async Task Volver()
    {
        await Shell.Current.GoToAsync("..");
    }
}