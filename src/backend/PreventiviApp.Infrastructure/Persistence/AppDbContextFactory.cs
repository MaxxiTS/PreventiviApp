using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PreventiviApp.Infrastructure.Persistence;

/// <summary>
/// Fábrica en tiempo de diseño para que las herramientas de EF Core
/// (<c>dotnet ef migrations</c>) puedan crear el contexto sin arrancar la API.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=preventivi.db")
            .Options;

        return new AppDbContext(options);
    }
}
