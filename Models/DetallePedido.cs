using CommunityToolkit.Mvvm.ComponentModel;

namespace ComandaApp.Models;

// Representa un platillo individual dentro de una orden
public partial class DetallePedido : ObservableObject
{
    [ObservableProperty]
    private string id = Guid.NewGuid().ToString();

    [ObservableProperty]
    private string nombrePlatillo = string.Empty;

    [ObservableProperty]
    private int cantidad;

    [ObservableProperty]
    private double precioUnitario;

    [ObservableProperty]
    private string notas = string.Empty; // Ej. "Sin cebolla"

    [ObservableProperty]
    private EstadoPedido estado = EstadoPedido.Pendiente;

    // Propiedad calculada, no necesita campo de respaldo
    public double Subtotal => Cantidad * PrecioUnitario;
}