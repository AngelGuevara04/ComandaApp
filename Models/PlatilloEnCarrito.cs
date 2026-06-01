using CommunityToolkit.Mvvm.ComponentModel;

namespace ComandaApp.Models;

// Envuelve un Platillo con su cantidad actual en el carrito.
// Se usa en la CollectionView del menu para mostrar controles +/- por platillo.
public partial class PlatilloEnCarrito : ObservableObject
{
    public Platillo Platillo { get; }

    [ObservableProperty]
    private int cantidad;

    // Se actualiza automaticamente cuando cambia Cantidad.
    public bool EstaEnCarrito => Cantidad > 0;

    partial void OnCantidadChanged(int value)
        => OnPropertyChanged(nameof(EstaEnCarrito));

    public PlatilloEnCarrito(Platillo platillo)
    {
        Platillo = platillo;
    }
}