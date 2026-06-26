using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Infrastructure.Persistence;
using PreventiviApp.Infrastructure.Persistence.Repositories;

namespace PreventiviApp.Infrastructure;

/// <summary>Registro de la capa de infraestructura (persistencia EF Core + SQLite).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var cadenaConexion = configuration.GetConnectionString("Sqlite")
            ?? "Data Source=preventivi.db";

        services.AddDbContext<AppDbContext>(options => options.UseSqlite(cadenaConexion));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IProyectoRepository, ProyectoRepository>();

        return services;
    }
}
