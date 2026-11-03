using InventarioEscuela.Services.Reportes;
using InventarioEscuela.ViewModels;

namespace InventarioEscuela.Services;

public interface IReporteCsvService
{
    ArchivoReporte GenerarInventarioGeneral(ReporteInventarioGeneralViewModel reporte, CancellationToken cancellationToken = default);
    ArchivoReporte GenerarRecursosPorUbicacion(ReporteRecursosPorUbicacionViewModel reporte, CancellationToken cancellationToken = default);
    ArchivoReporte GenerarEquiposDanados(ReporteEquiposDanadosViewModel reporte, CancellationToken cancellationToken = default);
    ArchivoReporte GenerarEquiposMantenimiento(ReporteEquiposMantenimientoViewModel reporte, CancellationToken cancellationToken = default);
    ArchivoReporte GenerarHistorialMovimientos(ReporteHistorialMovimientosViewModel reporte, CancellationToken cancellationToken = default);
}
