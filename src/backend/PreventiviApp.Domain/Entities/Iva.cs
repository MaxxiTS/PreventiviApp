using PreventiviApp.Domain.Common;

namespace PreventiviApp.Domain.Entities;

/// <summary>Tipo de IVA aplicable (p. ej. 21 % general, 10 % reducido).</summary>
public sealed class Iva : AuditableEntity
{
    public Iva(Guid id, string nombre, decimal porcentaje) : base(id)
    {
        Nombre = nombre;
        Porcentaje = porcentaje;
    }

    public string Nombre { get; set; }

    /// <summary>Porcentaje de IVA (p. ej. 21 para 21 %).</summary>
    public decimal Porcentaje { get; set; }

    public static Iva General() => new(Guid.NewGuid(), "IVA general", 21m);
}
