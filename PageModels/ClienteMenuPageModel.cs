using ComandaApp.Models;
using ComandaApp.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ComandaApp.PageModels;

[QueryProperty(nameof(NumeroMesa), "numeroMesa")]
public partial class ClienteMenuPageModel : ObservableObject
{
    private readonly MenuService _menuService;
    private readonly OrdenService _ordenService;
    private readonly HashSet<string> _pedidosListosNotificados = new();

    private bool _ordenYaFueCargada;
    private bool _clienteExpulsado;

    [ObservableProperty]
    private string numeroMesa = string.Empty;

    [ObservableProperty]
    private string idOrden = string.Empty;

    [ObservableProperty]
    private bool mostrandoMenu = true;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private ObservableCollection<PlatilloEnCarrito> platillosConCantidad = new();

    [ObservableProperty]
    private ObservableCollection<DetallePedido> pedidosConfirmados = new();

    public bool MostrandoMisPedidos => !MostrandoMenu;
    public bool TieneItemsEnCarrito => ItemsEnCarrito > 0;
    public bool PuedeSolicitarPago => MostrandoMisPedidos && PedidosConfirmados.Count > 0;
    public bool HayPlatillos => PlatillosConCantidad.Count > 0;
    public bool NoHayPlatillos => !HayPlatillos;
    public bool HayPedidos => PedidosConfirmados.Count > 0;
    public bool NoHayPedidos => !HayPedidos;

    public int ItemsEnCarrito => PlatillosConCantidad.Sum(p => p.Cantidad);

    public double TotalCarrito => PlatillosConCantidad.Sum(p => p.Cantidad * p.Platillo.Precio);

    public double TotalConfirmado => PedidosConfirmados
        .Where(p => p.Estado != EstadoPedido.Rechazado)
        .Sum(p => p.Subtotal);

    public double TotalGeneral => TotalCarrito + TotalConfirmado;

    partial void OnMostrandoMenuChanged(bool value)
    {
        OnPropertyChanged(nameof(MostrandoMisPedidos));
        OnPropertyChanged(nameof(PuedeSolicitarPago));
    }

    public ClienteMenuPageModel(MenuService menuService, OrdenService ordenService)
    {
        _menuService = menuService;
        _ordenService = ordenService;
    }

    public async Task InicializarAsync()
    {
        _clienteExpulsado = false;
        MostrandoMenu = true;

        await CargarDatosClienteAsync(true, false);
    }

    public async Task ActualizarDatosAsync()
    {
        await CargarDatosClienteAsync(false, true);
    }

    [RelayCommand]
    private async Task Actualizar()
    {
        await CargarDatosClienteAsync(true, true);
    }

    private async Task CargarDatosClienteAsync(bool mostrarErrores, bool permitirExpulsion)
    {
        if (_clienteExpulsado)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(NumeroMesa))
        {
            return;
        }

        if (IsBusy)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var cantidadesActuales = PlatillosConCantidad
                .ToDictionary(p => p.Platillo.Id, p => p.Cantidad);

            var platillos = await _menuService.GetDisponiblesAsync();

            PlatillosConCantidad = new ObservableCollection<PlatilloEnCarrito>(
                platillos.Select(p =>
                {
                    var item = new PlatilloEnCarrito(p);

                    if (cantidadesActuales.TryGetValue(p.Id, out var cantidad))
                    {
                        item.Cantidad = cantidad;
                    }

                    return item;
                }));

            var ordenActiva = await _ordenService.GetOrdenActivaAsync(NumeroMesa);

            if (ordenActiva != null)
            {
                IdOrden = ordenActiva.IdOrden;
                _ordenYaFueCargada = true;

                var pedidosActuales = ordenActiva.Platillos.ToList();

                await RevisarPedidosListosAsync(pedidosActuales, permitirExpulsion);

                PedidosConfirmados = new ObservableCollection<DetallePedido>(pedidosActuales);
            }
            else
            {
                if (_ordenYaFueCargada && permitirExpulsion)
                {
                    await ExpulsarClienteAsync();
                    return;
                }

                var nuevaOrden = await _ordenService.CrearOrdenAsync(NumeroMesa);

                IdOrden = nuevaOrden.IdOrden;
                _ordenYaFueCargada = true;

                PedidosConfirmados = new ObservableCollection<DetallePedido>();
                _pedidosListosNotificados.Clear();
            }

            RefrescarTotales();
        }
        catch (Exception ex)
        {
            if (mostrarErrores)
            {
                await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExpulsarClienteAsync()
    {
        _clienteExpulsado = true;

        PedidosConfirmados.Clear();
        PlatillosConCantidad.Clear();

        IdOrden = string.Empty;
        _ordenYaFueCargada = false;

        RefrescarTotales();

        await Shell.Current.DisplayAlertAsync(
            "Cuenta cerrada",
            "Caja cerró la cuenta de esta mesa. Serás enviado al inicio.",
            "OK");

        await Shell.Current.GoToAsync("//login");
    }

    private async Task RevisarPedidosListosAsync(List<DetallePedido> pedidos, bool permitirNotificaciones)
    {
        var nuevosListos = new List<DetallePedido>();

        foreach (var pedido in pedidos)
        {
            var clave = ObtenerClavePedido(pedido);

            if (pedido.Estado != EstadoPedido.Listo)
            {
                continue;
            }

            if (!permitirNotificaciones)
            {
                _pedidosListosNotificados.Add(clave);
                continue;
            }

            if (_pedidosListosNotificados.Contains(clave))
            {
                continue;
            }

            _pedidosListosNotificados.Add(clave);
            nuevosListos.Add(pedido);
        }

        if (nuevosListos.Count == 0)
        {
            return;
        }

        var mensaje = string.Join(
            Environment.NewLine,
            nuevosListos.Select(p => $"{p.Cantidad}x {p.NombrePlatillo}"));

        await Shell.Current.DisplayAlertAsync(
            "Pedido listo",
            $"Tu pedido está listo:\n\n{mensaje}",
            "OK");
    }

    private static string ObtenerClavePedido(DetallePedido pedido)
    {
        if (!string.IsNullOrWhiteSpace(pedido.Id))
        {
            return pedido.Id;
        }

        return $"{pedido.NombrePlatillo}|{pedido.Cantidad}|{pedido.PrecioUnitario}|{pedido.Estado}";
    }

    [RelayCommand]
    private void Agregar(PlatilloEnCarrito item)
    {
        item.Cantidad++;
        RefrescarTotales();
    }

    [RelayCommand]
    private void Quitar(PlatilloEnCarrito item)
    {
        if (item.Cantidad > 0)
        {
            item.Cantidad--;
            RefrescarTotales();
        }
    }

    [RelayCommand]
    private async Task ConfirmarPedido()
    {
        var itemsAPedir = PlatillosConCantidad.Where(p => p.Cantidad > 0).ToList();

        if (!itemsAPedir.Any())
        {
            return;
        }

        IsBusy = true;

        try
        {
            foreach (var item in itemsAPedir)
            {
                var detalle = new DetallePedido
                {
                    NombrePlatillo = item.Platillo.Nombre,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = item.Platillo.Precio
                };

                await _ordenService.AgregarDetalleAsync(IdOrden, detalle);

                PedidosConfirmados.Add(detalle);
                item.Cantidad = 0;
            }

            RefrescarTotales();
            MostrandoMenu = false;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"No se pudo confirmar el pedido: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void VerMenu()
    {
        MostrandoMenu = true;
    }

    [RelayCommand]
    private void VerMisPedidos()
    {
        MostrandoMenu = false;
    }

    [RelayCommand]
    private async Task SolicitarPago()
    {
        await Shell.Current.DisplayAlertAsync(
            "Solicitud de pago",
            $"Total a pagar: ${TotalConfirmado:F2}\n\nPor favor pasa a caja para realizar tu pago.",
            "OK");
    }

    private void RefrescarTotales()
    {
        OnPropertyChanged(nameof(TotalCarrito));
        OnPropertyChanged(nameof(TotalConfirmado));
        OnPropertyChanged(nameof(TotalGeneral));
        OnPropertyChanged(nameof(ItemsEnCarrito));
        OnPropertyChanged(nameof(TieneItemsEnCarrito));
        OnPropertyChanged(nameof(PuedeSolicitarPago));
        OnPropertyChanged(nameof(HayPlatillos));
        OnPropertyChanged(nameof(NoHayPlatillos));
        OnPropertyChanged(nameof(HayPedidos));
        OnPropertyChanged(nameof(NoHayPedidos));
    }
}