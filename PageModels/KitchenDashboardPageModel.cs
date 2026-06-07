using System.Collections.ObjectModel;
using ComandaApp.Models;
using ComandaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ComandaApp.PageModels;

public partial class KitchenDashboardPageModel : ObservableObject
{
    private readonly OrdenService _ordenService;
    private readonly MenuService _menuService;
    private readonly AuthService _authService;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private ObservableCollection<OrdenCocinaGroup> ordenesActivas = new();

    [ObservableProperty]
    private OrdenCocinaGroup? ordenSeleccionada;

    [ObservableProperty]
    private ObservableCollection<Platillo> menuRestaurante = new();

    [ObservableProperty]
    private bool mostrandoOrdenes = true;

    [ObservableProperty]
    private bool mostrandoDetalle = false;

    [ObservableProperty]
    private bool mostrandoDisponibilidad = false;

    public bool HayOrdenes => OrdenesActivas.Count > 0;
    public bool NoHayOrdenes => !HayOrdenes;

    public KitchenDashboardPageModel(
        OrdenService ordenService,
        MenuService menuService,
        AuthService authService)
    {
        _ordenService = ordenService;
        _menuService = menuService;
        _authService = authService;
    }

    [RelayCommand]
    public async Task CargarDatosAsync()
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var ordenes = await _ordenService.GetOrdenesActivasAsync();
            var platillos = await _menuService.GetAllAsync();

            var grupos = new List<OrdenCocinaGroup>();

            foreach (var orden in ordenes)
            {
                var pedidosGrupo = new List<PedidoCocinaItem>();

                foreach (var detalle in orden.Platillos)
                {
                    if (detalle.Estado == EstadoPedido.Pendiente ||
                        detalle.Estado == EstadoPedido.EnPreparacion)
                    {
                        pedidosGrupo.Add(new PedidoCocinaItem
                        {
                            IdDetalle = detalle.Id,
                            IdOrden = orden.IdOrden,
                            NumeroMesa = orden.MesaAsignada.NumeroMesa,
                            NombreCliente = orden.NombreCliente,
                            NombrePlatillo = detalle.NombrePlatillo,
                            Cantidad = detalle.Cantidad,
                            PrecioUnitario = detalle.PrecioUnitario,
                            Notas = detalle.Notas,
                            Estado = detalle.Estado
                        });
                    }
                }

                if (pedidosGrupo.Count > 0)
                {
                    grupos.Add(new OrdenCocinaGroup
                    {
                        IdOrden = orden.IdOrden,
                        NumeroMesa = orden.MesaAsignada.NumeroMesa,
                        NombreCliente = orden.NombreCliente,
                        FechaCreacion = orden.FechaCreacion,
                        Platillos = new ObservableCollection<PedidoCocinaItem>(
                            pedidosGrupo.OrderBy(p => p.NombrePlatillo))
                    });
                }
            }

            OrdenesActivas = new ObservableCollection<OrdenCocinaGroup>(
                grupos.OrderBy(g => g.FechaCreacion));

            MenuRestaurante = new ObservableCollection<Platillo>(platillos);

            if (OrdenSeleccionada != null)
            {
                var ordenActualizada = grupos.FirstOrDefault(g => g.IdOrden == OrdenSeleccionada.IdOrden);
                if (ordenActualizada != null)
                {
                    OrdenSeleccionada = ordenActualizada;
                }
                else
                {
                    VolverAOrdenes();
                }
            }

            RefrescarEstadoLista();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudieron cargar los datos: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task MarcarEnPreparacion(PedidoCocinaItem pedido)
    {
        if (pedido == null)
        {
            return;
        }

        if (pedido.Estado != EstadoPedido.Pendiente)
        {
            return;
        }

        try
        {
            await _ordenService.ActualizarEstadoDetalleAsync(pedido.IdDetalle, EstadoPedido.EnPreparacion);
            pedido.Estado = EstadoPedido.EnPreparacion;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo actualizar el pedido: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task MarcarListo(PedidoCocinaItem pedido)
    {
        if (pedido == null)
        {
            return;
        }

        if (pedido.Estado != EstadoPedido.Pendiente &&
            pedido.Estado != EstadoPedido.EnPreparacion)
        {
            return;
        }

        try
        {
            await _ordenService.ActualizarEstadoDetalleAsync(pedido.IdDetalle, EstadoPedido.Listo);
            pedido.Estado = EstadoPedido.Listo;

            if (OrdenSeleccionada != null)
            {
                OrdenSeleccionada.Platillos.Remove(pedido);
                if (OrdenSeleccionada.Platillos.Count == 0)
                {
                    VolverAOrdenes();
                    await CargarDatosAsync();
                }
            }
            RefrescarEstadoLista();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo marcar como listo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task RechazarPedido(PedidoCocinaItem pedido)
    {
        if (pedido == null)
        {
            return;
        }

        var confirmar = await Shell.Current.DisplayAlertAsync(
            "Rechazar pedido",
            $"¿Seguro que deseas rechazar {pedido.Cantidad}x {pedido.NombrePlatillo}?",
            "Sí",
            "No");

        if (!confirmar)
        {
            return;
        }

        try
        {
            await _ordenService.ActualizarEstadoDetalleAsync(pedido.IdDetalle, EstadoPedido.Rechazado);
            pedido.Estado = EstadoPedido.Rechazado;

            if (OrdenSeleccionada != null)
            {
                OrdenSeleccionada.Platillos.Remove(pedido);
                if (OrdenSeleccionada.Platillos.Count == 0)
                {
                    VolverAOrdenes();
                    await CargarDatosAsync();
                }
            }
            RefrescarEstadoLista();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo rechazar el pedido: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task AlternarDisponibilidad(Platillo platillo)
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

            if (!nuevoEstado)
            {
                await _ordenService.RechazarDetallesPorPlatilloAsync(platillo.Nombre);
                await CargarDatosAsync();
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo actualizar el platillo: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task SalirRol()
    {
        var confirmar = await Shell.Current.DisplayAlertAsync(
            "Salir de cocina",
            "¿Deseas salir del panel de cocina?",
            "Sí",
            "No");

        if (!confirmar)
        {
            return;
        }

        _authService.Logout();

        await Shell.Current.GoToAsync("//login");
    }

    [RelayCommand]
    private async Task VerOrdenes()
    {
        MostrandoOrdenes = true;
        MostrandoDetalle = false;
        MostrandoDisponibilidad = false;
        await CargarDatosAsync();
    }

    [RelayCommand]
    private async Task VerDisponibilidad()
    {
        MostrandoOrdenes = false;
        MostrandoDetalle = false;
        MostrandoDisponibilidad = true;
        await CargarDatosAsync();
    }

    [RelayCommand]
    private async Task AbrirOrden(OrdenCocinaGroup orden)
    {
        if (orden == null) return;
        
        await Task.Delay(100); // Prevent MAUI CollectionView crash when hiding parent
        
        MainThread.BeginInvokeOnMainThread(() =>
        {
            OrdenSeleccionada = orden;
            MostrandoOrdenes = false;
            MostrandoDetalle = true;
            MostrandoDisponibilidad = false;
        });
    }

    [RelayCommand]
    private void VolverAOrdenes()
    {
        OrdenSeleccionada = null;
        MostrandoOrdenes = true;
        MostrandoDetalle = false;
        MostrandoDisponibilidad = false;
    }

    private void RefrescarEstadoLista()
    {
        OnPropertyChanged(nameof(HayOrdenes));
        OnPropertyChanged(nameof(NoHayOrdenes));
    }
}

public partial class OrdenCocinaGroup : ObservableObject
{
    [ObservableProperty]
    private string idOrden = string.Empty;

    [ObservableProperty]
    private string numeroMesa = string.Empty;

    [ObservableProperty]
    private string nombreCliente = string.Empty;

    [ObservableProperty]
    private DateTime fechaCreacion;

    [ObservableProperty]
    private ObservableCollection<PedidoCocinaItem> platillos = new();

    public int TotalPlatillos => Platillos.Count;
    public string TiempoTranscurrido => $"{(int)(DateTime.Now - FechaCreacion).TotalMinutes} min";
}

public partial class PedidoCocinaItem : ObservableObject
{
    [ObservableProperty]
    private string idDetalle = string.Empty;

    [ObservableProperty]
    private string idOrden = string.Empty;

    [ObservableProperty]
    private string numeroMesa = string.Empty;

    [ObservableProperty]
    private string nombreCliente = string.Empty;

    [ObservableProperty]
    private string nombrePlatillo = string.Empty;

    [ObservableProperty]
    private int cantidad;

    [ObservableProperty]
    private double precioUnitario;

    [ObservableProperty]
    private string notas = string.Empty;

    [ObservableProperty]
    private EstadoPedido estado = EstadoPedido.Pendiente;

    public double Subtotal => Cantidad * PrecioUnitario;

    public bool TieneNotas => !string.IsNullOrWhiteSpace(Notas);

    public bool TieneCliente => !string.IsNullOrWhiteSpace(NombreCliente);

    public bool PuedePreparar => Estado == EstadoPedido.Pendiente;

    public bool PuedeMarcarListo =>
        Estado == EstadoPedido.Pendiente ||
        Estado == EstadoPedido.EnPreparacion;

    partial void OnEstadoChanged(EstadoPedido value)
    {
        OnPropertyChanged(nameof(PuedePreparar));
        OnPropertyChanged(nameof(PuedeMarcarListo));
    }

    partial void OnNotasChanged(string value)
    {
        OnPropertyChanged(nameof(TieneNotas));
    }

    partial void OnNombreClienteChanged(string value)
    {
        OnPropertyChanged(nameof(TieneCliente));
    }
}