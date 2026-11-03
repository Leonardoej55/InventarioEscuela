using InventarioEscuela.ViewModels;
using QuestPDF.Fluent;

namespace InventarioEscuela.Services.Reportes.Pdf;

internal sealed class PlantillaEquiposDanados(
    ReporteEquiposDanadosViewModel reporte, CancellationToken ct) : IPlantillaReportePdf
{
    public string Titulo => "Activos dañados";
    public string NombreArchivo => "EquiposDanados";
    public ReporteBaseViewModel Reporte => reporte;

    public void Componer(ColumnDescriptor columna)
    {
        ComponentesReportePdf.Resumen(columna,
            ("Activos dañados", reporte.TotalResultados), ("Ubicaciones", reporte.TotalUbicaciones),
            ("Marcas", reporte.TotalMarcas), ("Con observación", reporte.TotalConObservacion));

        if (reporte.Activos.Count == 0)
        {
            ComponentesReportePdf.SinResultados(columna);
            return;
        }

        ComponentesReportePdf.Tabla(columna,
            ["Identificación", "Descripción", "Marca / modelo", "Serie", "Ubicación actual", "Adquisición", "Observación registrada"],
            [1.2f, 2, 1.6f, 1.3f, 1.5f, 1.4f, 2.5f], tabla =>
            {
                int indice = 0;
                foreach (var activo in reporte.Activos)
                {
                    ct.ThrowIfCancellationRequested();
                    ComponentesReportePdf.Fila(tabla, indice++, activo.NumeroIdentificacion, activo.Descripcion,
                        ComponentesReportePdf.MarcaYModelo(activo), activo.Serie,
                        ComponentesReportePdf.Valor(activo.Ubicacion?.UbicacionNombre, "Sin ubicación"),
                        activo.ModoAdquisicion?.NombreModoAdquisicion,
                        ComponentesReportePdf.Valor(activo.Observacion, "Sin observación registrada"));
                }
            });
    }
}
