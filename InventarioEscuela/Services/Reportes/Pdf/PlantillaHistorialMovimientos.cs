using InventarioEscuela.ViewModels;
using QuestPDF.Fluent;

namespace InventarioEscuela.Services.Reportes.Pdf;

internal sealed class PlantillaHistorialMovimientos(
    ReporteHistorialMovimientosViewModel reporte, CancellationToken ct) : IPlantillaReportePdf
{
    public string Titulo => "Historial de movimientos";
    public string NombreArchivo => "HistorialMovimientos";
    public ReporteBaseViewModel Reporte => reporte;

    public void Componer(ColumnDescriptor columna)
    {
        ComponentesReportePdf.Resumen(columna,
            ("Movimientos", reporte.TotalMovimientos), ("Activos distintos", reporte.TotalActivosMovidos),
            ("Ubicaciones de origen", reporte.TotalUbicacionesOrigen),
            ("Ubicaciones de destino", reporte.TotalUbicacionesDestino));

        if (reporte.Movimientos.Count == 0)
        {
            ComponentesReportePdf.SinResultados(columna);
            return;
        }

        ComponentesReportePdf.Tabla(columna,
            ["Fecha y hora", "Identificación", "Descripción", "Origen", "Destino", "Responsable", "Observación"],
            [1.1f, 1.3f, 2.4f, 1.5f, 1.5f, 1.6f, 2.5f], tabla =>
            {
                int indice = 0;
                foreach (var movimiento in reporte.Movimientos)
                {
                    ct.ThrowIfCancellationRequested();
                    ComponentesReportePdf.Fila(tabla, indice++,
                        ComponentesReportePdf.FechaHora(movimiento.FechaMovimiento),
                        movimiento.Activo?.NumeroIdentificacion, movimiento.Activo?.Descripcion,
                        ComponentesReportePdf.Valor(movimiento.MovimientosOrigen?.UbicacionNombre, "Sin ubicación de origen"),
                        ComponentesReportePdf.Valor(movimiento.MovimientosDestino?.UbicacionNombre, "Sin ubicación de destino"),
                        movimiento.Responsable,
                        ComponentesReportePdf.Valor(movimiento.Observacion, "Sin observación registrada"));
                }
            });
    }
}
