using PreventiviApp.Domain.Common;

namespace PreventiviApp.Domain.Entities;

/// <summary>
/// Precio de una partida del preciosario. Puede marcarse como <see cref="Bloqueado"/>
/// para que no se sobrescriba al reimportar un DCF (doc 12 §5.3).
/// </summary>
public sealed class Precio : AuditableEntity
{
    public Precio(Guid id, Guid partidaId, decimal valor, string moneda = "EUR") : base(id)
    {
        PartidaId = partidaId;
        Valor = valor;
        Moneda = moneda;
        VigenteDesde = DateTimeOffset.UtcNow;
    }

    public Guid PartidaId { get; private set; }
    public decimal Valor { get; set; }
    public string Moneda { get; set; }
    public bool Bloqueado { get; set; }
    public DateTimeOffset VigenteDesde { get; set; }
}
