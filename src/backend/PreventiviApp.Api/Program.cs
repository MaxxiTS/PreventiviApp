using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using PreventiviApp.Api;
using PreventiviApp.Application;
using PreventiviApp.Infrastructure;
using PreventiviApp.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Conveniencia en desarrollo: crea la base SQLite local si no existe.
    // En producción se usan migraciones EF Core (dotnet ef database update).
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
