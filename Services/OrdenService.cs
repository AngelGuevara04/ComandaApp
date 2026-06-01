using ComandaApp.Models;
using ComandaApp.Models.Records;

namespace ComandaApp.Services;

public class OrdenService
{
    private readonly SupabaseService _supabaseService;
    private readonly AuthService _authService;

    public OrdenService(SupabaseService supabaseService, AuthService authService)
    {
        _supabaseService = supabaseService;
        _authService = authService;
    }

    public async Task<List<OrdenMesa>> GetOrdenesActivasAsync()
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        var ordenesResult = await client.From<OrdenRecord>().Get();

        var ordenes = ordenesResult.Models
            .Where(o => o.NegocioId == negocioId && o.EstaPagada == false)
            .OrderBy(o => o.NumeroMesa)
            .ThenBy(o => o.FechaCreacion)
            .Select(o => o.ToModel())
            .ToList();

        foreach (var orden in ordenes)
        {
            await CargarDetallesAsync(orden);
        }

        return ordenes;
    }

    public async Task<List<OrdenMesa>> GetOrdenesPagadasPorFechaAsync(DateTime fecha)
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        var ordenesResult = await client.From<OrdenRecord>().Get();

        var ordenes = ordenesResult.Models
            .Where(o => o.NegocioId == negocioId && o.EstaPagada)
            .Select(o => o.ToModel())
            .Where(o => o.FechaCreacion.Date == fecha.Date)
            .OrderByDescending(o => o.FechaCreacion)
            .ToList();

        foreach (var orden in ordenes)
        {
            await CargarDetallesAsync(orden);
        }

        return ordenes;
    }

    public async Task<OrdenMesa?> GetOrdenActivaAsync(string numeroMesa)
    {
        try
        {
            var client = await _supabaseService.GetClientAsync();
            var negocioId = _authService.NegocioIdActual;

            var ordenesResult = await client.From<OrdenRecord>().Get();

            var ordenRecord = ordenesResult.Models.FirstOrDefault(o =>
                o.NegocioId == negocioId &&
                o.EstaPagada == false &&
                NormalizarMesa(o.NumeroMesa) == NormalizarMesa(numeroMesa));

            if (ordenRecord == null)
            {
                return null;
            }

            var orden = ordenRecord.ToModel();

            await CargarDetallesAsync(orden);

            return orden;
        }
        catch
        {
            return null;
        }
    }

    public async Task<OrdenMesa> CrearOrdenAsync(string numeroMesa, string nombreCliente = "")
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        var nuevaOrden = new OrdenMesa
        {
            NombreCliente = nombreCliente
        };

        nuevaOrden.MesaAsignada.NumeroMesa = numeroMesa;

        await client.From<OrdenRecord>()
            .Insert(nuevaOrden.ToRecord(negocioId));

        return nuevaOrden;
    }

    public async Task<OrdenMesa> CrearOrdenParaCajaAsync(string numeroMesa, string nombreCliente)
    {
        var ordenActiva = await GetOrdenActivaAsync(numeroMesa);

        if (ordenActiva != null)
        {
            await ActualizarNombreClienteAsync(ordenActiva.IdOrden, nombreCliente);
            ordenActiva.NombreCliente = nombreCliente;
            return ordenActiva;
        }

        return await CrearOrdenAsync(numeroMesa, nombreCliente);
    }

    public async Task ActualizarNombreClienteAsync(string idOrden, string nombreCliente)
    {
        var client = await _supabaseService.GetClientAsync();

        await client.From<OrdenRecord>()
            .Where(o => o.Id == idOrden)
            .Set(o => o.NombreCliente, nombreCliente)
            .Update();
    }

    public async Task AgregarDetalleAsync(string idOrden, DetallePedido detalle)
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        await client.From<DetallePedidoRecord>()
            .Insert(detalle.ToRecord(idOrden, negocioId));
    }

    public async Task CerrarOrdenAsync(string idOrden)
    {
        var client = await _supabaseService.GetClientAsync();

        await client.From<OrdenRecord>()
            .Where(o => o.Id == idOrden)
            .Set(o => o.EstaPagada, true)
            .Update();
    }

    public async Task ActualizarEstadoDetalleAsync(string idDetalle, EstadoPedido estado)
    {
        var client = await _supabaseService.GetClientAsync();

        await client.From<DetallePedidoRecord>()
            .Where(d => d.Id == idDetalle)
            .Set(d => d.Estado, estado.ToString())
            .Update();
    }

    private async Task CargarDetallesAsync(OrdenMesa orden)
    {
        var client = await _supabaseService.GetClientAsync();
        var negocioId = _authService.NegocioIdActual;

        var detalles = await client.From<DetallePedidoRecord>().Get();

        orden.Platillos.Clear();

        foreach (var detalle in detalles.Models.Where(d =>
            d.NegocioId == negocioId &&
            d.OrdenId == orden.IdOrden))
        {
            orden.Platillos.Add(detalle.ToModel());
        }
    }

    private static string NormalizarMesa(string value)
    {
        return value
            .Trim()
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .ToLowerInvariant();
    }
}