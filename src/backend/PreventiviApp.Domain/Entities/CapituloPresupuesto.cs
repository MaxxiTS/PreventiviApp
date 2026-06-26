using PreventiviApp.Domain.Common;

namespace PreventiviApp.Domain.Entities;

/// <summary>
/// Capítulo de un presupuesto. Árbol jerárquico (autorreferencia
/// <see cref="PadreId"/>) que contiene partidas y subcapítulos. Cachea su
/// subtotal con dirty tracking para el recálculo incremental (doc 12 §8).
/// </summary>
public sealed class CapituloPresupuesto : AuditableEntity
{
    private readonly List<PartidaPresupuesto> _partidas = [];
    private readonly List<CapituloPresupuesto> _subcapitulos = [];

    public CapituloPresupuesto(
        Guid id,
        Guid presupuestoId,
        string codigo,
        string titulo,
        int orden,
        Guid? padreId = null) : base(id)
    {
        PresupuestoId = presupuestoId;
        Codigo = codigo;
        Titulo = titulo;
        Orden = orden;
        PadreId = padreId;
    }

    public Guid PresupuestoId { get; private set; }
    public Guid? PadreId { get; private set; }
    public string Codigo { get; set; }
    public string Titulo { get; set; }
    public int Orden { get; set; }

    public IReadOnlyList<PartidaPresupuesto> Partidas => _partidas;
    public IReadOnlyList<CapituloPresupuesto> Subcapitulos => _subcapitulos;

    // --- Dirty tracking para recálculo incremental ---
    public bool Dirty { get; private set; } = true;
    public decimal? SubtotalCache { get; private set; }

    public void AnadirPartida(PartidaPresupuesto partida)
    {
        _partidas.Add(partida);
        MarcarSucio();
    }

    public void AnadirSubcapitulo(CapituloPresupuesto capitulo)
    {
        _subcapitulos.Add(capitulo);
        MarcarSucio();
    }

    /// <summary>Invalida la caché de subtotal (al editar el capítulo o sus hijos).</summary>
    public void MarcarSucio()
    {
        Dirty = true;
        SubtotalCache = null;
    }

    /// <summary>Fija el subtotal calculado y marca el nodo como limpio.</summary>
    public void MarcarLimpio(decimal subtotal)
    {
        SubtotalCache = subtotal;
        Dirty = false;
    }

    /// <summary>Copia profunda del capítulo (nuevos identificadores) con sus partidas y subcapítulos.</summary>
    public CapituloPresupuesto Duplicar(Guid? nuevoPadreId)
    {
        var copia = new CapituloPresupuesto(Guid.NewGuid(), PresupuestoId, Codigo, Titulo, Orden, nuevoPadreId);

        foreach (var partida in _partidas)
            copia.AnadirPartida(partida.Duplicar(copia.Id));

        foreach (var subcapitulo in _subcapitulos)
            copia.AnadirSubcapitulo(subcapitulo.Duplicar(copia.Id));

        return copia;
    }
}
