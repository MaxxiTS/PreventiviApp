// Preventivi App — API .NET 9 (esqueleto)
// Punto de entrada de la Web API. La composición real (DI, EF Core, MediatR,
// autenticación JWT, SignalR, Serilog) se implementará en la fase de desarrollo
// siguiendo docs/02-arquitectura.md y docs/09-api-rest.md.

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TODO: builder.Services.AddApplication();      // MediatR, validadores, mappers
// TODO: builder.Services.AddInfrastructure();   // DbContext (SQLite/PostgreSQL), repos, sync
// TODO: Autenticación JWT + autorización RBAC (docs/19-seguridad.md)

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
