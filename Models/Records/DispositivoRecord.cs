using Postgrest.Attributes;
using Postgrest.Models;
using ComandaApp.Models;

namespace ComandaApp.Models.Records;

[Table("dispositivos")]
public class DispositivoRecord : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("rol")]
    public string Rol { get; set; } = string.Empty;

    [Column("detalle_extra")]
    public string DetalleExtra { get; set; } = string.Empty;

    [Column("qr_code_data")]
    public string QrCodeData { get; set; } = string.Empty;

    [Column("fecha_vinculacion")]
    public DateTime FechaVinculacion { get; set; } = DateTime.UtcNow;

    [Column("negocio_id")]
    public string NegocioId { get; set; } = string.Empty;
}

public static class DispositivoMapping
{
    public static Dispositivo ToModel(this DispositivoRecord r) => new()
    {
        Id = r.Id,
        Nombre = r.Nombre,
        Rol = Enum.Parse<RolDispositivo>(r.Rol),
        DetalleExtra = r.DetalleExtra,
        QrCodeData = r.QrCodeData,
        FechaVinculacion = r.FechaVinculacion.ToLocalTime()
    };

    public static DispositivoRecord ToRecord(this Dispositivo m, string negocioId) => new()
    {
        Id = m.Id,
        Nombre = m.Nombre,
        Rol = m.Rol.ToString(),
        DetalleExtra = m.DetalleExtra,
        QrCodeData = m.QrCodeData,
        FechaVinculacion = m.FechaVinculacion.ToUniversalTime(),
        NegocioId = negocioId
    };
}