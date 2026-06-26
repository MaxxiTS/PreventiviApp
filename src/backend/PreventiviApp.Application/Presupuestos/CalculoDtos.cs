namespace PreventiviApp.Application.Presupuestos;

// --- Entrada ---

public sealed record LineaMedicionDto(
    string? Comentario = null,
    decimal? Uds = null,
    decimal? Largo = null,
    decimal? Ancho = null,
    decimal? Alto = null,
    decimal? Coeficiente = null,
    string? Formula = null,
    bool EsComentario = false);

public sealed record PartidaCalculoDto(
    string Codigo,
    string Resumen,
    decimal Precio,
    IReadOnlyList<LineaMedicionDto> Lineas);

public sealed record CapituloCalculoDto(
    string Codigo,
    string Titulo,
    IReadOnlyList<PartidaCalculoDto> Partidas,
    IReadOnlyList<CapituloCalculoDto> Subcapitulos);

// --- Salida ---

public sealed record CapituloCalculadoDto(string Codigo, string Titulo, decimal Subtotal);

public sealed record PresupuestoCalculadoDto(
    decimal Pem,
    decimal PemAjustado,
    decimal BaseImponible,
    decimal CuotaIva,
    decimal Total,
    IReadOnlyList<CapituloCalculadoDto> SubtotalesCapitulos);
