using Microsoft.EntityFrameworkCore;
using PreventiviApp.Application.Abstractions.Importacion;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Entities;
using PreventiviApp.Domain.Services;

namespace PreventiviApp.Infrastructure.Persistence.Repositories;

internal sealed class PreciosarioRepository(AppDbContext db) : IPreciosarioRepository
{
    public async Task AnadirPreciosarioAsync(PreciosarioImportado grafo, CancellationToken ct)
    {
        await db.Preciosarios.AddAsync(grafo.Preciosario, ct);
        await db.Unidades.AddRangeAsync(grafo.Unidades, ct);
        await db.Capitulos.AddRangeAsync(grafo.Capitulos, ct);
        await db.Recursos.AddRangeAsync(grafo.Recursos, ct);
        await db.Partidas.AddRangeAsync(grafo.Partidas, ct);
        await db.Descompuestos.AddRangeAsync(grafo.Descompuestos, ct);
        await db.Precios.AddRangeAsync(grafo.Precios, ct);
    }

    public async Task<IReadOnlyList<Preciosario>> ListarPreciosariosAsync(CancellationToken ct = default)
        => await db.Preciosarios.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync(ct);

    public async Task<Preciosario?> ObtenerPreciosarioAsync(Guid id, CancellationToken ct = default)
        => await db.Preciosarios.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Capitulo>> ListarCapitulosAsync(Guid preciosarioId, Guid? padreId, CancellationToken ct = default)
    {
        var consulta = db.Capitulos.AsNoTracking().Where(c => c.PreciosarioId == preciosarioId);
        consulta = padreId is null
            ? consulta.Where(c => c.PadreId == null)
            : consulta.Where(c => c.PadreId == padreId.Value);
        return await consulta.OrderBy(c => c.Orden).ThenBy(c => c.Codigo).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Partida>> ListarPartidasAsync(Guid capituloId, CancellationToken ct = default)
        => await db.Partidas.AsNoTracking().Where(p => p.CapituloId == capituloId)
            .OrderBy(p => p.Codigo).ToListAsync(ct);

    public async Task<Partida?> ObtenerPartidaAsync(Guid partidaId, CancellationToken ct = default)
        => await db.Partidas.AsNoTracking().FirstOrDefaultAsync(p => p.Id == partidaId, ct);

    public async Task<IReadOnlyList<Descompuesto>> ListarDescompuestoAsync(Guid partidaId, CancellationToken ct = default)
        => await db.Descompuestos.AsNoTracking().Where(d => d.PartidaId == partidaId).ToListAsync(ct);

    public async Task<IReadOnlyDictionary<string, decimal>> ObtenerPreciosPorCodigoAsync(Guid preciosarioId, CancellationToken ct = default)
    {
        var capituloIds = await db.Capitulos
            .Where(c => c.PreciosarioId == preciosarioId)
            .Select(c => c.Id)
            .ToListAsync(ct);

        var partidas = await db.Partidas.AsNoTracking()
            .Where(p => capituloIds.Contains(p.CapituloId))
            .Select(p => new { p.Codigo, p.Precio })
            .ToListAsync(ct);

        var precios = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in partidas)
            precios[p.Codigo] = p.Precio;

        return precios;
    }

    public async Task<IReadOnlyList<EstadoPartidaCatalogo>> ObtenerEstadoPartidasCatalogoAsync(Guid preciosarioId, CancellationToken ct = default)
    {
        var capituloIds = await db.Capitulos
            .Where(c => c.PreciosarioId == preciosarioId)
            .Select(c => c.Id)
            .ToListAsync(ct);

        var partidas = await db.Partidas.AsNoTracking()
            .Where(p => capituloIds.Contains(p.CapituloId))
            .Select(p => new { p.Id, p.Codigo, p.Precio })
            .ToListAsync(ct);

        var partidaIds = partidas.Select(p => p.Id).ToList();

        var bloqueadas = (await db.Precios.AsNoTracking()
            .Where(pr => partidaIds.Contains(pr.PartidaId) && pr.Bloqueado)
            .Select(pr => pr.PartidaId)
            .ToListAsync(ct)).ToHashSet();

        return partidas
            .Select(p => new EstadoPartidaCatalogo(p.Codigo, p.Precio, bloqueadas.Contains(p.Id)))
            .ToList();
    }

    public async Task<IReadOnlyList<Partida>> ObtenerPartidasTrackedAsync(Guid preciosarioId, CancellationToken ct = default)
    {
        var capituloIds = await db.Capitulos
            .Where(c => c.PreciosarioId == preciosarioId)
            .Select(c => c.Id)
            .ToListAsync(ct);

        return await db.Partidas
            .Where(p => capituloIds.Contains(p.CapituloId))
            .ToListAsync(ct);
    }
}

