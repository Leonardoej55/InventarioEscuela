using System.Security.Claims;
using InventarioEscuela.Services;
using InventarioEscuela.Services.Reportes;
using InventarioEscuela.Services.Reportes.Pdf;
using InventarioEscuela.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace InventarioEscuela.Controllers;

[Authorize(Roles = "Administrador,Secretario,Usuario")]
public class ReportesController : Controller
{
    private readonly IReporteConsultaService _consultas;
    private readonly IReportePdfService _pdf;
    private readonly IReporteCsvService _csv;
    private readonly TimeProvider _reloj;
    private readonly TimeZoneInfo _zonaHoraria;
    private readonly ILogger<ReportesController> _logger;

    public ReportesController(
        IReporteConsultaService consultas,
        IReportePdfService pdf,
        IReporteCsvService csv,
        TimeProvider reloj,
        IOptions<OpcionesReportes> opciones,
        ILogger<ReportesController> logger)
    {
        _consultas = consultas;
        _pdf = pdf;
        _csv = csv;
        _reloj = reloj;
        _zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById(opciones.Value.ZonaHoraria);
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index() => View();

    [HttpGet]
    public Task<IActionResult> InventarioGeneral([FromQuery] ReporteInventarioGeneralViewModel filtros, CancellationToken ct = default) =>
        ConfigurarAsync(filtros, nameof(InventarioGeneral), _consultas.PrepararInventarioGeneralAsync, ct);

    [HttpGet]
    public Task<IActionResult> InventarioGeneralPreview([FromQuery] ReporteInventarioGeneralViewModel filtros, int pagina = 1, CancellationToken ct = default) =>
        ConsultarAsync(filtros, pagina, nameof(InventarioGeneral), nameof(InventarioGeneralPreview),
            _consultas.PrepararInventarioGeneralAsync, _consultas.ConsultarInventarioGeneralAsync, ct);

    [HttpGet]
    public Task<IActionResult> GenerarInventarioGeneralPdf([FromQuery] ReporteInventarioGeneralViewModel filtros, CancellationToken ct = default) =>
        ExportarAsync(filtros, nameof(InventarioGeneral), _consultas.PrepararInventarioGeneralAsync,
            _consultas.ConsultarInventarioGeneralAsync,
            (reporte, token) => _pdf.GenerarInventarioGeneral(reporte, CrearContextoPdf(), token), ct);

    [HttpGet]
    public Task<IActionResult> GenerarInventarioGeneralCsv([FromQuery] ReporteInventarioGeneralViewModel filtros, CancellationToken ct = default) =>
        ExportarAsync(filtros, nameof(InventarioGeneral), _consultas.PrepararInventarioGeneralAsync,
            _consultas.ConsultarInventarioGeneralAsync, _csv.GenerarInventarioGeneral, ct);

    [HttpGet]
    public Task<IActionResult> RecursosPorUbicacion([FromQuery] ReporteRecursosPorUbicacionViewModel filtros, CancellationToken ct = default) =>
        ConfigurarAsync(filtros, nameof(RecursosPorUbicacion), _consultas.PrepararRecursosPorUbicacionAsync, ct);

    [HttpGet]
    public Task<IActionResult> RecursosPorUbicacionPreview([FromQuery] ReporteRecursosPorUbicacionViewModel filtros, int pagina = 1, CancellationToken ct = default) =>
        ConsultarAsync(filtros, pagina, nameof(RecursosPorUbicacion), nameof(RecursosPorUbicacionPreview),
            _consultas.PrepararRecursosPorUbicacionAsync, _consultas.ConsultarRecursosPorUbicacionAsync, ct);

    [HttpGet]
    public Task<IActionResult> GenerarRecursosPorUbicacionPdf([FromQuery] ReporteRecursosPorUbicacionViewModel filtros, CancellationToken ct = default) =>
        ExportarAsync(filtros, nameof(RecursosPorUbicacion), _consultas.PrepararRecursosPorUbicacionAsync,
            _consultas.ConsultarRecursosPorUbicacionAsync,
            (reporte, token) => _pdf.GenerarRecursosPorUbicacion(reporte, CrearContextoPdf(), token), ct);

    [HttpGet]
    public Task<IActionResult> GenerarRecursosPorUbicacionCsv([FromQuery] ReporteRecursosPorUbicacionViewModel filtros, CancellationToken ct = default) =>
        ExportarAsync(filtros, nameof(RecursosPorUbicacion), _consultas.PrepararRecursosPorUbicacionAsync,
            _consultas.ConsultarRecursosPorUbicacionAsync, _csv.GenerarRecursosPorUbicacion, ct);

    [HttpGet]
    public Task<IActionResult> EquiposDanados([FromQuery] ReporteEquiposDanadosViewModel filtros, CancellationToken ct = default) =>
        ConfigurarAsync(filtros, nameof(EquiposDanados), _consultas.PrepararEquiposDanadosAsync, ct);

    [HttpGet]
    public Task<IActionResult> EquiposDanadosPreview([FromQuery] ReporteEquiposDanadosViewModel filtros, int pagina = 1, CancellationToken ct = default) =>
        ConsultarAsync(filtros, pagina, nameof(EquiposDanados), nameof(EquiposDanadosPreview),
            _consultas.PrepararEquiposDanadosAsync, _consultas.ConsultarEquiposDanadosAsync, ct);

    [HttpGet]
    public Task<IActionResult> GenerarEquiposDanadosPdf([FromQuery] ReporteEquiposDanadosViewModel filtros, CancellationToken ct = default) =>
        ExportarAsync(filtros, nameof(EquiposDanados), _consultas.PrepararEquiposDanadosAsync,
            _consultas.ConsultarEquiposDanadosAsync,
            (reporte, token) => _pdf.GenerarEquiposDanados(reporte, CrearContextoPdf(), token), ct);

    [HttpGet]
    public Task<IActionResult> GenerarEquiposDanadosCsv([FromQuery] ReporteEquiposDanadosViewModel filtros, CancellationToken ct = default) =>
        ExportarAsync(filtros, nameof(EquiposDanados), _consultas.PrepararEquiposDanadosAsync,
            _consultas.ConsultarEquiposDanadosAsync, _csv.GenerarEquiposDanados, ct);

    [HttpGet]
    public Task<IActionResult> EquiposMantenimiento([FromQuery] ReporteEquiposMantenimientoViewModel filtros, CancellationToken ct = default) =>
        ConfigurarAsync(filtros, nameof(EquiposMantenimiento), _consultas.PrepararEquiposMantenimientoAsync, ct);

    [HttpGet]
    public Task<IActionResult> EquiposMantenimientoPreview([FromQuery] ReporteEquiposMantenimientoViewModel filtros, int pagina = 1, CancellationToken ct = default) =>
        ConsultarAsync(filtros, pagina, nameof(EquiposMantenimiento), nameof(EquiposMantenimientoPreview),
            _consultas.PrepararEquiposMantenimientoAsync, _consultas.ConsultarEquiposMantenimientoAsync, ct);

    [HttpGet]
    public Task<IActionResult> GenerarEquiposMantenimientoPdf([FromQuery] ReporteEquiposMantenimientoViewModel filtros, CancellationToken ct = default) =>
        ExportarAsync(filtros, nameof(EquiposMantenimiento), _consultas.PrepararEquiposMantenimientoAsync,
            _consultas.ConsultarEquiposMantenimientoAsync,
            (reporte, token) => _pdf.GenerarEquiposMantenimiento(reporte, CrearContextoPdf(), token), ct);

    [HttpGet]
    public Task<IActionResult> GenerarEquiposMantenimientoCsv([FromQuery] ReporteEquiposMantenimientoViewModel filtros, CancellationToken ct = default) =>
        ExportarAsync(filtros, nameof(EquiposMantenimiento), _consultas.PrepararEquiposMantenimientoAsync,
            _consultas.ConsultarEquiposMantenimientoAsync, _csv.GenerarEquiposMantenimiento, ct);

    [HttpGet]
    public Task<IActionResult> HistorialMovimientos([FromQuery] ReporteHistorialMovimientosViewModel filtros, CancellationToken ct = default) =>
        ConfigurarAsync(filtros, nameof(HistorialMovimientos), _consultas.PrepararHistorialMovimientosAsync, ct);

    [HttpGet]
    public Task<IActionResult> HistorialMovimientosPreview([FromQuery] ReporteHistorialMovimientosViewModel filtros, int pagina = 1, CancellationToken ct = default) =>
        ConsultarAsync(filtros, pagina, nameof(HistorialMovimientos), nameof(HistorialMovimientosPreview),
            _consultas.PrepararHistorialMovimientosAsync, _consultas.ConsultarHistorialMovimientosAsync, ct);

    [HttpGet]
    public Task<IActionResult> GenerarHistorialMovimientosPdf([FromQuery] ReporteHistorialMovimientosViewModel filtros, CancellationToken ct = default) =>
        ExportarAsync(filtros, nameof(HistorialMovimientos), _consultas.PrepararHistorialMovimientosAsync,
            _consultas.ConsultarHistorialMovimientosAsync,
            (reporte, token) => _pdf.GenerarHistorialMovimientos(reporte, CrearContextoPdf(), token), ct);

    [HttpGet]
    public Task<IActionResult> GenerarHistorialMovimientosCsv([FromQuery] ReporteHistorialMovimientosViewModel filtros, CancellationToken ct = default) =>
        ExportarAsync(filtros, nameof(HistorialMovimientos), _consultas.PrepararHistorialMovimientosAsync,
            _consultas.ConsultarHistorialMovimientosAsync, _csv.GenerarHistorialMovimientos, ct);

    [HttpGet]
    public Task<IActionResult> GenerarVerificacionAnualPdf([FromQuery] ReporteRecursosPorUbicacionViewModel filtros, CancellationToken ct = default) =>
        ExportarAsync(filtros, nameof(RecursosPorUbicacion), _consultas.PrepararRecursosPorUbicacionAsync,
            _consultas.ConsultarRecursosPorUbicacionAsync,
            (reporte, token) => _pdf.GenerarVerificacionAnual(reporte, CrearContextoPdf(), token), ct);

    private async Task<IActionResult> ConfigurarAsync<T>(T filtros, string vista,
        Func<T, CancellationToken, Task<T>> preparar, CancellationToken ct) where T : ReporteBaseViewModel
    {
        ValidarFechas(filtros);
        return await FormularioAsync(filtros, vista, preparar, ModelState.IsValid ? 200 : 400, ct);
    }

    private async Task<IActionResult> ConsultarAsync<T>(T filtros, int pagina, string configuracion, string vista,
        Func<T, CancellationToken, Task<T>> preparar,
        Func<T, int?, CancellationToken, Task<T>> consultar, CancellationToken ct) where T : ReporteBaseViewModel
    {
        ValidarFechas(filtros);
        if (!ModelState.IsValid)
            return await FormularioAsync(filtros, configuracion, preparar, 400, ct);

        try
        {
            return View(vista, await consultar(filtros, pagina, ct));
        }
        catch (Exception ex) when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
        {
            _logger.LogError(ex, "No se pudo consultar el reporte {Reporte}.", configuracion);
            ModelState.AddModelError(string.Empty, "No se pudo consultar el reporte. Sus filtros se conservan; intente nuevamente en unos momentos.");
            return await FormularioAsync(filtros, configuracion, preparar, 503, ct);
        }
    }

    private async Task<IActionResult> ExportarAsync<T>(T filtros, string configuracion,
        Func<T, CancellationToken, Task<T>> preparar,
        Func<T, int?, CancellationToken, Task<T>> consultar,
        Func<T, CancellationToken, ArchivoReporte> generar, CancellationToken ct) where T : ReporteBaseViewModel
    {
        ValidarFechas(filtros);
        if (!ModelState.IsValid)
            return await FormularioAsync(filtros, configuracion, preparar, 400, ct);

        T reporte;
        try
        {
            // Una exportación contiene todas las coincidencias, no sólo la página consultada.
            reporte = await consultar(filtros, null, ct);
        }
        catch (Exception ex) when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
        {
            _logger.LogError(ex, "No se pudieron obtener los datos para exportar {Reporte}.", configuracion);
            ModelState.AddModelError(string.Empty, "No se pudo obtener el reporte. Sus filtros se conservan; intente nuevamente en unos momentos.");
            return await FormularioAsync(filtros, configuracion, preparar, 503, ct);
        }

        if (reporte.TotalResultados == 0)
        {
            ModelState.AddModelError(string.Empty, "No hay resultados con estos filtros. Puede cambiarlos o limpiarlos para ampliar la consulta.");
            return View(configuracion, reporte);
        }

        try
        {
            ct.ThrowIfCancellationRequested();
            ArchivoReporte archivo = generar(reporte, ct);
            ct.ThrowIfCancellationRequested();
            return File(archivo.Contenido, archivo.TipoContenido, archivo.NombreArchivo);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "No se pudo generar el archivo del reporte {Reporte}.", configuracion);
            ModelState.AddModelError(string.Empty, "No se pudo generar el archivo. Sus filtros se conservan; puede intentar nuevamente o descargar el CSV.");
            Response.StatusCode = 500;
            return View(configuracion, reporte);
        }
    }

    private async Task<IActionResult> FormularioAsync<T>(T filtros, string vista,
        Func<T, CancellationToken, Task<T>> preparar, int statusCode, CancellationToken ct) where T : ReporteBaseViewModel
    {
        try
        {
            T modelo = await preparar(filtros, ct);
            Response.StatusCode = statusCode;
            return View(vista, modelo);
        }
        catch (Exception ex) when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
        {
            _logger.LogError(ex, "No se pudieron cargar las opciones del reporte {Reporte}.", vista);
            ModelState.TryAddModelError(string.Empty, "No se pudieron cargar las opciones del reporte. Intente nuevamente en unos momentos.");
            Response.StatusCode = 503;
            return View(vista, filtros);
        }
    }

    private void ValidarFechas(ReporteBaseViewModel filtros)
    {
        (DateTime? Desde, DateTime? Hasta) fechas = filtros switch
        {
            ReporteInventarioGeneralViewModel f => (f.FechaDesde, f.FechaHasta),
            ReporteEquiposMantenimientoViewModel f => (f.FechaDesde, f.FechaHasta),
            ReporteHistorialMovimientosViewModel f => (f.FechaDesde, f.FechaHasta),
            _ => (null, null)
        };
        if (fechas.Desde.HasValue && fechas.Hasta.HasValue && fechas.Desde.Value.Date > fechas.Hasta.Value.Date)
            ModelState.TryAddModelError("FechaHasta", "La fecha inicial no puede ser posterior a la fecha final.");
    }

    private ContextoReportePdf CrearContextoPdf() => new(
        User.FindFirst(ClaimTypes.Name)?.Value ?? User.Identity?.Name ?? "Usuario",
        TimeZoneInfo.ConvertTime(_reloj.GetUtcNow(), _zonaHoraria));
}
