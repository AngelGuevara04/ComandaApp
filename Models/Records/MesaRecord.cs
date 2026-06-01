using Postgrest.Attributes;
using Postgrest.Models;
using ComandaApp.Models;

namespace ComandaApp.Models.Records;

[Table("mesas")]
public class MesaRecord : BaseModel
{
    [PrimaryKey("id", true)]
    public string Id { get; set; } = string.Empty;

    [Column("numero_mesa")]
    public string NumeroMesa { get; set; } = string.Empty;

    [Column("capacidad")]
    public int Capacidad { get; set; } = 4;

    [Column("area")]
    public string Area { get; set; } = "General";

    [Column("qr_code_data")]
    public string QrCodeData { get; set; } = string.Empty;

    [Column("esta_ocupada")]
    public bool EstaOcupada { get; set; }

    [Column("negocio_id")]
    public string NegocioId { get; set; } = string.Empty;
}

public static class MesaMapping
{
    public static Mesa ToModel(this MesaRecord r) => new()
    {
        NumeroMesa = r.NumeroMesa,
        Capacidad = r.Capacidad,
        Area = r.Area,
        QrCodeData = r.QrCodeData,
        EstaOcupada = r.EstaOcupada
    };

    public static MesaRecord ToRecord(this Mesa m, string negocioId) => new()
    {
        NumeroMesa = m.NumeroMesa,
        Capacidad = m.Capacidad,
        Area = m.Area,
        QrCodeData = m.QrCodeData,
        EstaOcupada = m.EstaOcupada,
        NegocioId = negocioId
    };
}