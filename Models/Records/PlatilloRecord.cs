using Postgrest.Attributes;
using Postgrest.Models;
using ComandaApp.Models;

namespace ComandaApp.Models.Records;

[Table("platillos")]
public class PlatilloRecord : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [Column("precio")]
    public double Precio { get; set; }

    [Column("categoria")]
    public string Categoria { get; set; } = "Comida";

    [Column("imagen_url")]
    public string ImagenUrl { get; set; } = "dotnet_bot.svg";

    [Column("disponible")]
    public bool Disponible { get; set; } = true;

    [Column("negocio_id")]
    public string NegocioId { get; set; } = string.Empty;
}

public static class PlatilloMapping
{
    public static Platillo ToModel(this PlatilloRecord r) => new()
    {
        Id = r.Id,
        Nombre = r.Nombre,
        Descripcion = r.Descripcion,
        Precio = r.Precio,
        Categoria = r.Categoria,
        ImagenSource = r.ImagenUrl,
        EstaDisponible = r.Disponible
    };

    public static PlatilloRecord ToRecord(this Platillo m, string negocioId) => new()
    {
        Id = m.Id,
        Nombre = m.Nombre,
        Descripcion = m.Descripcion,
        Precio = m.Precio,
        Categoria = m.Categoria,
        ImagenUrl = m.ImagenSource,
        Disponible = m.EstaDisponible,
        NegocioId = negocioId
    };
}