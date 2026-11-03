using InventarioEscuela.ViewModels;

namespace InventarioEscuela.Services.Reportes.Pdf;

/// <summary>Metadatos explícitos para reproducir un reporte sin depender de HttpContext o del reloj.</summary>
public record ContextoReportePdf(string GeneradoPor, DateTimeOffset FechaGeneracion);

public interface IReportePdfService
{
    ArchivoReporte GenerarInventarioGeneral(ReporteInventarioGeneralViewModel reporte,
        ContextoReportePdf contexto, CancellationToken ct = default);
    ArchivoReporte GenerarRecursosPorUbicacion(ReporteRecursosPorUbicacionViewModel reporte,
        ContextoReportePdf contexto, CancellationToken ct = default);
    ArchivoReporte GenerarEquiposDanados(ReporteEquiposDanadosViewModel reporte,
        ContextoReportePdf contexto, CancellationToken ct = default);
    ArchivoReporte GenerarEquiposMantenimiento(ReporteEquiposMantenimientoViewModel reporte,
        ContextoReportePdf contexto, CancellationToken ct = default);
    ArchivoReporte GenerarHistorialMovimientos(ReporteHistorialMovimientosViewModel reporte,
        ContextoReportePdf contexto, CancellationToken ct = default);
    ArchivoReporte GenerarVerificacionAnual(ReporteRecursosPorUbicacionViewModel reporte,
        ContextoReportePdf contexto, CancellationToken ct = default);
}
