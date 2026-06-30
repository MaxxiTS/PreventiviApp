using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PreventiviApp.Application.Abstractions.Importacion;
using PreventiviApp.Application.Abstractions.Persistence;
using PreventiviApp.Domain.Entities;
using PreventiviApp.Infrastructure.Persistence;

namespace PreventiviApp.Infrastructure;

/// <summary>
/// Siembra datos de ejemplo en desarrollo para poder ver la app funcionando al
/// instante: un cliente, un proyecto, un preciosario importado de un DCF demo y un
/// presupuesto con mediciones. Es idempotente (no hace nada si ya hay proyectos).
/// </summary>
public static class DevDataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        if (await db.Proyectos.AnyAsync())
            return;

        var clientes = services.GetRequiredService<IClienteRepository>();
        var proyectos = services.GetRequiredService<IProyectoRepository>();
        var preciosarios = services.GetRequiredService<IPreciosarioRepository>();
        var presupuestos = services.GetRequiredService<IPresupuestoRepository>();
        var importador = services.GetRequiredService<IImportadorPreciosario>();
        var unidadDeTrabajo = services.GetRequiredService<IUnitOfWork>();

        // Cliente + proyecto
        var cliente = new Cliente(Guid.NewGuid(), "Constructora Demo S.L.")
        {
            Nif = "B12345678",
            Email = "demo@constructora.example",
        };
        await clientes.AnadirAsync(cliente);

        var proyecto = new Proyecto(Guid.NewGuid(), "Obra de ejemplo", cliente.Id)
        {
            Direccion = "Calle Mayor 1, Madrid",
        };
        await proyectos.AnadirAsync(proyecto);

        // Preciosario desde un DCF de ejemplo embebido
        using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(DcfDemo)))
        {
            var importacion = await importador.ImportarAsync(ms, "demo.dcf");
            if (importacion.Exito)
                await preciosarios.AnadirPreciosarioAsync(importacion.Valor, CancellationToken.None);
        }

        // Presupuesto demo con un capítulo y dos partidas con medición
        var presupuesto = new Presupuesto(Guid.NewGuid(), proyecto.Id, "Presupuesto demo", Iva.General())
        {
            CostesIndirectosPct = 3m,
        };
        var capitulo = new CapituloPresupuesto(Guid.NewGuid(), presupuesto.Id, "01", "Movimiento de tierras", 1);

        var excavacion = new PartidaPresupuesto(Guid.NewGuid(), capitulo.Id, "01.001", "Excavación en zanja", 18.45m);
        var e1 = excavacion.Medicion.AnadirLinea();
        e1.Comentario = "Zanja muro norte";
        (e1.Uds, e1.Largo, e1.Ancho, e1.Alto) = (1m, 25.00m, 0.60m, 1.20m);
        var e2 = excavacion.Medicion.AnadirLinea();
        e2.Comentario = "Pozos de cimentación";
        (e2.Uds, e2.Largo, e2.Ancho, e2.Alto) = (4m, 1.00m, 1.00m, 1.50m);
        capitulo.AnadirPartida(excavacion);

        var hormigon = new PartidaPresupuesto(Guid.NewGuid(), capitulo.Id, "01.002", "Hormigón de limpieza", 92.30m);
        var h1 = hormigon.Medicion.AnadirLinea();
        (h1.Uds, h1.Largo, h1.Ancho, h1.Alto) = (1m, 18.00m, 0.80m, 0.10m);
        capitulo.AnadirPartida(hormigon);

        presupuesto.AnadirCapituloRaiz(capitulo);

        await presupuestos.AnadirAsync(presupuesto);
        await presupuestos.AnadirCapituloAsync(capitulo);

        await unidadDeTrabajo.GuardarCambiosAsync();
    }

    private static readonly string DcfDemo = string.Join('\n',
        "PRECIOSARIO;Preciosario demo;2026",
        "UNIDAD;m3;Metro cúbico",
        "UNIDAD;m2;Metro cuadrado",
        "UNIDAD;h;Hora",
        "CAPITULO;01;Movimiento de tierras",
        "CAPITULO;02;Cimentaciones",
        "RECURSO;MO;MO.001;Peón ordinario;h;17.20",
        "RECURSO;MQ;MQ.001;Retroexcavadora;h;38.50",
        "RECURSO;MT;MT.001;Hormigón HM-20;m3;78.50",
        "PARTIDA;01;01.001;Excavación en zanja;m3;18.45",
        "PARTIDA;01;01.002;Hormigón de limpieza;m2;92.30",
        "PARTIDA;02;02.001;Zapata de hormigón armado;m3;145.80",
        "DESCOMPUESTO;01.001;MO.001;0.150",
        "DESCOMPUESTO;01.001;MQ.001;0.250",
        "DESCOMPUESTO;01.002;MT.001;0.100");
}
