using Microsoft.EntityFrameworkCore;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Infrastructure.Persistence;

/// <summary>
/// Contexto EF Core de Preventivi App. Implementa <see cref="IUnitOfWork"/>.
/// Por defecto usa SQLite (offline-first); el mismo modelo es compatible con
/// PostgreSQL en el servidor (ver docs/03-modelo-er-base-datos.md).
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public Task<int> GuardarCambiosAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);
}
