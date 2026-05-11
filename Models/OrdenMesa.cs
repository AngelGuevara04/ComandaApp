using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ComandaApp.Models;

// Representa la cuenta completa de una mesa en su sesión activa
public partial class OrdenMesa : ObservableObject
{
    [ObservableProperty]
    private string idOrden = Guid.NewGuid().ToString();

    [ObservableProperty]
    private Mesa mesaAsignada = new();

    [ObservableProperty]
    private string nombreCliente = string.Empty; // Útil si es un QR temporal

    [ObservableProperty]
    private DateTime fechaCreacion = DateTime.Now;

    [ObservableProperty]
    private bool estaPagada;

    // Lista observable de los platillos pedidos
    [ObservableProperty]
    private ObservableCollection<DetallePedido> platillos = new();

    // Calcula el total a pagar sumando los subtotales de platillos que NO fueron rechazados
    public double TotalCuenta => platillos
        .Where(p => p.Estado != EstadoPedido.Rechazado)
        .Sum(p => p.Subtotal);
}