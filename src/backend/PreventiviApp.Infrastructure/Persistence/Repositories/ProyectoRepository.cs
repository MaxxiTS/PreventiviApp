using Microsoft.EntityFrameworkCore;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Infrastructure.Persistence.Repositories;

internal sealed class ProyectoRepository(AppDbContext db) : IProyectoRepository
{
    public async Task<Proyecto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await db.Proyectos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Proyecto>> ListarAsync(CancellationToken cancellationToken = default)
        => await db.Proyectos.AsNoTracking().OrderByDescending(p => p.CreadoEn).ToListAsync(cancellationToken);

    public async Task AnadirAsync(Proyecto proyecto, CancellationToken cancellationToken = default)
        => await db.Proyectos.AddAsync(proyecto, cancellationToken);
}
