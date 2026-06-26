using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PreventiviApp.Application.Common.Behaviors;
using PreventiviApp.Domain.Services;

namespace PreventiviApp.Application;

/// <summary>Registro de la capa de aplicación (CQRS + validación + servicios de dominio).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Servicios de dominio sin estado: el motor de cálculo.
        services.AddSingleton<IEvaluadorFormula, EvaluadorFormula>();
        services.AddSingleton<EvaluadorMedicion>();
        services.AddSingleton<CalculadoraPresupuesto>();
        services.AddSingleton<ActualizadorPreciosPresupuesto>();
        services.AddSingleton<ConciliadorPreciosario>();
        services.AddSingleton<ComparadorPresupuestos>();

        return services;
    }
}
