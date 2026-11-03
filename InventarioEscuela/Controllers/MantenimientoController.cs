using InventarioEscuela.Data;
using InventarioEscuela.Models;
using InventarioEscuela.Services;
using InventarioEscuela.Services.Reportes;
using InventarioEscuela.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventarioEscuela.Controllers
{
    [Authorize(Roles = "Administrador,Secretario,Usuario")]
    public class MantenimientoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RelojSistema _reloj;
        private readonly ILogger<MantenimientoController> _logger;

        public MantenimientoController(
            ApplicationDbContext context,
            RelojSistema reloj,
            ILogger<MantenimientoController> logger)
        {
            _context = context;
            _reloj = reloj;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? codigoFiltro,
            string? descripcionActivoFiltro,
            int? idUbicacionFiltro,
            int? idEstadoFiltro,
            int? idTipoFiltro,
            string? responsableFiltro,
            string? descripcionMantenimientoFiltro,
            int pagina = 1)
        {
            try
            {
                const int tamanoPagina = 10;

                var consulta = _context.Mantenimientos
                    .AsNoTracking()
                    .Include(m => m.Activo)
                        .ThenInclude(a => a.Ubicacion)
                    .Include(m => m.Activo)
                        .ThenInclude(a => a.EstadoActivo)
                    .Include(m => m.TipoMantenimiento)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(codigoFiltro))
                {
                    codigoFiltro = codigoFiltro.Trim();
                    consulta = consulta.Where(m =>
                        m.Activo.NumeroIdentificacion.Contains(codigoFiltro));
                }

                if (!string.IsNullOrWhiteSpace(descripcionActivoFiltro))
                {
                    descripcionActivoFiltro = descripcionActivoFiltro.Trim();
                    consulta = consulta.Where(m =>
                        m.Activo.Descripcion.Contains(descripcionActivoFiltro));
                }

                if (idUbicacionFiltro.HasValue)
                    consulta = consulta.Where(m =>
                        m.Activo.IdUbicacion == idUbicacionFiltro.Value);

                if (idEstadoFiltro.HasValue)
                    consulta = consulta.Where(m =>
                        m.Activo.IdEstado == idEstadoFiltro.Value);

                if (idTipoFiltro.HasValue)
                    consulta = consulta.Where(m =>
                        m.IdTipo == idTipoFiltro.Value);

                if (!string.IsNullOrWhiteSpace(responsableFiltro))
                {
                    responsableFiltro = responsableFiltro.Trim();
                    consulta = consulta.Where(m =>
                        m.Responsable != null &&
                        m.Responsable.Contains(responsableFiltro));
                }

                if (!string.IsNullOrWhiteSpace(descripcionMantenimientoFiltro))
                {
                    descripcionMantenimientoFiltro =
                        descripcionMantenimientoFiltro.Trim();

                    consulta = consulta.Where(m =>
                        m.Descripcion != null &&
                        m.Descripcion.Contains(descripcionMantenimientoFiltro));
                }

                int totalResultados = await consulta.CountAsync();
                int totalPaginas = (int)Math.Ceiling(
                    totalResultados / (double)tamanoPagina);

                if (pagina < 1)
                    pagina = 1;

                if (totalPaginas > 0 && pagina > totalPaginas)
                    pagina = totalPaginas;

                var mantenimientos = await consulta
                    .OrderByDescending(m => m.FechaMantenimiento)
                    .ThenByDescending(m => m.IdMantenimiento)
                    .Skip((pagina - 1) * tamanoPagina)
                    .Take(tamanoPagina)
                    .ToListAsync();

                int totalMantenimientos =
                    await _context.Mantenimientos.CountAsync();

                int activosConMantenimiento = await _context.Mantenimientos
                    .Select(m => m.IdActivo)
                    .Distinct()
                    .CountAsync();

                DateTime hoy = _reloj.Hoy;
                DateTime inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
                DateTime inicioMesSiguiente = inicioMes.AddMonths(1);

                int mantenimientosEsteMes =
                    await _context.Mantenimientos.CountAsync(m =>
                        m.FechaMantenimiento >= inicioMes &&
                        m.FechaMantenimiento < inicioMesSiguiente);

                int activosDanados = await _context.Activos
                    .CountAsync(EstadosReporte.EsDanado);

                int activosEnMantenimiento = await _context.Activos
                    .CountAsync(EstadosReporte.EsEnMantenimiento);

                var ubicaciones = await _context.Ubicaciones
                    .AsNoTracking()
                    .OrderBy(u => u.UbicacionNombre)
                    .ToListAsync();

                var estados = await _context.EstadosActivos
                    .AsNoTracking()
                    .OrderBy(e => e.Estado)
                    .ToListAsync();

                var tipos = await _context.TiposMantenimiento
                    .AsNoTracking()
                    .OrderBy(t => t.Mantenimiento)
                    .ToListAsync();

                return View(new MantenimientoIndexViewModel
                {
                    Mantenimientos = mantenimientos,
                    CodigoFiltro = codigoFiltro,
                    DescripcionActivoFiltro = descripcionActivoFiltro,
                    IdUbicacionFiltro = idUbicacionFiltro,
                    IdEstadoFiltro = idEstadoFiltro,
                    IdTipoFiltro = idTipoFiltro,
                    ResponsableFiltro = responsableFiltro,
                    DescripcionMantenimientoFiltro =
                        descripcionMantenimientoFiltro,
                    Ubicaciones = ubicaciones,
                    Estados = estados,
                    TiposMantenimiento = tipos,
                    TotalMantenimientos = totalMantenimientos,
                    ActivosConMantenimiento = activosConMantenimiento,
                    MantenimientosEsteMes = mantenimientosEsteMes,
                    ActivosDanados = activosDanados,
                    ActivosEnMantenimiento = activosEnMantenimiento,
                    PaginaActual = pagina,
                    TamanoPagina = tamanoPagina,
                    TotalResultados = totalResultados,
                    TotalPaginas = totalPaginas
                });
            }
            catch (Exception ex)
                when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
            {
                _logger.LogError(
                    ex, "Error de base de datos al consultar los mantenimientos.");

                Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

                return View("ErrorOperacion", new ErrorOperacionViewModel
                {
                    Titulo = "No se pudo cargar el historial de mantenimientos",
                    Mensaje =
                        "No se pudo consultar la información en este momento. " +
                        "Intente cargarla nuevamente.",
                    UrlReintento = Url.Action(
                        nameof(Index), "Mantenimiento", new
                        {
                            codigoFiltro,
                            descripcionActivoFiltro,
                            idUbicacionFiltro,
                            idEstadoFiltro,
                            idTipoFiltro,
                            responsableFiltro,
                            descripcionMantenimientoFiltro,
                            pagina
                        }) ?? string.Empty
                });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Secretario")]
        public async Task<IActionResult> Create()
        {
            var model = new MantenimientoCreateViewModel
            {
                FechaMantenimiento = _reloj.Hoy
            };

            await CargarListasMantenimiento();
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Secretario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MantenimientoCreateViewModel model)
        {
            model.Responsable = model.Responsable?.Trim() ?? string.Empty;

            if (!ModelState.IsValid)
            {
                await CargarListasMantenimiento(model.IdActivo, model.IdTipo);
                return View(model);
            }

            try
            {
                bool existeActivo = await _context.Activos
                    .AnyAsync(a => a.IdActivo == model.IdActivo);

                if (!existeActivo)
                    ModelState.AddModelError(
                        nameof(model.IdActivo),
                        "El activo seleccionado no existe.");

                bool existeTipo = await _context.TiposMantenimiento
                    .AnyAsync(t => t.IdTipo == model.IdTipo);

                if (!existeTipo)
                    ModelState.AddModelError(
                        nameof(model.IdTipo),
                        "El tipo de mantenimiento seleccionado no existe.");

                if (!ModelState.IsValid)
                {
                    await CargarListasMantenimiento(model.IdActivo, model.IdTipo);
                    return View(model);
                }

                var mantenimiento = new Mantenimiento
                {
                    IdActivo = model.IdActivo,
                    IdTipo = model.IdTipo,
                    FechaMantenimiento = model.FechaMantenimiento!.Value,
                    Descripcion = model.Descripcion,
                    Responsable = model.Responsable
                };

                _context.Mantenimientos.Add(mantenimiento);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
                when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
            {
                _logger.LogError(
                    ex, "Error de base de datos al registrar un mantenimiento.");

                _context.ChangeTracker.Clear();
                Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

                ModelState.AddModelError(
                    string.Empty, ErroresBaseDeDatos.MensajeGuardado(ex));

                await CargarListasMantenimiento(model.IdActivo, model.IdTipo);
                return View(model);
            }
        }

        private async Task CargarListasMantenimiento(
            int? idActivo = null,
            int? idTipo = null)
        {
            try
            {
                var activos = await _context.Activos
                    .AsNoTracking()
                    .OrderBy(a => a.NumeroIdentificacion)
                    .ToListAsync();

                ViewBag.Activos = new SelectList(
                    activos, "IdActivo", "NumeroIdentificacion", idActivo);

                var tipos = await _context.TiposMantenimiento
                    .AsNoTracking()
                    .OrderBy(t => t.Mantenimiento)
                    .ToListAsync();

                ViewBag.TiposMantenimiento = new SelectList(
                    tipos, "IdTipo", "Mantenimiento", idTipo);
            }
            catch (Exception ex)
                when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
            {
                _logger.LogError(
                    ex, "Error al cargar las opciones de mantenimientos.");

                Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudieron cargar todas las opciones. " +
                    "Sus datos se conservan; intente cargar el formulario nuevamente.");

                ViewData["Activos"] ??= OpcionesAnteriores(idActivo);
                ViewData["TiposMantenimiento"] ??= OpcionesAnteriores(idTipo);
            }
        }

        private static List<SelectListItem> OpcionesAnteriores(int? id) =>
            id > 0
                ? new List<SelectListItem>
                {
                    new SelectListItem(
                        "Selección anterior", id.Value.ToString())
                }
                : new List<SelectListItem>();
    }
}