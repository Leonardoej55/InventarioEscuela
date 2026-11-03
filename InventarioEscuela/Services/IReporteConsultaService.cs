using InventarioEscuela.ViewModels;

namespace InventarioEscuela.Services;

public interface IReporteConsultaService
{
    Task<ReporteInventarioGeneralViewModel> PrepararInventarioGeneralAsync(ReporteInventarioGeneralViewModel filtros, CancellationToken ct = default);
    Task<ReporteInventarioGeneralViewModel> ConsultarInventarioGeneralAsync(ReporteInventarioGeneralViewModel filtros, int? pagina = 1, CancellationToken ct = default);
    Task<ReporteRecursosPorUbicacionViewModel> PrepararRecursosPorUbicacionAsync(ReporteRecursosPorUbicacionViewModel filtros, CancellationToken ct = default);
    Task<ReporteRecursosPorUbicacionViewModel> ConsultarRecursosPorUbicacionAsync(ReporteRecursosPorUbicacionViewModel filtros, int? pagina = 1, CancellationToken ct = default);
    Task<ReporteEquiposDanadosViewModel> PrepararEquiposDanadosAsync(ReporteEquiposDanadosViewModel filtros, CancellationToken ct = default);
    Task<ReporteEquiposDanadosViewModel> ConsultarEquiposDanadosAsync(ReporteEquiposDanadosViewModel filtros, int? pagina = 1, CancellationToken ct = default);
    Task<ReporteEquiposMantenimientoViewModel> PrepararEquiposMantenimientoAsync(ReporteEquiposMantenimientoViewModel filtros, CancellationToken ct = default);
    Task<ReporteEquiposMantenimientoViewModel> ConsultarEquiposMantenimientoAsync(ReporteEquiposMantenimientoViewModel filtros, int? pagina = 1, CancellationToken ct = default);
    Task<ReporteHistorialMovimientosViewModel> PrepararHistorialMovimientosAsync(ReporteHistorialMovimientosViewModel filtros, CancellationToken ct = default);
    Task<ReporteHistorialMovimientosViewModel> ConsultarHistorialMovimientosAsync(ReporteHistorialMovimientosViewModel filtros, int? pagina = 1, CancellationToken ct = default);
}
