using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComandaApp.Models;

namespace ComandaApp.PageModels;

public partial class MenuManagementPageModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Platillo> platillos = new();

    public MenuManagementPageModel() => cargarMenuInicial();

    [RelayCommand]
    private async Task agregarPlatillo()
    {
        string nombre = await Shell.Current.DisplayPromptAsync("Nuevo Platillo", "Nombre:");
        if (string.IsNullOrWhiteSpace(nombre)) return;

        string precioStr = await Shell.Current.DisplayPromptAsync("Precio", $"Precio para {nombre}:", keyboard: Keyboard.Numeric);
        double.TryParse(precioStr, out double precio);

        string categoria = await Shell.Current.DisplayActionSheet("Categoría", "Cancelar", null, "Comida", "Bebida");
        if (categoria == "Cancelar") return;

        var nuevo = new Platillo
        {
            Nombre = nombre,
            Precio = precio,
            Categoria = categoria,
            ImagenSource = categoria == "Comida" ? "food_placeholder.png" : "drink_placeholder.png"
        };

        Platillos.Add(nuevo);
    }

    [RelayCommand]
    private void eliminarPlatillo(Platillo p) => Platillos.Remove(p);

    private void cargarMenuInicial()
    {
        Platillos.Add(new Platillo { Nombre = "Hamburguesa", Precio = 150, Categoria = "Comida" });
        Platillos.Add(new Platillo { Nombre = "Cerveza Nacional", Precio = 45, Categoria = "Bebida" });
    }
}