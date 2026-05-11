using CommunityToolkit.Mvvm.ComponentModel;

namespace ComandaApp.Models;

public partial class Platillo : ObservableObject
{
    [ObservableProperty]
    private string id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string nombre = string.Empty;

    [ObservableProperty]
    private string descripcion = string.Empty;

    [ObservableProperty]
    private double precio;

    [ObservableProperty]
    private string categoria = "Comida"; // Puede ser Comida, Bebida, Postre

    // Esta es la propiedad clave que la cocina modificará
    [ObservableProperty]
    private bool estaDisponible = true;
}