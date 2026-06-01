using Postgrest.Attributes;
using Postgrest.Models;
using ComandaApp.Models;

namespace ComandaApp.Models.Records;

[Table("ordenes")]
public class OrdenRecord : BaseModel
{
    [PrimaryKey("id", false)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Column("numero_mesa")]
    public string NumeroMesa { get; set; } = string.Empty;

    [Column("nombre_cliente")]
    public string NombreCliente { get; set; } = string.Empty;

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    [Column("esta_pagada")]
    public bool EstaPagada { get; set; }

    [Column("negocio_id")]
    public string NegocioId { get; set; } = string.Empty;
}

public static class OrdenMapping
{
    public static OrdenMesa ToModel(this OrdenRecord r) => new()
    {
        IdOrden = r.Id,
        NombreCliente = r.NombreCliente,
        FechaCreacion = r.FechaCreacion.ToLocalTime(),
        EstaPagada = r.EstaPagada,
        MesaAsignada = new Mesa { NumeroMesa = r.NumeroMesa }
    };

    public static OrdenRecord ToRecord(this OrdenMesa m, string negocioId) => new()
    {
        Id = m.IdOrden,
        NumeroMesa = m.MesaAsignada.NumeroMesa,
        NombreCliente = m.NombreCliente,
        FechaCreacion = m.FechaCreacion.ToUniversalTime(),
        EstaPagada = m.EstaPagada,
        NegocioId = negocioId
    };
}