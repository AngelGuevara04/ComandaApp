using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ComandaApp.Models;

namespace ComandaApp.PageModels;

public partial class KitchenDashboardPageModel : ObservableObject
{
    // Lista de pedidos que los clientes envían (Pendientes y En Preparación)
    [ObservableProperty]
    private ObservableCollection<DetallePedido> pedidosActivos = new();

    // Lista de platillos del menú para que la cocina controle existencias
    [ObservableProperty]
    private ObservableCollection<Platillo> menuRestaurante = new();

    public KitchenDashboardPageModel()
    {
        // NOTA: Aquí a futuro conectaremos el Servicio de WebSockets.
        // Por ahora, cargamos datos de prueba para diseñar la UI.
        cargarDatosPrueba();
    }

    [RelayCommand]
    private void marcarEnPreparacion(DetallePedido pedido)
    {
        if (pedido.Estado == EstadoPedido.Pendiente)
        {
            pedido.Estado = EstadoPedido.EnPreparacion;
            // Aquí enviaríamos un evento por WebSocket al cliente avisando que su plato se está cocinando
        }
    }

    [RelayCommand]
    private void marcarListo(DetallePedido pedido)
    {
        if (pedido.Estado == EstadoPedido.EnPreparacion || pedido.Estado == EstadoPedido.Pendiente)
        {
            pedido.Estado = EstadoPedido.Listo;

            // Una vez listo, el mesero se lo lleva, así que lo quitamos de la pantalla de cocina
            PedidosActivos.Remove(pedido);

            // Aquí enviaríamos el evento WebSocket para que el mesero sepa que debe recogerlo
        }
    }

    [RelayCommand]
    private async Task rechazarPedido(DetallePedido pedido)
    {
        bool confirmacion = await Shell.Current.DisplayAlert(
            "Rechazar Pedido",
            $"¿Seguro que deseas rechazar {pedido.Cantidad}x {pedido.NombrePlatillo}? Se notificará al cliente.",
            "Sí, rechazar", "Cancelar");

        if (confirmacion)
        {
            pedido.Estado = EstadoPedido.Rechazado;
            PedidosActivos.Remove(pedido);
            // Aquí notificaríamos al cliente por WebSocket
        }
    }

    [RelayCommand]
    private void alternarDisponibilidad(Platillo platillo)
    {
        platillo.EstaDisponible = !platillo.EstaDisponible;
        // Al cambiar este valor, gracias a ObservableObject, la UI se actualiza sola.
        // Aquí enviaríamos por WebSocket un aviso global: "Actualizar menú de clientes".
    }

    private void cargarDatosPrueba()
    {
        PedidosActivos.Add(new DetallePedido { NombrePlatillo = "Hamburguesa Clásica", Cantidad = 2, Notas = "Sin tomate" });
        PedidosActivos.Add(new DetallePedido { NombrePlatillo = "Tacos al Pastor", Cantidad = 5, Notas = "Con todo" });

        MenuRestaurante.Add(new Platillo { Nombre = "Hamburguesa Clásica", Precio = 120, EstaDisponible = true });
        MenuRestaurante.Add(new Platillo { Nombre = "Tacos al Pastor", Precio = 80, EstaDisponible = true });
        MenuRestaurante.Add(new Platillo { Nombre = "Sopa Azteca", Precio = 90, EstaDisponible = false });
    }
}