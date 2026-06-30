using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PreventiviApp.Api;
using PreventiviApp.Application;
using PreventiviApp.Infrastructure;
using PreventiviApp.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

const string politicaCorsDev = "dev";

// Capas (Clean Architecture)
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Manejo de errores de validación → 400 ProblemDetails
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();

// CORS para el frontend Flutter (web/desktop) en desarrollo
builder.Services.AddCors(options =>
    options.AddPolicy(politicaCorsDev, policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(politicaCorsDev);

    // Crea la base SQLite local y siembra datos de ejemplo (idempotente).
    // En producción se usan migraciones EF Core (dotnet ef database update).
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DevDataSeeder.SeedAsync(scope.ServiceProvider);
}
else
{
    app.UseHttpsRedirection();
}

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
