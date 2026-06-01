using System.Collections.ObjectModel;
using System.Globalization;
using ComandaApp.Models;
using ComandaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ComandaApp.PageModels;

public partial class MenuManagementPageModel : ObservableObject
{
    private readonly MenuService _menuService;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private ObservableCollection<Platillo> platillos = new();

    [ObservableProperty]
    private string nombreInput = string.Empty;

    [ObservableProperty]
    private string descripcionInput = string.Empty;

    [ObservableProperty]
    private string precioInput = string.Empty;

    [ObservableProperty]
    private string categoriaInput = "Comida";

    [ObservableProperty]
    private bool isEditing;

    [ObservableProperty]
    private Platillo? platilloEditando;

    public string TituloFormulario => IsEditing ? "Editar platillo" : "Agregar platillo";
    public string TextoBotonGuardar => IsEditing ? "Guardar cambios" : "Agregar platillo";
    public bool HayPlatillos => Platillos.Count > 0;
    public bool NoHayPlatillos => !HayPlatillos;

    public MenuManagementPageModel(MenuService menuService)
    {
        _menuService = menuService;
    }

    partial void OnIsEditingChanged(bool value)
    {
        OnPropertyChanged(nameof(TituloFormulario));
        OnPropertyChanged(nameof(TextoBotonGuardar));
    }

    [RelayCommand]
    public async Task CargarMenuAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var datos = await _menuService.GetAllAsync();
            Platillos = new ObservableCollection<Platillo>(datos);

            RefrescarLista();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo cargar el menú: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GuardarPlatillo()
    {
        if (string.IsNullOrWhiteSpace(NombreInput))
        {
            await Shell.Current.DisplayAlertAsync("Dato faltante", "Ingresa el nombre del platillo.", "OK");
            return;
        }

        if (!TryParsePrecio(PrecioInput, out var precio))
        {
            await Shell.Current.DisplayAlertAsync("Dato inválido", "Ingresa un precio válido.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(CategoriaInput))
        {
            CategoriaInput = "Comida";
        }

        IsBusy = true;

        try
        {
            if (IsEditing && PlatilloEditando != null)
            {
                PlatilloEditando.Nombre = NombreInput.Trim();
                PlatilloEditando.Descripcion = DescripcionInput.Trim();
                PlatilloEditando.Precio = precio;
                PlatilloEditando.Categoria = CategoriaInput.Trim();

                await _menuService.UpdateAsync(PlatilloEditando);

                await Shell.Current.DisplayAlertAsync("Guardado", "El platillo fue actualizado.", "OK");
            }
            else
            {
                var nuevo = new Platillo
                {
                    Nombre = NombreInput.Trim(),
                    Descripcion = DescripcionInput.Trim(),
                    Precio = precio,
                    Categoria = CategoriaInput.Trim(),
                    ImagenSource = "dotnet_bot.svg",
                    EstaDisponible = true
                };

                await _menuService.AddAsync(nuevo);

                await Shell.Current.DisplayAlertAsync("Guardado", "El platillo fue agregado.", "OK");
            }

            LimpiarFormulario();
            await CargarMenuAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo guardar el platillo: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void EditarPlatillo(Platillo platillo)
    {
        if (platillo == null)
        {
            return;
        }

        PlatilloEditando = platillo;
        NombreInput = platillo.Nombre;
        DescripcionInput = platillo.Descripcion;
        PrecioInput = platillo.Precio.ToString("0.##", CultureInfo.InvariantCulture);
        CategoriaInput = platillo.Categoria;
        IsEditing = true;
    }

    [RelayCommand]
    private async Task EliminarPlatillo(Platillo platillo)
    {
        if (platillo == null)
        {
            return;
        }

        var confirmar = await Shell.Current.DisplayAlertAsync(
            "Eliminar platillo",
            $"¿Deseas eliminar {platillo.Nombre}?",
            "Sí",
            "No");

        if (!confirmar)
        {
            return;
        }

        IsBusy = true;

        try
        {
            await _menuService.DeleteAsync(platillo.Id);
            Platillos.Remove(platillo);
            RefrescarLista();

            if (PlatilloEditando?.Id == platillo.Id)
            {
                LimpiarFormulario();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo eliminar el platillo: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CambiarDisponibilidad(Platillo platillo)
    {
        if (platillo == null)
        {
            return;
        }

        try
        {
            var nuevoEstado = !platillo.EstaDisponible;

            await _menuService.ActualizarDisponibilidadAsync(platillo.Id, nuevoEstado);

            platillo.EstaDisponible = nuevoEstado;

            await CargarMenuAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo cambiar la disponibilidad: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private void CancelarEdicion()
    {
        LimpiarFormulario();
    }

    private void LimpiarFormulario()
    {
        NombreInput = string.Empty;
        DescripcionInput = string.Empty;
        PrecioInput = string.Empty;
        CategoriaInput = "Comida";
        PlatilloEditando = null;
        IsEditing = false;
    }

    private void RefrescarLista()
    {
        OnPropertyChanged(nameof(HayPlatillos));
        OnPropertyChanged(nameof(NoHayPlatillos));
    }

    private static bool TryParsePrecio(string value, out double precio)
    {
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.CurrentCulture, out precio))
        {
            return true;
        }

        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out precio);
    }
}