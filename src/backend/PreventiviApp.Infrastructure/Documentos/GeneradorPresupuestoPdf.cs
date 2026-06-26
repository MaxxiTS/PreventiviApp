using System.Globalization;
using PreventiviApp.Application.Abstractions.Documentos;
using PreventiviApp.Domain.Entities;
using PreventiviApp.Domain.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PreventiviApp.Infrastructure.Documentos;

/// <summary>
/// Generador de PDF de presupuestos con QuestPDF (Fluent API). Reutiliza el motor
/// de dominio (<see cref="CalculadoraPresupuesto"/> y <see cref="EvaluadorMedicion"/>)
/// para importes, subtotales y cierre económico. No persiste nada.
/// </summary>
public sealed class GeneradorPresupuestoPdf : IGeneradorPresupuestoPdf
{
    private static readonly CultureInfo CulturaEs = CultureInfo.GetCultureInfo("es-ES");

    static GeneradorPresupuestoPdf()
    {
        // Licencia Community de QuestPDF (uso gratuito permitido).
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private readonly CalculadoraPresupuesto _calculadora;
    private readonly EvaluadorMedicion _mediciones;

    public GeneradorPresupuestoPdf(CalculadoraPresupuesto calculadora, EvaluadorMedicion mediciones)
    {
        _calculadora = calculadora;
        _mediciones = mediciones;
    }

    public byte[] Generar(Presupuesto presupuesto)
    {
        ArgumentNullException.ThrowIfNull(presupuesto);

        return Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(2, Unit.Centimetre);
                pagina.DefaultTextStyle(estilo => estilo.FontSize(9).FontColor(Colors.Grey.Darken4));

                pagina.Header().Element(c => ComponerCabecera(c, presupuesto));
                pagina.Content().Element(c => ComponerCuerpo(c, presupuesto));
                pagina.Footer().Element(ComponerPie);
            });
        }).GeneratePdf();
    }

    private static void ComponerCabecera(IContainer contenedor, Presupuesto presupuesto)
    {
        contenedor.Column(columna =>
        {
            columna.Item()
                .Text("Presupuesto")
                .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);

            columna.Item().PaddingTop(2)
                .Text(presupuesto.Nombre)
                .FontSize(13).SemiBold();

            columna.Item()
                .Text($"Versión {presupuesto.NumeroVersion}")
                .FontSize(9).FontColor(Colors.Grey.Darken1);

            columna.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    private void ComponerCuerpo(IContainer contenedor, Presupuesto presupuesto)
    {
        contenedor.PaddingVertical(10).Column(columna =>
        {
            columna.Spacing(12);

            foreach (var capitulo in presupuesto.CapitulosRaiz)
                columna.Item().Element(c => ComponerCapitulo(c, capitulo, nivel: 0));

            columna.Item().PaddingTop(8).Element(c => ComponerResumenEconomico(c, presupuesto));
        });
    }

    /// <summary>Pinta un capítulo (cabecera, tabla de partidas y subtotal) y recursa en sus subcapítulos.</summary>
    private void ComponerCapitulo(IContainer contenedor, CapituloPresupuesto capitulo, int nivel)
    {
        contenedor.Column(columna =>
        {
            columna.Spacing(6);

            // Cabecera del capítulo (la sangría comunica el nivel jerárquico).
            columna.Item().PaddingLeft(nivel * 12).Background(Colors.Grey.Lighten3).Padding(4).Row(fila =>
            {
                fila.ConstantItem(70).Text(capitulo.Codigo).SemiBold();
                fila.RelativeItem().Text(capitulo.Titulo).SemiBold();
            });

            if (capitulo.Partidas.Count > 0)
                columna.Item().PaddingLeft(nivel * 12).Element(c => ComponerTablaPartidas(c, capitulo));

            // Subcapítulos (recursión).
            foreach (var subcapitulo in capitulo.Subcapitulos)
                columna.Item().Element(c => ComponerCapitulo(c, subcapitulo, nivel + 1));

            // Subtotal del capítulo (incluye partidas y subcapítulos).
            var subtotal = _calculadora.CalcularSubtotalCapitulo(capitulo);
            columna.Item().PaddingLeft(nivel * 12).PaddingTop(2).Row(fila =>
            {
                fila.RelativeItem().AlignRight()
                    .Text($"Subtotal {capitulo.Codigo}").SemiBold();
                fila.ConstantItem(100).AlignRight()
                    .Text(FormatearEuro(subtotal)).SemiBold();
            });
        });
    }

    private void ComponerTablaPartidas(IContainer contenedor, CapituloPresupuesto capitulo)
    {
        contenedor.Table(tabla =>
        {
            tabla.ColumnsDefinition(columnas =>
            {
                columnas.ConstantColumn(70);  // Código
                columnas.RelativeColumn();     // Resumen
                columnas.ConstantColumn(70);   // Medición
                columnas.ConstantColumn(75);   // Precio
                columnas.ConstantColumn(85);   // Importe
            });

            tabla.Header(encabezado =>
            {
                EncabezadoCelda(encabezado, "Código");
                EncabezadoCelda(encabezado, "Resumen");
                EncabezadoCelda(encabezado, "Medición", alinearDerecha: true);
                EncabezadoCelda(encabezado, "Precio", alinearDerecha: true);
                EncabezadoCelda(encabezado, "Importe", alinearDerecha: true);
            });

            foreach (var partida in capitulo.Partidas)
            {
                var medicion = _mediciones.CalcularTotal(partida.Medicion);
                var importe = _calculadora.CalcularImportePartida(partida);

                Celda(tabla).Text(partida.Codigo);
                Celda(tabla).Text(partida.Resumen);
                Celda(tabla).AlignRight().Text(FormatearNumero(medicion));
                Celda(tabla).AlignRight().Text(FormatearEuro(partida.Precio));
                Celda(tabla).AlignRight().Text(FormatearEuro(importe));
            }
        });

        static void EncabezadoCelda(TableCellDescriptor encabezado, string texto, bool alinearDerecha = false)
        {
            var celda = encabezado.Cell()
                .BorderBottom(1).BorderColor(Colors.Grey.Medium)
                .PaddingVertical(3).PaddingHorizontal(2);
            (alinearDerecha ? celda.AlignRight() : celda).Text(texto).SemiBold();
        }

        static IContainer Celda(TableDescriptor tabla)
            => tabla.Cell()
                .BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(3).PaddingHorizontal(2);
    }

    private void ComponerResumenEconomico(IContainer contenedor, Presupuesto presupuesto)
    {
        var resultado = _calculadora.CalcularPresupuesto(presupuesto);

        contenedor.Column(columna =>
        {
            columna.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
            columna.Item().PaddingTop(4).PaddingBottom(2)
                .Text("Resumen económico").FontSize(12).Bold();

            columna.Item().Table(tabla =>
            {
                tabla.ColumnsDefinition(columnas =>
                {
                    columnas.RelativeColumn();
                    columnas.ConstantColumn(120);
                });

                FilaResumen(tabla, "PEM (Presupuesto de Ejecución Material)", FormatearEuro(resultado.Pem));
                FilaResumen(tabla, $"Baja de adjudicación ({FormatearNumero(presupuesto.BajaPct)} %)",
                    FormatearEuro(resultado.PemAjustado - resultado.Pem));
                FilaResumen(tabla, "PEM ajustado", FormatearEuro(resultado.PemAjustado));
                FilaResumen(tabla, "Descuentos", FormatearEuro(-presupuesto.DescuentosImporte));
                FilaResumen(tabla, "Base imponible", FormatearEuro(resultado.BaseImponible), destacar: true);
                FilaResumen(tabla, $"{presupuesto.Iva.Nombre} ({FormatearNumero(presupuesto.Iva.Porcentaje)} %)",
                    FormatearEuro(resultado.CuotaIva));
                FilaResumen(tabla, "TOTAL", FormatearEuro(resultado.Total), total: true);
            });
        });

        static void FilaResumen(TableDescriptor tabla, string concepto, string importe, bool destacar = false, bool total = false)
        {
            var conceptoCelda = tabla.Cell().PaddingVertical(2);
            var importeCelda = tabla.Cell().PaddingVertical(2).AlignRight();

            if (total)
            {
                conceptoCelda.BorderTop(1).BorderColor(Colors.Grey.Darken1).PaddingTop(4)
                    .Text(concepto).FontSize(11).Bold();
                importeCelda.BorderTop(1).BorderColor(Colors.Grey.Darken1).PaddingTop(4)
                    .Text(importe).FontSize(11).Bold().FontColor(Colors.Blue.Darken2);
            }
            else if (destacar)
            {
                conceptoCelda.Text(concepto).SemiBold();
                importeCelda.Text(importe).SemiBold();
            }
            else
            {
                conceptoCelda.Text(concepto);
                importeCelda.Text(importe);
            }
        }
    }

    private static void ComponerPie(IContainer contenedor)
    {
        contenedor.AlignCenter().Text(texto =>
        {
            texto.DefaultTextStyle(estilo => estilo.FontSize(8).FontColor(Colors.Grey.Darken1));
            texto.Span("Página ");
            texto.CurrentPageNumber();
            texto.Span(" de ");
            texto.TotalPages();
        });
    }

    /// <summary>Formatea un importe monetario en formato es-ES con símbolo €.</summary>
    private static string FormatearEuro(decimal valor)
        => string.Create(CulturaEs, $"{valor:N2} €");

    /// <summary>Formatea un número decimal en formato es-ES con 2 decimales (sin símbolo).</summary>
    private static string FormatearNumero(decimal valor)
        => valor.ToString("N2", CulturaEs);
}
