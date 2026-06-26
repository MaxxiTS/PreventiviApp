using PreventiviApp.Domain.Common;
using PreventiviApp.Domain.Enums;

namespace PreventiviApp.Domain.Entities;

/// <summary>
/// Recurso básico del análisis de precios: mano de obra, material, maquinaria u
/// otros (discriminador <see cref="TipoRecurso"/>).
/// </summary>
public sealed class Recurso : AuditableEntity
{
    public Recurso(
        Guid id,
        Guid preciosarioId,
        TipoRecurso tipo,
        string codigo,
        string descripcion,
        decimal precio,
        Guid? unidadId = null) : base(id)
    {
        PreciosarioId = preciosarioId;
        Tipo = tipo;
        Codigo = codigo;
        Descripcion = descripcion;
        Precio = precio;
        UnidadId = unidadId;
    }

    public Guid PreciosarioId { get; private set; }
    public TipoRecurso Tipo { get; set; }
    public string Codigo { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public Guid? UnidadId { get; set; }
}
