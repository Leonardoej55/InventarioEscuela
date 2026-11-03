using InventarioEscuela.ViewModels;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace InventarioEscuela.Services.Reportes.Pdf;

/// <summary>
/// Genera documentos a partir de resultados completos ya materializados.
/// Cada plantilla define su contenido y comparte únicamente la presentación institucional.
/// </summary>
public sealed class ReportePdfService : IReportePdfService
{
    public ArchivoReporte GenerarInventarioGeneral(ReporteInventarioGeneralViewModel reporte,
        ContextoReportePdf contexto, CancellationToken ct = default) =>
        Generar(new PlantillaInventarioGeneral(reporte, ct), contexto, ct);

    public ArchivoReporte GenerarRecursosPorUbicacion(ReporteRecursosPorUbicacionViewModel reporte,
        ContextoReportePdf contexto, CancellationToken ct = default) =>
        Generar(new PlantillaRecursosPorUbicacion(reporte, ct), contexto, ct);

    public ArchivoReporte GenerarEquiposDanados(ReporteEquiposDanadosViewModel reporte,
        ContextoReportePdf contexto, CancellationToken ct = default) =>
        Generar(new PlantillaEquiposDanados(reporte, ct), contexto, ct);

    public ArchivoReporte GenerarEquiposMantenimiento(ReporteEquiposMantenimientoViewModel reporte,
        ContextoReportePdf contexto, CancellationToken ct = default) =>
        Generar(new PlantillaHistorialMantenimientos(reporte, ct), contexto, ct);

    public ArchivoReporte GenerarHistorialMovimientos(ReporteHistorialMovimientosViewModel reporte,
        ContextoReportePdf contexto, CancellationToken ct = default) =>
        Generar(new PlantillaHistorialMovimientos(reporte, ct), contexto, ct);

    public ArchivoReporte GenerarVerificacionAnual(ReporteRecursosPorUbicacionViewModel reporte,
        ContextoReportePdf contexto, CancellationToken ct = default) =>
        Generar(new PlantillaVerificacionAnual(reporte, ct), contexto, ct);

    private static ArchivoReporte Generar(IPlantillaReportePdf plantilla, ContextoReportePdf contexto,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(contexto);
        ct.ThrowIfCancellationRequested();

        var documento = Document.Create(contenedor => contenedor.Page(pagina =>
        {
            ComponentesReportePdf.ConfigurarPagina(pagina);
            pagina.Header().Element(c => ComponentesReportePdf.Encabezado(c, plantilla.Titulo, contexto));
            pagina.Content().PaddingVertical(12).Column(columna =>
            {
                columna.Spacing(10);
                columna.Item().Element(c => ComponentesReportePdf.FiltrosYAlcance(c, plantilla.Reporte));
                plantilla.Componer(columna);
            });
            pagina.Footer().Element(ComponentesReportePdf.PiePagina);
        })).WithMetadata(new DocumentMetadata
        {
            Title = plantilla.Titulo,
            Author = ComponentesReportePdf.Valor(contexto.GeneradoPor),
            Subject = "Sistema de Inventario Institucional · Escuela Proyecto Pacuare",
            Creator = "InventarioEscuela",
            CreationDate = contexto.FechaGeneracion,
            ModifiedDate = contexto.FechaGeneracion,
            Language = "es-CR"
        });

        byte[] contenido = documento.GeneratePdf();
        ct.ThrowIfCancellationRequested();
        return new ArchivoReporte(contenido,
            $"{plantilla.NombreArchivo}_{contexto.FechaGeneracion:yyyyMMdd_HHmmss}.pdf", "application/pdf");
    }
}
