using ComandaApp.Models;
using ComandaApp.Pages;
using ComandaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;
using System.Collections.ObjectModel;

namespace ComandaApp.PageModels;

public partial class DeviceManagementPageModel : ObservableObject
{
    private readonly DeviceService _deviceService;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private ObservableCollection<GrupoDispositivos> dispositivosAgrupados = new();

    [ObservableProperty]
    private Dispositivo? dispositivoSeleccionado;

    [ObservableProperty]
    private ImageSource? qrImageSource;

    [ObservableProperty]
    private bool qrVisible;

    public bool NoHayDispositivos => DispositivosAgrupados.Count == 0;

    public DeviceManagementPageModel(DeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [RelayCommand]
    public async Task CargarDispositivosAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var dispositivos = await _deviceService.GetAllAsync();

            var grupos = dispositivos
                .GroupBy(d => d.Rol)
                .Select(g => new GrupoDispositivos(g.Key.ToString(), g.ToList()))
                .OrderBy(g => g.Titulo)
                .ToList();

            DispositivosAgrupados = new ObservableCollection<GrupoDispositivos>(grupos);

            OnPropertyChanged(nameof(NoHayDispositivos));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudieron cargar los dispositivos: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task AbrirCuestionario(string rolSeleccionado)
    {
        try
        {
            await Shell.Current.GoToAsync($"{nameof(AddDevicePage)}?rol={Uri.EscapeDataString(rolSeleccionado)}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo abrir el formulario: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task MostrarQr(Dispositivo dispositivo)
    {
        if (dispositivo == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(dispositivo.QrCodeData))
        {
            await Shell.Current.DisplayAlertAsync("Sin código", "Este dispositivo no tiene código QR guardado.", "OK");
            return;
        }

        DispositivoSeleccionado = dispositivo;
        QrImageSource = CrearImagenQr(dispositivo.QrCodeData);
        QrVisible = true;
    }

    [RelayCommand]
    private void CerrarQr()
    {
        QrVisible = false;
        DispositivoSeleccionado = null;
        QrImageSource = null;
    }

    [RelayCommand]
    private async Task EliminarDispositivo(Dispositivo dispositivo)
    {
        if (dispositivo == null)
        {
            return;
        }

        var confirmar = await Shell.Current.DisplayAlertAsync(
            "Eliminar dispositivo",
            $"¿Deseas eliminar {dispositivo.Nombre}?",
            "Sí",
            "No");

        if (!confirmar)
        {
            return;
        }

        try
        {
            await _deviceService.DeleteAsync(dispositivo.Id);
            await CargarDispositivosAsync();

            if (DispositivoSeleccionado?.Id == dispositivo.Id)
            {
                CerrarQr();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo eliminar: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task EliminarPorRol()
    {
        var rolStr = await Shell.Current.DisplayActionSheet(
            "Eliminar rol completo",
            "Cancelar",
            null,
            "Mesa",
            "Caja",
            "Cocina");

        if (string.IsNullOrWhiteSpace(rolStr) || rolStr == "Cancelar")
        {
            return;
        }

        var confirmar = await Shell.Current.DisplayAlertAsync(
            "Confirmar eliminación",
            $"¿Deseas eliminar todos los dispositivos del rol {rolStr}?",
            "Sí",
            "No");

        if (!confirmar)
        {
            return;
        }

        try
        {
            var rol = Enum.Parse<RolDispositivo>(rolStr);
            await _deviceService.DeleteByRolAsync(rol);
            await CargarDispositivosAsync();
            CerrarQr();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo eliminar el rol: {ex.Message}", "OK");
        }
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