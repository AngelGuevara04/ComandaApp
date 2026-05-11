using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComandaApp.Models;

namespace ComandaApp.PageModels;

public partial class DeviceManagementPageModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<GrupoDispositivos> dispositivosAgrupados = new();

    private List<Dispositivo> listaMaestra = new();

    public DeviceManagementPageModel() => actualizarGrupos();

    [RelayCommand]
    private async Task agregarDispositivo()
    {
        string rolStr = await Shell.Current.DisplayActionSheet("Tipo de Dispositivo", "Cancelar", null, "Mesa", "Caja", "Cocina");
        if (rolStr == "Cancelar") return;

        string nombre = await Shell.Current.DisplayPromptAsync("Identificador", $"Nombre para {rolStr}:");
        if (string.IsNullOrWhiteSpace(nombre)) return;

        var rol = Enum.Parse<RolDispositivo>(rolStr);
        var nuevo = new Dispositivo
        {
            Nombre = nombre,
            Rol = rol,
            QrCodeData = $"comanda_{rol.ToString().ToLower()}_{nombre}_{Guid.NewGuid().ToString().Substring(0, 5)}"
        };

        listaMaestra.Add(nuevo);
        actualizarGrupos();
    }

    [RelayCommand]
    private void eliminarDispositivo(Dispositivo d)
    {
        listaMaestra.Remove(d);
        actualizarGrupos();
    }

    [RelayCommand]
    private async Task eliminarPorRol()
    {
        string rolStr = await Shell.Current.DisplayActionSheet("Limpiar Rol Completo", "Cancelar", null, "Mesa", "Caja", "Cocina");
        if (rolStr == "Cancelar") return;

        var rol = Enum.Parse<RolDispositivo>(rolStr);
        listaMaestra.RemoveAll(x => x.Rol == rol);
        actualizarGrupos();
    }

    private void actualizarGrupos()
    {
        var nuevosGrupos = listaMaestra
            .GroupBy(d => d.Rol)
            .Select(g => new GrupoDispositivos(g.Key.ToString(), g.ToList()))
            .OrderBy(g => g.Titulo)
            .ToList();

        DispositivosAgrupados = new ObservableCollection<GrupoDispositivos>(nuevosGrupos);
    }
}