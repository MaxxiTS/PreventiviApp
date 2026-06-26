using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Application.Abstractions.Persistence;

public interface IProyectoRepository
{
    Task<Proyecto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Proyecto>> ListarAsync(CancellationToken cancellationToken = default);
    Task AnadirAsync(Proyecto proyecto, CancellationToken cancellationToken = default);
}
