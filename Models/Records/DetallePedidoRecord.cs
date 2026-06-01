using Postgrest.Attributes;
using Postgrest.Models;
using ComandaApp.Models;

namespace ComandaApp.Models.Records;

[Table("detalles_pedido")]
public class DetallePedidoRecord : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Column("orden_id")]
    public string OrdenId { get; set; } = string.Empty;

    [Column("nombre_platillo")]
    public string NombrePlatillo { get; set; } = string.Empty;

    [Column("cantidad")]
    public int Cantidad { get; set; }

    [Column("precio_unitario")]
    public double PrecioUnitario { get; set; }

    [Column("notas")]
    public string Notas { get; set; } = string.Empty;

    [Column("estado")]
    public string Estado { get; set; } = "Pendiente";

    [Column("negocio_id")]
    public string NegocioId { get; set; } = string.Empty;
}

public static class DetallePedidoMapping
{
    public static DetallePedido ToModel(this DetallePedidoRecord r) => new()
    {
        Id = r.Id,
        NombrePlatillo = r.NombrePlatillo,
        Cantidad = r.Cantidad,
        PrecioUnitario = r.PrecioUnitario,
        Notas = r.Notas,
        Estado = Enum.Parse<EstadoPedido>(r.Estado)
    };

    public static DetallePedidoRecord ToRecord(this DetallePedido m, string ordenId, string negocioId) => new()
    {
        Id = m.Id,
        OrdenId = ordenId,
        NombrePlatillo = m.NombrePlatillo,
        Cantidad = m.Cantidad,
        PrecioUnitario = m.PrecioUnitario,
        Notas = m.Notas,
        Estado = m.Estado.ToString(),
        NegocioId = negocioId
    };
}