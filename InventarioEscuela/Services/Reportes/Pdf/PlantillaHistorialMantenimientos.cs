using InventarioEscuela.ViewModels;
using QuestPDF.Fluent;

namespace InventarioEscuela.Services.Reportes.Pdf;

internal sealed class PlantillaHistorialMantenimientos(
    ReporteEquiposMantenimientoViewModel reporte, CancellationToken ct) : IPlantillaReportePdf
{
    public string Titulo => "Historial de mantenimientos";
    public string NombreArchivo => "HistorialMantenimientos";
    public ReporteBaseViewModel Reporte => reporte;

    public void Componer(ColumnDescriptor columna)
    {
        ComponentesReportePdf.Resumen(columna,
            ("Registros de mantenimiento", reporte.TotalResultados), ("Activos distintos", reporte.TotalActivos),
            ("Ubicaciones actuales", reporte.TotalUbicaciones), ("Tipos", reporte.TotalTiposMantenimiento),
            ("Responsables", reporte.TotalResponsables));
        columna.Item().Text("Un activo puede tener varios mantenimientos. La ubicación corresponde a su ubicación actual; este historial no indica que el activo siga en mantenimiento.");

        if (reporte.Mantenimientos.Count == 0)
        {
            ComponentesReportePdf.SinResultados(columna);
            return;
        }

        ComponentesReportePdf.Tabla(columna,
            ["Fecha", "Identificación", "Activo", "Ubicación actual", "Tipo", "Responsable", "Trabajo registrado"],
            [1.1f, 1.3f, 2.7f, 1.5f, 1.2f, 1.5f, 2.5f], tabla =>
            {
                int indice = 0;
                foreach (var mantenimiento in reporte.Mantenimientos)
                {
                    ct.ThrowIfCancellationRequested();
                    ComponentesReportePdf.Fila(tabla, indice++,
                        ComponentesReportePdf.Fecha(mantenimiento.FechaMantenimiento),
                        mantenimiento.Activo?.NumeroIdentificacion,
                        ComponentesReportePdf.Identidad(mantenimiento.Activo),
                        ComponentesReportePdf.Valor(mantenimiento.Activo?.Ubicacion?.UbicacionNombre, "Sin ubicación"),
                        mantenimiento.TipoMantenimiento?.Mantenimiento, mantenimiento.Responsable,
                        mantenimiento.Descripcion);
                }
            });
    }
}
