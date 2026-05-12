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

    // 1. CORRECCIÓN: PascalCase y Try/Catch para navegación segura
    [RelayCommand]
    private async Task AbrirCuestionario(string rolSeleccionado)
    {
        try
        {
            await Shell.Current.GoToAsync($"{nameof(AddDevicePage)}?rol={rolSeleccionado}");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error de Navegación", $"Fallo al abrir formulario: {ex.Message}", "OK");
        }
    }

    // Este método público permite que el formulario guarde el dispositivo aquí al terminar
    public void AgregarDispositivoDesdeForm(Dispositivo nuevoDispositivo)
    {
        listaMaestra.Add(nuevoDispositivo);
        actualizarGrupos();
    }

    // 2. CORRECCIÓN: PascalCase para evitar fallos en el botón de eliminar
    [RelayCommand]
    private void EliminarDispositivo(Dispositivo d)
    {
        listaMaestra.Remove(d);
        actualizarGrupos();
    }

    // 3. CORRECCIÓN: PascalCase para evitar fallos en el botón limpiar
    [RelayCommand]
    private async Task EliminarPorRol()
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