using System.Globalization;
using InventarioEscuela.Models;
using InventarioEscuela.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace InventarioEscuela.Services.Reportes.Pdf;

internal interface IPlantillaReportePdf
{
    string Titulo { get; }
    string NombreArchivo { get; }
    ReporteBaseViewModel Reporte { get; }
    void Componer(ColumnDescriptor columna);
}

internal static class ComponentesReportePdf
{
    internal const string Azul = "#17436A";

    internal static void ConfigurarPagina(PageDescriptor pagina)
    {
        pagina.Size(PageSizes.A4.Landscape());
        pagina.Margin(25);
        pagina.DefaultTextStyle(estilo => estilo.FontSize(9).FontColor(Colors.Grey.Darken4));
    }

    internal static void Encabezado(IContainer contenedor, string titulo, ContextoReportePdf contexto)
    {
        contenedor.Column(columna =>
        {
            columna.Spacing(3);
            columna.Item().Text("ESCUELA PROYECTO PACUARE").FontSize(16).Bold().FontColor(Azul);
            columna.Item().Text("Sistema de Inventario Institucional").FontSize(10);
            columna.Item().PaddingTop(5).Text(titulo).FontSize(14).Bold();
            columna.Item().Text($"Generado: {contexto.FechaGeneracion:dd/MM/yyyy HH:mm zzz}  |  Por: {Valor(contexto.GeneradoPor)}");
            columna.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    internal static void FiltrosYAlcance(IContainer contenedor, ReporteBaseViewModel reporte)
    {
        contenedor.Background(Colors.Grey.Lighten4).Padding(8).Column(columna =>
        {
            columna.Spacing(3);
            columna.Item().Text(texto =>
            {
                texto.Span("Alcance: ").Bold();
                texto.Span(string.IsNullOrWhiteSpace(reporte.Alcance)
                    ? "Resultados completos según los filtros seleccionados."
                    : reporte.Alcance);
            });
            columna.Item().Text(texto =>
            {
                texto.Span("Filtros aplicados: ").Bold();
                texto.Span(reporte.FiltrosAplicados.Count == 0
                    ? "Sin filtros adicionales."
                    : string.Join(" · ", reporte.FiltrosAplicados));
            });
        });
    }

    internal static void Resumen(ColumnDescriptor columna, params (string Etiqueta, int Total)[] valores)
    {
        columna.Item().Row(fila =>
        {
            fila.Spacing(5);
            foreach (var valor in valores)
            {
                fila.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(7)
                    .Column(celda =>
                    {
                        celda.Item().Text(valor.Total.ToString(CultureInfo.InvariantCulture)).FontSize(15).Bold().FontColor(Azul);
                        celda.Item().Text(valor.Etiqueta);
                    });
            }
        });
    }

    internal static void Tabla(ColumnDescriptor columna, string[] encabezados, float[] anchos,
        Action<TableDescriptor> filas, string? ubicacion = null)
    {
        columna.Item().Table(tabla =>
        {
            tabla.ColumnsDefinition(columnas =>
            {
                foreach (var ancho in anchos)
                    columnas.RelativeColumn(ancho);
            });
            tabla.Header(cabecera =>
            {
                if (ubicacion is not null)
                    cabecera.Cell().ColumnSpan((uint)encabezados.Length).PaddingVertical(6)
                        .Text(ubicacion).FontSize(11).Bold().FontColor(Azul);
                foreach (var encabezado in encabezados)
                    cabecera.Cell().Background(Azul).Padding(5).AlignMiddle()
                        .Text(encabezado).FontColor(Colors.White).Bold();
            });
            filas(tabla);
        });
    }

    internal static IContainer Celda(TableDescriptor tabla, bool alternar = false)
        => tabla.Cell().Background(alternar ? Colors.Grey.Lighten4 : Colors.White)
            .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(5);

    internal static void Fila(TableDescriptor tabla, int indice, params string?[] valores)
    {
        foreach (var valor in valores)
            Celda(tabla, indice % 2 == 1).Text(Valor(valor));
    }

    internal static void SinResultados(ColumnDescriptor columna)
        => columna.Item().PaddingVertical(12).Text("No hay resultados para los filtros seleccionados.");

    internal static string Valor(string? valor, string alternativo = "No informado")
        => string.IsNullOrWhiteSpace(valor) ? alternativo : valor.Trim();

    internal static string Fecha(DateTime fecha)
        => fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    internal static string FechaHora(DateTime fecha)
        => fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

    internal static string MarcaYModelo(Activo? activo)
        => $"{Valor(activo?.Marca?.MarcaNombre)}\nModelo: {Valor(activo?.Modelo)}";

    internal static string Identidad(Activo? activo)
        => $"{Valor(activo?.Descripcion)}\nMarca: {Valor(activo?.Marca?.MarcaNombre)} · Modelo: {Valor(activo?.Modelo)}\nSerie: {Valor(activo?.Serie)}";

    internal static void PiePagina(IContainer contenedor)
    {
        contenedor.AlignCenter().Text(texto =>
        {
            texto.DefaultTextStyle(estilo => estilo.FontSize(8).FontColor(Colors.Grey.Darken1));
            texto.Span("Inventario Institucional · Página ");
            texto.CurrentPageNumber();
            texto.Span(" de ");
            texto.TotalPages();
        });
    }
}
