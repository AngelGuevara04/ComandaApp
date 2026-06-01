using Postgrest.Attributes;
using Postgrest.Models;
using ComandaApp.Models;

namespace ComandaApp.Models.Records;

[Table("configuracion_negocio")]
public class ConfiguracionRecord : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("nombre_restaurante")]
    public string NombreRestaurante { get; set; } = string.Empty;

    [Column("rfc")]
    public string Rfc { get; set; } = string.Empty;

    [Column("direccion")]
    public string Direccion { get; set; } = string.Empty;

    [Column("logo_url")]
    public string LogoUrl { get; set; } = string.Empty;

    [Column("negocio_id")]
    public string NegocioId { get; set; } = string.Empty;
}

public static class ConfiguracionMapping
{
    public static ConfiguracionNegocio ToModel(this ConfiguracionRecord r) => new()
    {
        NombreRestaurante = r.NombreRestaurante,
        Rfc = r.Rfc,
        Direccion = r.Direccion,
        LogoUrl = r.LogoUrl
    };

    public static ConfiguracionRecord ToRecord(this ConfiguracionNegocio m, string negocioId, int id) => new()
    {
        Id = id,
        NombreRestaurante = m.NombreRestaurante,
        Rfc = m.Rfc,
        Direccion = m.Direccion,
        LogoUrl = m.LogoUrl,
        NegocioId = negocioId
    };
}