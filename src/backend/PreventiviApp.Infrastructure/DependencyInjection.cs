using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PreventiviApp.Application.Abstractions.Documentos;
using PreventiviApp.Application.Abstractions.Importacion;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Infrastructure.Documentos;
using PreventiviApp.Infrastructure.Importacion.Dcf;
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
        services.AddScoped<IPreciosarioRepository, PreciosarioRepository>();
        services.AddScoped<IPresupuestoRepository, PresupuestoRepository>();

        // Importación DCF y generación de documentos
        services.AddScoped<IImportadorPreciosario, DcfImporter>();
        services.AddScoped<IGeneradorPresupuestoPdf, GeneradorPresupuestoPdf>();

        return services;
    }
}
