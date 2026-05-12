using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComandaApp.Models;
using ComandaApp.Pages;

namespace ComandaApp.PageModels;

public partial class DeviceManagementPageModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<GrupoDispositivos> dispositivosAgrupados = new();

    private List<Dispositivo> listaMaestra = new();

    public DeviceManagementPageModel() => actualizarGrupos();

    // Este comando abre la nueva página enviándole qué apartado presionó el usuario
    [RelayCommand]
    private async Task abrirCuestionario(string rolSeleccionado)
    {
        await Shell.Current.GoToAsync($"{nameof(AddDevicePage)}?rol={rolSeleccionado}");
    }

    // Este método público permite que el formulario guarde el dispositivo aquí al terminar
    public void AgregarDispositivoDesdeForm(Dispositivo nuevoDispositivo)
    {
        listaMaestra.Add(nuevoDispositivo);
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