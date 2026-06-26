using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Application.Abstractions.Persistence;

public interface IClienteRepository
{
    Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken = default);
    Task AnadirAsync(Cliente cliente, CancellationToken cancellationToken = default);
}
