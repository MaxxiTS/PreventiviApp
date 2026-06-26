using PreventiviApp.Domain.Entities;

namespace PreventiviApp.Application.Preciosarios;

public sealed record PreciosarioDto(Guid Id, string Nombre, string Version, string? Fuente)
{
    public static PreciosarioDto De(Preciosario p) => new(p.Id, p.Nombre, p.VersionPreciosario, p.Fuente);
}

public sealed record CapituloDto(Guid Id, string Codigo, string Titulo, int Orden)
{
    public static CapituloDto De(Capitulo c) => new(c.Id, c.Codigo, c.Titulo, c.Orden);
}

public sealed record PartidaDto(Guid Id, string Codigo, string Resumen, decimal Precio)
{
    public static PartidaDto De(Partida p) => new(p.Id, p.Codigo, p.Resumen, p.Precio);
}

public sealed record DescompuestoDto(Guid Id, string Tipo, decimal Rendimiento, decimal PrecioUnitario, decimal Importe)
{
    public static DescompuestoDto De(Descompuesto d) =>
        new(d.Id, d.Tipo.ToString(), d.Rendimiento, d.PrecioUnitario, d.Importe);
}

public sealed record AnalisisPreciosDto(
    Guid PartidaId,
    string Codigo,
    string Resumen,
    decimal Precio,
    decimal CosteDirecto,
    IReadOnlyList<DescompuestoDto> Lineas);
