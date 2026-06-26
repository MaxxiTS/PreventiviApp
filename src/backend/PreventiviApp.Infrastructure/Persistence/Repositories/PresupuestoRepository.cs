using Microsoft.EntityFrameworkCore;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Infrastructure.Persistence.Repositories;

internal sealed class PresupuestoRepository(AppDbContext db) : IPresupuestoRepository
{
    public async Task AnadirAsync(Presupuesto presupuesto, CancellationToken cancellationToken = default)
        => await db.Presupuestos.AddAsync(presupuesto, cancellationToken);

    public async Task AnadirCapituloAsync(CapituloPresupuesto capitulo, CancellationToken cancellationToken = default)
        => await db.CapitulosPresupuesto.AddAsync(capitulo, cancellationToken);

    public async Task AnadirPartidaAsync(PartidaPresupuesto partida, CancellationToken cancellationToken = default)
        => await db.PartidasPresupuesto.AddAsync(partida, cancellationToken); // cascada a Medicion + Líneas

    public async Task<Presupuesto?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await db.Presupuestos.Include(p => p.Iva).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<CapituloPresupuesto?> ObtenerCapituloAsync(Guid capituloId, CancellationToken cancellationToken = default)
        => await db.CapitulosPresupuesto.FirstOrDefaultAsync(c => c.Id == capituloId, cancellationToken);

    public async Task<PartidaPresupuesto?> ObtenerPartidaTrackedAsync(Guid partidaId, CancellationToken cancellationToken = default)
        => await db.PartidasPresupuesto.FirstOrDefaultAsync(p => p.Id == partidaId, cancellationToken);

    public async Task<PartidaPresupuesto?> ObtenerPartidaConMedicionAsync(Guid partidaId, CancellationToken cancellationToken = default)
        => await db.PartidasPresupuesto.AsNoTracking()
            .Include(p => p.Medicion).ThenInclude(m => m.Lineas)
            .FirstOrDefaultAsync(p => p.Id == partidaId, cancellationToken);

    public async Task<IReadOnlyList<PartidaPresupuesto>> ListarPartidasTrackedAsync(Guid presupuestoId, CancellationToken cancellationToken = default)
    {
        var capituloIds = await db.CapitulosPresupuesto
            .Where(c => c.PresupuestoId == presupuestoId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        return await db.PartidasPresupuesto
            .Where(p => capituloIds.Contains(p.CapituloPresupuestoId))
            .ToListAsync(cancellationToken);
    }

    public async Task<Presupuesto?> ObtenerArbolAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var presupuesto = await db.Presupuestos.AsNoTracking()
            .Include(p => p.Iva)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (presupuesto is null)
            return null;

        var capitulos = await db.CapitulosPresupuesto.AsNoTracking()
            .Where(c => c.PresupuestoId == id)
            .OrderBy(c => c.Orden)
            .ToListAsync(cancellationToken);

        var capituloIds = capitulos.Select(c => c.Id).ToList();

        var partidas = await db.PartidasPresupuesto.AsNoTracking()
            .Include(p => p.Medicion).ThenInclude(m => m.Lineas)
            .Where(p => capituloIds.Contains(p.CapituloPresupuestoId))
            .ToListAsync(cancellationToken);

        // Ensamblar el árbol en memoria.
        var partidasPorCapitulo = partidas.ToLookup(p => p.CapituloPresupuestoId);
        var capitulosPorId = capitulos.ToDictionary(c => c.Id);

        foreach (var capitulo in capitulos)
            foreach (var partida in partidasPorCapitulo[capitulo.Id])
                capitulo.AnadirPartida(partida);

        foreach (var capitulo in capitulos)
        {
            if (capitulo.PadreId is Guid padreId && capitulosPorId.TryGetValue(padreId, out var padre))
                padre.AnadirSubcapitulo(capitulo);
            else
                presupuesto.AnadirCapituloRaiz(capitulo);
        }

        return presupuesto;
    }

    public async Task<IReadOnlyList<Presupuesto>> ListarPorProyectoAsync(Guid proyectoId, CancellationToken cancellationToken = default)
        => await db.Presupuestos.AsNoTracking()
            .Include(p => p.Iva)
            .Where(p => p.ProyectoId == proyectoId)
            .OrderByDescending(p => p.CreadoEn)
            .ToListAsync(cancellationToken);
}
