using InventarioEscuela.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace InventarioEscuela.Services.Reportes.Pdf;

internal sealed class PlantillaVerificacionAnual(
    ReporteRecursosPorUbicacionViewModel reporte, CancellationToken ct) : IPlantillaReportePdf
{
    public string Titulo => "Hoja de verificación física anual";
    public string NombreArchivo => "VerificacionFisicaAnual";
    public ReporteBaseViewModel Reporte => reporte;

    public void Componer(ColumnDescriptor columna)
    {
        ComponentesReportePdf.Resumen(columna,
            ("Activos por verificar", reporte.TotalResultados), ("Ubicaciones", reporte.TotalUbicaciones));
        columna.Item().Text("Recorra cada ubicación, identifique cada activo y marque si lo encontró. Anote el estado observado y cualquier diferencia de serie, ubicación o condición.");
        columna.Item().Text("Esta hoja representa el inventario actual al generarla, no un inventario histórico al cierre de un año. Los activos antiguos también se incluyen según los filtros. Las anotaciones en papel no actualizan el sistema: registre después las correcciones y los movimientos en la aplicación.");

        if (reporte.Activos.Count == 0)
        {
            ComponentesReportePdf.SinResultados(columna);
            return;
        }

        bool primeraUbicacion = true;
        foreach (var grupo in reporte.Activos.GroupBy(a => a.IdUbicacion)
                     .OrderBy(g => g.First().Ubicacion?.UbicacionNombre))
        {
            ct.ThrowIfCancellationRequested();
            if (!primeraUbicacion)
                columna.Item().PageBreak();
            primeraUbicacion = false;
            string ubicacion = ComponentesReportePdf.Valor(grupo.First().Ubicacion?.UbicacionNombre, "Sin ubicación");
            columna.Item().PaddingTop(5).Text($"Ubicación: {ubicacion}  |  Responsable: __________________________  |  Fecha: ____ / ____ / ______");
            ComponentesReportePdf.Tabla(columna,
                ["Identificación", "Descripción / marca / modelo / serie", "Estado actual", "Encontrado", "Estado observado", "Observaciones / diferencias"],
                [1.2f, 3.1f, 1.4f, 1.2f, 1.7f, 2.4f], tabla =>
                {
                    int indice = 0;
                    foreach (var activo in grupo.OrderBy(a => a.NumeroIdentificacion))
                    {
                        ct.ThrowIfCancellationRequested();
                        bool alternar = indice++ % 2 == 1;
                        ComponentesReportePdf.Celda(tabla, alternar).MinHeight(45).Text(ComponentesReportePdf.Valor(activo.NumeroIdentificacion));
                        ComponentesReportePdf.Celda(tabla, alternar).Text(ComponentesReportePdf.Identidad(activo));
                        ComponentesReportePdf.Celda(tabla, alternar).Text(ComponentesReportePdf.Valor(activo.EstadoActivo?.Estado));
                        ComponentesReportePdf.Celda(tabla, alternar).Text("[ ] Sí\n\n[ ] No");
                        ComponentesReportePdf.Celda(tabla, alternar).MinHeight(45).Text(" ");
                        ComponentesReportePdf.Celda(tabla, alternar).MinHeight(45).Text(" ");
                    }
                }, $"{ubicacion} · {grupo.Count()} activo(s) por verificar");

            columna.Item().PaddingVertical(6).Text("Conteo físico: ______ activos encontrados  |  Diferencias pendientes: ______  |  Firma: __________________________");
        }

        columna.Item().Background(Colors.Grey.Lighten4).Padding(8)
            .Text("Si encuentra activos que no aparecen en esta hoja, anote su identificación, descripción y ubicación en una hoja adicional y regístrelos o corrija sus datos en la aplicación.");
    }
}
