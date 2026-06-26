namespace PreventiviApp.Application.Presupuestos;

public sealed record PartidaDetalleDto(
    Guid Id,
    string Codigo,
    string Resumen,
    decimal Precio,
    decimal MedicionTotal,
    decimal Importe);

public sealed record CapituloDetalleDto(
    Guid Id,
    string Codigo,
    string Titulo,
    decimal Subtotal,
    IReadOnlyList<PartidaDetalleDto> Partidas,
    IReadOnlyList<CapituloDetalleDto> Subcapitulos);

public sealed record PresupuestoDetalleDto(
    Guid Id,
    string Nombre,
    int Version,
    decimal Pem,
    decimal PemAjustado,
    decimal BaseImponible,
    decimal CuotaIva,
    decimal Total,
    IReadOnlyList<CapituloDetalleDto> Capitulos);

public sealed record PresupuestoResumenDto(
    Guid Id,
    string Nombre,
    int Version,
    string Estado,
    decimal Total);
