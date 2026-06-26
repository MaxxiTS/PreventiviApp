namespace PreventiviApp.Domain.Services;

/// <summary>Acción a aplicar a una partida al reimportar un preciosario (doc 12 §6).</summary>
public enum AccionConciliacion
{
    /// <summary>Código nuevo: la partida no existe en el catálogo actual.</summary>
    Alta,

    /// <summary>Precio no bloqueado y distinto: se actualiza.</summary>
    Actualizar,

    /// <summary>Precio bloqueado: se conserva (nunca se sobrescribe).</summary>
    Respetar,

    /// <summary>El precio coincide: sin cambios.</summary>
    SinCambio,

    /// <summary>La partida del catálogo no aparece en el DCF nuevo: se marca obsoleta.</summary>
    Obsoleta,
}

/// <summary>Estado de una partida ya presente en el catálogo (preciosario).</summary>
public readonly record struct EstadoPartidaCatalogo(string Codigo, decimal Precio, bool Bloqueado);

/// <summary>Partida procedente del DCF que se reimporta.</summary>
public readonly record struct PartidaEntranteDcf(string Codigo, decimal Precio);

/// <summary>Decisión de conciliación para un código de partida.</summary>
public sealed record DecisionConciliacion(string Codigo, AccionConciliacion Accion, decimal PrecioNuevo);

/// <summary>
/// Compara el catálogo actual de un preciosario con las partidas de un DCF nuevo y
/// produce el plan de conciliación (doc 12 §6), con política conservadora: respeta
/// los precios bloqueados y no borra; las ausentes se marcan obsoletas.
/// </summary>
public sealed class ConciliadorPreciosario
{
    public IReadOnlyList<DecisionConciliacion> Conciliar(
        IEnumerable<EstadoPartidaCatalogo> existentes,
        IEnumerable<PartidaEntranteDcf> entrantes)
    {
        var existentesPorCodigo = new Dictionary<string, EstadoPartidaCatalogo>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in existentes)
            existentesPorCodigo[e.Codigo] = e;

        var entrantesPorCodigo = new Dictionary<string, PartidaEntranteDcf>(StringComparer.OrdinalIgnoreCase);
        foreach (var x in entrantes)
            entrantesPorCodigo[x.Codigo] = x;

        var decisiones = new List<DecisionConciliacion>();

        foreach (var entrante in entrantesPorCodigo.Values)
        {
            if (!existentesPorCodigo.TryGetValue(entrante.Codigo, out var existente))
                decisiones.Add(new DecisionConciliacion(entrante.Codigo, AccionConciliacion.Alta, entrante.Precio));
            else if (existente.Bloqueado)
                decisiones.Add(new DecisionConciliacion(entrante.Codigo, AccionConciliacion.Respetar, existente.Precio));
            else if (existente.Precio != entrante.Precio)
                decisiones.Add(new DecisionConciliacion(entrante.Codigo, AccionConciliacion.Actualizar, entrante.Precio));
            else
                decisiones.Add(new DecisionConciliacion(entrante.Codigo, AccionConciliacion.SinCambio, existente.Precio));
        }

        foreach (var existente in existentesPorCodigo.Values)
            if (!entrantesPorCodigo.ContainsKey(existente.Codigo))
                decisiones.Add(new DecisionConciliacion(existente.Codigo, AccionConciliacion.Obsoleta, existente.Precio));

        return decisiones;
    }
}
