using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ComandaApp.Models;

// Representa la cuenta completa de una mesa.
public partial class OrdenMesa : ObservableObject
{
    [ObservableProperty]
    private string idOrden = Guid.NewGuid().ToString();

    [ObservableProperty]
    private Mesa mesaAsignada = new();

    [ObservableProperty]
    private string nombreCliente = string.Empty;

    [ObservableProperty]
    private DateTime fechaCreacion = DateTime.Now;

    [ObservableProperty]
    private bool estaPagada;

    [ObservableProperty]
    private ObservableCollection<DetallePedido> platillos = new();

    // Calcula el total de la cuenta.
    public double TotalCuenta => Platillos
        .Where(p => p.Estado == EstadoPedido.Listo)
        .Sum(p => p.Subtotal);
}