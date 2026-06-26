namespace PreventiviApp.Domain.Common;

/// <summary>
/// Result Pattern: representa éxito o fallo sin lanzar excepciones para flujos de
/// negocio esperables (ver ADR 0001 y docs/02-arquitectura.md).
/// </summary>
public class Result
{
    protected Result(bool exito, Error error)
    {
        if (exito && error != Error.Ninguno)
            throw new InvalidOperationException("Un resultado exitoso no puede tener error.");
        if (!exito && error == Error.Ninguno)
            throw new InvalidOperationException("Un resultado fallido requiere un error.");

        Exito = exito;
        Error = error;
    }

    public bool Exito { get; }
    public bool EsFallo => !Exito;
    public Error Error { get; }

    public static Result Ok() => new(true, Error.Ninguno);
    public static Result Fallo(Error error) => new(false, error);

    public static Result<T> Ok<T>(T valor) => Result<T>.Correcto(valor);
    public static Result<T> Fallo<T>(Error error) => Result<T>.Incorrecto(error);
}

/// <summary>Result con valor asociado en caso de éxito.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _valor;

    private Result(T? valor, bool exito, Error error) : base(exito, error) => _valor = valor;

    /// <summary>Valor del resultado. Lanza si se accede en un resultado fallido.</summary>
    public T Valor => Exito
        ? _valor!
        : throw new InvalidOperationException("No se puede acceder al valor de un resultado fallido.");

    internal static Result<T> Correcto(T valor) => new(valor, true, Error.Ninguno);
    internal static Result<T> Incorrecto(Error error) => new(default, false, error);

    public static implicit operator Result<T>(T valor) => Correcto(valor);
    public static implicit operator Result<T>(Error error) => Incorrecto(error);
}
