using InventarioEscuela.Data;
using InventarioEscuela.Models;
using InventarioEscuela.ViewModels;
using InventarioEscuela.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventarioEscuela.Controllers
{
    [Authorize(Roles = "Administrador,Secretario,Usuario")]
    public class MovimientosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MovimientosController> _logger;

        public MovimientosController(
            ApplicationDbContext context,
            ILogger<MovimientosController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // =========================================
        // MOVIMIENTOS
        // FILTROS + PAGINACIÓN
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idUbicacionOrigenFiltro,
            int? idUbicacionDestinoFiltro,
            DateTime? fechaFiltro,
            int? idEstadoFiltro,
            string? responsableFiltro,
            int pagina = 1)
        {
            const int tamanoPagina = 10;


            // =====================================
            // CONSULTA BASE
            // =====================================
            var consulta = _context.MovimientosActivos
                .AsNoTracking()
                .Include(m => m.Activo)
                    .ThenInclude(a => a.EstadoActivo)
                .Include(m => m.MovimientosOrigen)
                .Include(m => m.MovimientosDestino)
                .AsQueryable();


            // =====================================
            // FILTRO POR CÓDIGO
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                codigoFiltro))
            {
                codigoFiltro =
                    codigoFiltro.Trim();

                consulta = consulta.Where(m =>
                    m.Activo.NumeroIdentificacion
                        .Contains(codigoFiltro));
            }


            // =====================================
            // FILTRO POR DESCRIPCIÓN
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                descripcionFiltro))
            {
                descripcionFiltro =
                    descripcionFiltro.Trim();

                consulta = consulta.Where(m =>
                    m.Activo.Descripcion
                        .Contains(descripcionFiltro));
            }


            // =====================================
            // FILTRO UBICACIÓN ORIGEN
            // =====================================
            if (idUbicacionOrigenFiltro.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.UbicacionOrigen ==
                    idUbicacionOrigenFiltro.Value);
            }


            // =====================================
            // FILTRO UBICACIÓN DESTINO
            // =====================================
            if (idUbicacionDestinoFiltro.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.UbicacionDestino ==
                    idUbicacionDestinoFiltro.Value);
            }


            // =====================================
            // FILTRO FECHA
            // =====================================
            if (fechaFiltro.HasValue)
            {
                DateTime inicio =
                    fechaFiltro.Value.Date;

                DateTime fin =
                    inicio.AddDays(1);

                consulta = consulta.Where(m =>
                    m.FechaMovimiento >= inicio &&
                    m.FechaMovimiento < fin);
            }


            // =====================================
            // FILTRO ESTADO ACTUAL
            // =====================================
            if (idEstadoFiltro.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.Activo.IdEstado ==
                    idEstadoFiltro.Value);
            }


            // =====================================
            // FILTRO RESPONSABLE
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                responsableFiltro))
            {
                responsableFiltro =
                    responsableFiltro.Trim();

                consulta = consulta.Where(m =>
                    m.Responsable != null &&
                    m.Responsable.Contains(
                        responsableFiltro));
            }


            // =====================================
            // TOTAL RESULTADOS FILTRADOS
            // =====================================
            int totalResultados =
                await consulta.CountAsync();


            // =====================================
            // TOTAL PÁGINAS
            // =====================================
            int totalPaginas =
                (int)Math.Ceiling(
                    totalResultados /
                    (double)tamanoPagina);


            if (pagina < 1)
            {
                pagina = 1;
            }


            if (totalPaginas > 0 &&
                pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }


            // =====================================
            // SOLO 10 MOVIMIENTOS
            // =====================================
            var movimientos =
                await consulta
                    .OrderByDescending(m =>
                        m.FechaMovimiento)
                    .ThenByDescending(m =>
                        m.IdMovimiento)
                    .Skip(
                        (pagina - 1) *
                        tamanoPagina)
                    .Take(tamanoPagina)
                    .ToListAsync();


            // =====================================
            // DASHBOARD GENERAL
            // =====================================
            int totalMovimientos =
                await _context.MovimientosActivos
                    .CountAsync();


            DateTime hoy =
                DateTime.Today;

            DateTime manana =
                hoy.AddDays(1);


            int movimientosHoy =
                await _context.MovimientosActivos
                    .CountAsync(m =>
                        m.FechaMovimiento >= hoy &&
                        m.FechaMovimiento < manana);


            int activosMovidos =
                await _context.MovimientosActivos
                    .Select(m => m.IdActivo)
                    .Distinct()
                    .CountAsync();


            int ubicacionesDestino =
                await _context.MovimientosActivos
                    .Select(m =>
                        m.UbicacionDestino)
                    .Distinct()
                    .CountAsync();


            // =====================================
            // LISTAS PARA FILTROS
            // =====================================
            var ubicaciones =
                await _context.Ubicaciones
                    .AsNoTracking()
                    .OrderBy(u =>
                        u.UbicacionNombre)
                    .ToListAsync();


            var estados =
                await _context.EstadosActivos
                    .AsNoTracking()
                    .OrderBy(e =>
                        e.Estado)
                    .ToListAsync();


            // =====================================
            // VIEWMODEL
            // =====================================
            var viewModel =
                new MovimientoIndexViewModel
                {
                    Movimientos =
                        movimientos,

                    CodigoFiltro =
                        codigoFiltro,

                    DescripcionFiltro =
                        descripcionFiltro,

                    IdUbicacionOrigenFiltro =
                        idUbicacionOrigenFiltro,

                    IdUbicacionDestinoFiltro =
                        idUbicacionDestinoFiltro,

                    FechaFiltro =
                        fechaFiltro,

                    IdEstadoFiltro =
                        idEstadoFiltro,

                    ResponsableFiltro =
                        responsableFiltro,

                    Ubicaciones =
                        ubicaciones,

                    Estados =
                        estados,

                    TotalMovimientos =
                        totalMovimientos,

                    MovimientosHoy =
                        movimientosHoy,

                    ActivosMovidos =
                        activosMovidos,

                    UbicacionesDestino =
                        ubicacionesDestino,

                    PaginaActual =
                        pagina,

                    TamanoPagina =
                        tamanoPagina,

                    TotalResultados =
                        totalResultados,

                    TotalPaginas =
                        totalPaginas
                };


            return View(viewModel);
        }


        // =========================================
        // CREAR MOVIMIENTO
        // =========================================
        [HttpGet]
        [Authorize(Roles = "Administrador,Secretario")]
        public async Task<IActionResult> Create(int? idActivo = null)
        {
            var model = new MovimientoCreateViewModel { IdActivo = idActivo ?? 0 };
            try
            {
                if (idActivo.HasValue && !await _context.Activos.AnyAsync(a => a.IdActivo == idActivo.Value))
                    return NotFound();
            }
            catch (Exception ex) when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
            {
                _logger.LogError(ex, "Error al consultar el activo para un movimiento.");
                ModelState.AddModelError(string.Empty, "No se pudo consultar el activo en este momento. Intente nuevamente.");
            }
            await CargarListasMovimiento(model.IdActivo);
            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Secretario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovimientoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarListasMovimiento(model.IdActivo, model.UbicacionDestino);
                return View(model);
            }

            try
            {
                var activo = await _context.Activos.AsNoTracking()
                    .FirstOrDefaultAsync(a => a.IdActivo == model.IdActivo);
                if (activo == null)
                {
                    ModelState.AddModelError(nameof(model.IdActivo), "El activo seleccionado no existe.");
                }
                else if (activo.IdUbicacion == model.UbicacionDestino)
                {
                    ModelState.AddModelError(nameof(model.UbicacionDestino), "El activo ya se encuentra en esa ubicación.");
                }
                if (!await _context.Ubicaciones.AnyAsync(u => u.IdUbicacion == model.UbicacionDestino))
                    ModelState.AddModelError(nameof(model.UbicacionDestino), "La ubicación de destino no existe.");

                if (!ModelState.IsValid)
                {
                    await CargarListasMovimiento(model.IdActivo, model.UbicacionDestino);
                    return View(model);
                }

                int ubicacionAnterior = activo!.IdUbicacion;
                int filasActualizadas;
                await using (var transaccion = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        filasActualizadas = await _context.Activos
                            .Where(a => a.IdActivo == model.IdActivo && a.IdUbicacion == ubicacionAnterior)
                            .ExecuteUpdateAsync(setters => setters.SetProperty(a => a.IdUbicacion, model.UbicacionDestino));

                        if (filasActualizadas == 1)
                        {
                            _context.MovimientosActivos.Add(new MovimientoActivo
                            {
                                IdActivo = activo.IdActivo,
                                UbicacionOrigen = ubicacionAnterior,
                                UbicacionDestino = model.UbicacionDestino,
                                FechaMovimiento = DateTime.Now,
                                Responsable = string.IsNullOrWhiteSpace(model.Responsable)
                                    ? User.Identity?.Name ?? "Usuario" : model.Responsable.Trim(),
                                Observacion = model.Observacion?.Trim() ?? string.Empty
                            });
                            await _context.SaveChangesAsync();
                            await transaccion.CommitAsync();
                        }
                        else
                        {
                            await transaccion.RollbackAsync();
                        }
                    }
                    catch
                    {
                        // Conservar el error original aunque falle la limpieza de la transacción.
                        try { await transaccion.RollbackAsync(); }
                        catch (Exception rollbackError)
                        {
                            _logger.LogError(rollbackError, "Error al revertir una transacción de movimiento.");
                        }
                        throw;
                    }
                }

                if (filasActualizadas != 1)
                {
                    ModelState.AddModelError(string.Empty,
                        "La ubicación cambió mientras se procesaba el traslado, o el activo fue eliminado. " +
                        "Consulte el inventario y revise el movimiento antes de intentarlo nuevamente.");
                    await CargarListasMovimiento(model.IdActivo, model.UbicacionDestino);
                    return View(model);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
            {
                _logger.LogError(ex, "Error de base de datos al registrar un movimiento.");
                _context.ChangeTracker.Clear();
                ModelState.AddModelError(string.Empty, ErroresBaseDeDatos.MensajeGuardado(ex));
                await CargarListasMovimiento(model.IdActivo, model.UbicacionDestino);
                return View(model);
            }
        }

        private async Task CargarListasMovimiento(int? idActivo = null, int? idUbicacionDestino = null)
        {
            try
            {
                ViewBag.Activos = new SelectList(await _context.Activos.AsNoTracking()
                    .OrderBy(a => a.NumeroIdentificacion).ToListAsync(), "IdActivo", "NumeroIdentificacion", idActivo);
                ViewBag.Ubicaciones = new SelectList(await _context.Ubicaciones.AsNoTracking()
                    .OrderBy(u => u.UbicacionNombre).ToListAsync(), "IdUbicacion", "UbicacionNombre", idUbicacionDestino);
            }
            catch (Exception ex) when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
            {
                _logger.LogError(ex, "Error al recargar las opciones de movimientos.");
                ModelState.AddModelError(string.Empty, "No se pudieron cargar todas las opciones. Intente cargar el formulario nuevamente.");
                ViewData["Activos"] ??= OpcionesAnteriores(idActivo);
                ViewData["Ubicaciones"] ??= OpcionesAnteriores(idUbicacionDestino);
            }
        }

        private static List<SelectListItem> OpcionesAnteriores(int? id) =>
            id > 0 ? new List<SelectListItem>
            {
                new SelectListItem("Selección anterior", id.Value.ToString())
            } : new List<SelectListItem>();
    }
}
