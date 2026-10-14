using InventarioEscuela.Data;
using InventarioEscuela.Models;
using InventarioEscuela.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventarioEscuela.Controllers
{
    [Authorize]
    public class MovimientosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovimientosController(
            ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> Create()
        {
            await CargarListasMovimiento();

            var model =
                new MovimientoCreateViewModel();

            return View(model);
        }


        // =========================================
        // CREAR MOVIMIENTO POST
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MovimientoCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarListasMovimiento(
                    model.IdActivo,
                    model.UbicacionDestino);

                return View(model);
            }


            // =====================================
            // BUSCAR ACTIVO
            // =====================================
            var activo =
                await _context.Activos
                    .FirstOrDefaultAsync(a =>
                        a.IdActivo ==
                        model.IdActivo);


            if (activo == null)
            {
                ModelState.AddModelError(
                    nameof(model.IdActivo),
                    "El activo seleccionado no existe.");

                await CargarListasMovimiento(
                    model.IdActivo,
                    model.UbicacionDestino);

                return View(model);
            }


            // =====================================
            // VALIDAR UBICACIÓN DESTINO
            // =====================================
            bool existeDestino =
                await _context.Ubicaciones
                    .AnyAsync(u =>
                        u.IdUbicacion ==
                        model.UbicacionDestino);


            if (!existeDestino)
            {
                ModelState.AddModelError(
                    nameof(model.UbicacionDestino),
                    "La ubicación de destino no existe.");

                await CargarListasMovimiento(
                    model.IdActivo,
                    model.UbicacionDestino);

                return View(model);
            }


            // =====================================
            // EVITAR MISMA UBICACIÓN
            // =====================================
            if (activo.IdUbicacion ==
                model.UbicacionDestino)
            {
                ModelState.AddModelError(
                    nameof(model.UbicacionDestino),
                    "El activo ya se encuentra en esa ubicación.");

                await CargarListasMovimiento(
                    model.IdActivo,
                    model.UbicacionDestino);

                return View(model);
            }


            int ubicacionAnterior =
                activo.IdUbicacion;


            // =====================================
            // TRANSACCIÓN
            // =====================================
            await using var transaccion =
                await _context.Database
                    .BeginTransactionAsync();


            try
            {
                // =================================
                // CREAR MOVIMIENTO
                // =================================
                var movimiento =
                    new MovimientoActivo
                    {
                        IdActivo =
                            activo.IdActivo,

                        UbicacionOrigen =
                            ubicacionAnterior,

                        UbicacionDestino =
                            model.UbicacionDestino,

                        FechaMovimiento =
                            DateTime.Now,

                        Responsable =
                            model.Responsable,

                        Observacion =
                            model.Observacion
                    };


                _context.MovimientosActivos
                    .Add(movimiento);


                // =================================
                // ACTUALIZAR UBICACIÓN DEL ACTIVO
                // =================================
                activo.IdUbicacion =
                    model.UbicacionDestino;


                await _context
                    .SaveChangesAsync();


                await transaccion
                    .CommitAsync();
            }
            catch
            {
                await transaccion
                    .RollbackAsync();

                throw;
            }


            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // CARGAR SELECTS DE MOVIMIENTO
        // =========================================
        private async Task CargarListasMovimiento(
            int? idActivo = null,
            int? idUbicacionDestino = null)
        {
            var activos =
                await _context.Activos
                    .AsNoTracking()
                    .OrderBy(a =>
                        a.NumeroIdentificacion)
                    .ToListAsync();


            ViewBag.Activos =
                new SelectList(
                    activos,
                    "IdActivo",
                    "NumeroIdentificacion",
                    idActivo);


            var ubicaciones =
                await _context.Ubicaciones
                    .AsNoTracking()
                    .OrderBy(u =>
                        u.UbicacionNombre)
                    .ToListAsync();


            ViewBag.Ubicaciones =
                new SelectList(
                    ubicaciones,
                    "IdUbicacion",
                    "UbicacionNombre",
                    idUbicacionDestino);
        }
    }
}