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
    private readonly SupabaseService _supabaseService;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private ObservableCollection<Platillo> platillos = new();

    public ObservableCollection<string> CategoriasDisponibles { get; } = new();

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

    [ObservableProperty]
    private ImageSource imagenMuestra = "dotnet_bot.svg";

    private FileResult? fotoSeleccionada;

    public string TituloFormulario => IsEditing ? "Editar platillo" : "Agregar platillo";
    public string TextoBotonGuardar => IsEditing ? "Guardar cambios" : "Agregar platillo";
    public bool HayPlatillos => Platillos.Count > 0;
    public bool NoHayPlatillos => !HayPlatillos;

    public MenuManagementPageModel(MenuService menuService, SupabaseService supabaseService)
    {
        _menuService = menuService;
        _supabaseService = supabaseService;
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

            var cats = datos.Select(p => p.Categoria)
                            .Where(c => !string.IsNullOrWhiteSpace(c))
                            .Distinct()
                            .OrderBy(c => c)
                            .ToList();
            
            CategoriasDisponibles.Clear();
            foreach (var c in cats)
            {
                CategoriasDisponibles.Add(c);
            }
            if (!CategoriasDisponibles.Any())
            {
                CategoriasDisponibles.Add("Comida");
            }

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

        if (!TryParsePrecio(PrecioInput, out var precio) || precio <= 0)
        {
            await Shell.Current.DisplayAlertAsync("Dato inválido", "Ingresa un precio válido y mayor a 0.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(CategoriaInput))
        {
            CategoriaInput = "Comida";
        }

        IsBusy = true;

        try
        {
            string? nuevaImageUrl = null;
            if (fotoSeleccionada != null)
            {
                nuevaImageUrl = await SubirImagenAsync();
            }

            if (IsEditing && PlatilloEditando != null)
            {
                PlatilloEditando.Nombre = NombreInput.Trim();
                PlatilloEditando.Descripcion = DescripcionInput.Trim();
                PlatilloEditando.Precio = precio;
                PlatilloEditando.Categoria = CategoriaInput.Trim();
                if (nuevaImageUrl != null)
                {
                    PlatilloEditando.ImagenSource = nuevaImageUrl;
                }

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
                    ImagenSource = nuevaImageUrl ?? "dotnet_bot.svg",
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
    private async Task SeleccionarImagen()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions { Title = "Selecciona una imagen" });
            if (photo != null)
            {
                fotoSeleccionada = photo;
                var stream = await photo.OpenReadAsync();
                ImagenMuestra = ImageSource.FromStream(() => stream);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo abrir la galería: {ex.Message}", "OK");
        }
    }

    private async Task<string?> SubirImagenAsync()
    {
        if (fotoSeleccionada == null) return null;

        try
        {
            var client = await _supabaseService.GetClientAsync();
            using var stream = await fotoSeleccionada.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(fotoSeleccionada.FileName)}";
            
            await client.Storage.From("platillos").Upload(bytes, fileName, new Supabase.Storage.FileOptions { Upsert = true });
            return client.Storage.From("platillos").GetPublicUrl(fileName);
        }
        catch
        {
            return null;
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
        ImagenMuestra = platillo.ImagenSource;
        fotoSeleccionada = null;
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
    private async Task AgregarCategoria()
    {
        string nuevaCat = await Shell.Current.DisplayPromptAsync("Nueva Categoría", "Ingresa el nombre de la categoría:", "OK", "Cancelar", "Ej. Bebidas, Postres...");
        if (!string.IsNullOrWhiteSpace(nuevaCat))
        {
            string catLimpia = nuevaCat.Trim();
            if (!CategoriasDisponibles.Contains(catLimpia))
            {
                CategoriasDisponibles.Add(catLimpia);
            }
            CategoriaInput = catLimpia;
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
        ImagenMuestra = "dotnet_bot.svg";
        fotoSeleccionada = null;
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