using InventarioEscuela.ViewModels;
using QuestPDF.Fluent;

namespace InventarioEscuela.Services.Reportes.Pdf;

internal sealed class PlantillaInventarioGeneral(
    ReporteInventarioGeneralViewModel reporte, CancellationToken ct) : IPlantillaReportePdf
{
    public string Titulo => "Inventario general";
    public string NombreArchivo => "InventarioGeneral";
    public ReporteBaseViewModel Reporte => reporte;

    public void Componer(ColumnDescriptor columna)
    {
        ComponentesReportePdf.Resumen(columna,
            ("Activos", reporte.TotalResultados), ("Funcionales", reporte.TotalFuncionales),
            ("Dañados", reporte.TotalDanados), ("En mantenimiento", reporte.TotalMantenimiento),
            ("Otros estados", reporte.TotalOtrosEstados));

        if (reporte.Activos.Count == 0)
        {
            ComponentesReportePdf.SinResultados(columna);
            return;
        }

        ComponentesReportePdf.Tabla(columna,
            ["Identificación", "Descripción", "Marca / modelo", "Serie", "Ubicación actual", "Estado actual", "Adquisición", "Fecha de registro"],
            [1.2f, 2.3f, 1.6f, 1.3f, 1.5f, 1.3f, 1.3f, 1.2f], tabla =>
            {
                int indice = 0;
                foreach (var activo in reporte.Activos)
                {
                    ct.ThrowIfCancellationRequested();
                    ComponentesReportePdf.Fila(tabla, indice++,
                        activo.NumeroIdentificacion, activo.Descripcion,
                        ComponentesReportePdf.MarcaYModelo(activo), activo.Serie,
                        ComponentesReportePdf.Valor(activo.Ubicacion?.UbicacionNombre, "Sin ubicación"),
                        activo.EstadoActivo?.Estado, activo.ModoAdquisicion?.NombreModoAdquisicion,
                        ComponentesReportePdf.Fecha(activo.FechaRegistro));
                }
            });
    }
}
