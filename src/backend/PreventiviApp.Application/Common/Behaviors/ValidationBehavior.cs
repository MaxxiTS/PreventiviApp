using FluentValidation;
using MediatR;

namespace PreventiviApp.Application.Common.Behaviors;

/// <summary>
/// Comportamiento del pipeline de MediatR que ejecuta los validadores de
/// FluentValidation antes de invocar el handler. Si hay errores, lanza
/// <see cref="ValidationException"/> (la capa API la traduce a 400/ProblemDetails).
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var contexto = new ValidationContext<TRequest>(request);
            var resultados = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(contexto, cancellationToken)));

            var fallos = resultados
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (fallos.Count != 0)
                throw new ValidationException(fallos);
        }

        return await next();
    }
}
