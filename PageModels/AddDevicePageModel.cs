using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComandaApp.Models;

namespace ComandaApp.PageModels;

// Atrapamos el parámetro ?rol=Mesa que enviamos en la pantalla anterior
[QueryProperty(nameof(RolStr), "rol")]
public partial class AddDevicePageModel : ObservableObject
{
    // Instancia del gestor principal para poder guardar el dispositivo ahí
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

    [ObservableProperty]
    private bool qrGenerado = false;

    [ObservableProperty]
    private bool formularioVisible = true;

    // Inyectamos el ViewModel principal
    public AddDevicePageModel(DeviceManagementPageModel adminViewModel)
    {
        _adminViewModel = adminViewModel;
    }

    // Este método se ejecuta automáticamente cuando MAUI recibe el Rol
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
    private async Task generarQR()
    {
        if (string.IsNullOrWhiteSpace(NombreInput))
        {
            await Shell.Current.DisplayAlert("Error", "El campo principal es obligatorio", "OK");
            return;
        }

        // Generamos el Hash/Texto único del QR
        QrCodeData = $"comanda_{RolStr.ToLower()}_{NombreInput.Replace(" ", "_")}_{Guid.NewGuid().ToString().Substring(0, 5)}";

        var nuevoDispositivo = new Dispositivo
        {
            Rol = Enum.Parse<RolDispositivo>(RolStr),
            Nombre = NombreInput,
            DetalleExtra = DetalleInput,
            QrCodeData = QrCodeData
        };

        // Lo mandamos al panel de administración
        _adminViewModel.AgregarDispositivoDesdeForm(nuevoDispositivo);

        // Ocultamos el formulario y revelamos el QR
        FormularioVisible = false;
        QrGenerado = true;
    }

    [RelayCommand]
    private async Task volver()
    {
        await Shell.Current.GoToAsync("..");
    }
}