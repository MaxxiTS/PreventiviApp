namespace PreventiviApp.Domain.Common;

/// <summary>
/// Error de dominio con código estable y mensaje legible. Forma parte del
/// <see cref="Result"/> / <see cref="Result{T}"/> (Result Pattern, ver ADR 0001).
/// </summary>
public sealed record Error(string Codigo, string Mensaje)
{
    public static readonly Error Ninguno = new(string.Empty, string.Empty);

    /// <summary>Error de validación de invariante de dominio.</summary>
    public static Error Validacion(string mensaje) => new("dominio.validacion", mensaje);

    /// <summary>Entidad no encontrada.</summary>
    public static Error NoEncontrado(string mensaje) => new("dominio.no_encontrado", mensaje);

    /// <summary>Conflicto / operación no permitida en el estado actual.</summary>
    public static Error Conflicto(string mensaje) => new("dominio.conflicto", mensaje);
}
