using Microsoft.EntityFrameworkCore;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Infrastructure.Persistence.Repositories;

internal sealed class ClienteRepository(AppDbContext db) : IClienteRepository
{
    public async Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await db.Clientes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Cliente>> ListarAsync(CancellationToken cancellationToken = default)
        => await db.Clientes.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync(cancellationToken);

    public async Task AnadirAsync(Cliente cliente, CancellationToken cancellationToken = default)
        => await db.Clientes.AddAsync(cliente, cancellationToken);
}
