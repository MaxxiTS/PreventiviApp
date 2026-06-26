namespace PreventiviApp.Domain.Services;

/// <summary>
/// Resultado del cierre económico de un presupuesto (doc 12 §4.5).
/// </summary>
public readonly record struct ResultadoPresupuesto(
    decimal Pem,
    decimal PemAjustado,
    decimal BaseImponible,
    decimal CuotaIva,
    decimal Total);
