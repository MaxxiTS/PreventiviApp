using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Application.Abstractions.Persistence;

public interface IPresupuestoRepository
{
    Task AnadirAsync(Presupuesto presupuesto, CancellationToken cancellationToken = default);
    Task AnadirCapituloAsync(CapituloPresupuesto capitulo, CancellationToken cancellationToken = default);
    Task AnadirPartidaAsync(PartidaPresupuesto partida, CancellationToken cancellationToken = default);

    /// <summary>Presupuesto sin el árbol (para validaciones/lecturas ligeras).</summary>
    Task<Presupuesto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Capítulo concreto del presupuesto (para validar el destino al añadir).</summary>
    Task<CapituloPresupuesto?> ObtenerCapituloAsync(Guid capituloId, CancellationToken cancellationToken = default);

    /// <summary>Reconstruye el árbol completo (capítulos, subcapítulos, partidas y mediciones).</summary>
    Task<Presupuesto?> ObtenerArbolAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Presupuesto>> ListarPorProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default);
}
