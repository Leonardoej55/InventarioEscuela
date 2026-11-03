using InventarioEscuela.ViewModels;
using QuestPDF.Fluent;

namespace InventarioEscuela.Services.Reportes.Pdf;

internal sealed class PlantillaRecursosPorUbicacion(
    ReporteRecursosPorUbicacionViewModel reporte, CancellationToken ct) : IPlantillaReportePdf
{
    public string Titulo => "Inventario por ubicación";
    public string NombreArchivo => "RecursosPorUbicacion";
    public ReporteBaseViewModel Reporte => reporte;

    public void Componer(ColumnDescriptor columna)
    {
        ComponentesReportePdf.Resumen(columna,
            ("Activos", reporte.TotalResultados), ("Ubicaciones", reporte.TotalUbicaciones),
            ("Marcas", reporte.TotalMarcas), ("Estados", reporte.TotalEstados));

        if (reporte.Activos.Count == 0)
        {
            ComponentesReportePdf.SinResultados(columna);
            return;
        }

        if (reporte.ResumenUbicaciones.Count > 0)
        {
            columna.Item().Text("Resumen por ubicación").FontSize(11).Bold();
            ComponentesReportePdf.Tabla(columna,
                ["Ubicación", "Total", "Funcionales", "Dañados", "En mantenimiento", "Otros estados"],
                [3, 1, 1, 1, 1.4f, 1.2f], tabla =>
                {
                    int indice = 0;
                    foreach (var ubicacion in reporte.ResumenUbicaciones)
                    {
                        ct.ThrowIfCancellationRequested();
                        ComponentesReportePdf.Fila(tabla, indice++,
                            ComponentesReportePdf.Valor(ubicacion.Ubicacion, "Sin ubicación"),
                            ubicacion.Total.ToString(), ubicacion.Funcionales.ToString(), ubicacion.Danados.ToString(),
                            ubicacion.EnMantenimiento.ToString(), ubicacion.Otros.ToString());
                    }
                });
        }

        foreach (var grupo in reporte.Activos.GroupBy(a => a.IdUbicacion)
                     .OrderBy(g => g.First().Ubicacion?.UbicacionNombre))
        {
            ct.ThrowIfCancellationRequested();
            string ubicacion = ComponentesReportePdf.Valor(grupo.First().Ubicacion?.UbicacionNombre, "Sin ubicación");
            ComponentesReportePdf.Tabla(columna,
                ["Identificación", "Descripción", "Marca / modelo", "Serie", "Estado actual", "Adquisición"],
                [1.2f, 2.6f, 1.8f, 1.5f, 1.4f, 1.5f], tabla =>
                {
                    int indice = 0;
                    foreach (var activo in grupo)
                    {
                        ct.ThrowIfCancellationRequested();
                        ComponentesReportePdf.Fila(tabla, indice++, activo.NumeroIdentificacion, activo.Descripcion,
                            ComponentesReportePdf.MarcaYModelo(activo), activo.Serie,
                            activo.EstadoActivo?.Estado, activo.ModoAdquisicion?.NombreModoAdquisicion);
                    }
                }, $"{ubicacion} · {grupo.Count()} activo(s)");
        }
    }
}
