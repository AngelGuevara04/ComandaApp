using CommunityToolkit.Mvvm.ComponentModel;

namespace ComandaApp.Models;

public enum RolDispositivo { Mesa, Caja, Cocina }

public partial class Dispositivo : ObservableObject
{
    [ObservableProperty]
    private string id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private RolDispositivo rol;

    [ObservableProperty]
    private string qrCodeData = string.Empty;

    [ObservableProperty]
    private DateTime fechaVinculacion = DateTime.Now;
}

// Clase para agrupar dispositivos por Rol en la UI
public class GrupoDispositivos : List<Dispositivo>
{
    public string Titulo { get; set; }
    public GrupoDispositivos(string titulo, List<Dispositivo> dispositivos) : base(dispositivos)
    {
        Titulo = titulo;
    }
}