using InventarioEscuela.Data;
using InventarioEscuela.Models;
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


        public MantenimientoController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // INDEX
        // FILTROS + PAGINACIÓN
        // =========================================
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
            const int tamanoPagina = 10;


            // =====================================
            // CONSULTA BASE
            // =====================================
            var consulta =
                _context.Mantenimientos
                    .AsNoTracking()
                    .Include(m => m.Activo)
                        .ThenInclude(a => a.Ubicacion)
                    .Include(m => m.Activo)
                        .ThenInclude(a => a.EstadoActivo)
                    .Include(m => m.TipoMantenimiento)
                    .AsQueryable();


            // =====================================
            // CÓDIGO
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
            // DESCRIPCIÓN ACTIVO
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                descripcionActivoFiltro))
            {
                descripcionActivoFiltro =
                    descripcionActivoFiltro.Trim();

                consulta = consulta.Where(m =>
                    m.Activo.Descripcion
                        .Contains(
                            descripcionActivoFiltro));
            }


            // =====================================
            // UBICACIÓN
            // =====================================
            if (idUbicacionFiltro.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.Activo.IdUbicacion ==
                    idUbicacionFiltro.Value);
            }


            // =====================================
            // ESTADO
            // =====================================
            if (idEstadoFiltro.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.Activo.IdEstado ==
                    idEstadoFiltro.Value);
            }


            // =====================================
            // TIPO MANTENIMIENTO
            // =====================================
            if (idTipoFiltro.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.IdTipo ==
                    idTipoFiltro.Value);
            }


            // =====================================
            // RESPONSABLE
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
            // DESCRIPCIÓN MANTENIMIENTO
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                descripcionMantenimientoFiltro))
            {
                descripcionMantenimientoFiltro =
                    descripcionMantenimientoFiltro
                        .Trim();

                consulta = consulta.Where(m =>
                    m.Descripcion != null &&
                    m.Descripcion.Contains(
                        descripcionMantenimientoFiltro));
            }


            // =====================================
            // TOTAL RESULTADOS
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
            // SOLO 10 REGISTROS
            // =====================================
            var mantenimientos =
                await consulta
                    .OrderByDescending(m =>
                        m.FechaMantenimiento)
                    .ThenByDescending(m =>
                        m.IdMantenimiento)
                    .Skip(
                        (pagina - 1) *
                        tamanoPagina)
                    .Take(tamanoPagina)
                    .ToListAsync();


            // =====================================
            // DASHBOARD
            // =====================================
            int totalMantenimientos =
                await _context.Mantenimientos
                    .CountAsync();


            int activosConMantenimiento =
                await _context.Mantenimientos
                    .Select(m => m.IdActivo)
                    .Distinct()
                    .CountAsync();


            DateTime inicioMes =
                new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);


            DateTime inicioMesSiguiente =
                inicioMes.AddMonths(1);


            int mantenimientosEsteMes =
                await _context.Mantenimientos
                    .CountAsync(m =>
                        m.FechaMantenimiento >=
                        inicioMes &&
                        m.FechaMantenimiento <
                        inicioMesSiguiente);


            int activosDanados =
                await _context.Activos
                    .CountAsync(a =>
                        a.EstadoActivo != null &&
                        a.EstadoActivo.Estado ==
                        "Dañado");


            int activosEnMantenimiento =
                await _context.Activos
                    .CountAsync(a =>
                        a.EstadoActivo != null &&
                        a.EstadoActivo.Estado ==
                        "En mantenimiento");


            // =====================================
            // CATÁLOGOS
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


            var tipos =
                await _context.TiposMantenimiento
                    .AsNoTracking()
                    .OrderBy(t =>
                        t.Mantenimiento)
                    .ToListAsync();


            // =====================================
            // VIEWMODEL
            // =====================================
            var viewModel =
                new MantenimientoIndexViewModel
                {
                    Mantenimientos =
                        mantenimientos,

                    CodigoFiltro =
                        codigoFiltro,

                    DescripcionActivoFiltro =
                        descripcionActivoFiltro,

                    IdUbicacionFiltro =
                        idUbicacionFiltro,

                    IdEstadoFiltro =
                        idEstadoFiltro,

                    IdTipoFiltro =
                        idTipoFiltro,

                    ResponsableFiltro =
                        responsableFiltro,

                    DescripcionMantenimientoFiltro =
                        descripcionMantenimientoFiltro,

                    Ubicaciones =
                        ubicaciones,

                    Estados =
                        estados,

                    TiposMantenimiento =
                        tipos,

                    TotalMantenimientos =
                        totalMantenimientos,

                    ActivosConMantenimiento =
                        activosConMantenimiento,

                    MantenimientosEsteMes =
                        mantenimientosEsteMes,

                    ActivosDanados =
                        activosDanados,

                    ActivosEnMantenimiento =
                        activosEnMantenimiento,

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
        // CREATE GET
        // =========================================
        [HttpGet]
        [Authorize(Roles = "Administrador,Secretario")]
        public async Task<IActionResult> Create()
        {
            await CargarListasMantenimiento();


            var model =
                new MantenimientoCreateViewModel
                {
                    FechaMantenimiento =
                        DateTime.Today
                };


            return View(model);
        }


        // =========================================
        // CREATE POST
        // =========================================
        [HttpPost]
        [Authorize(Roles = "Administrador,Secretario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MantenimientoCreateViewModel model)
        {
            // =====================================
            // VALIDACIÓN DEL MODELO
            // =====================================
            if (!ModelState.IsValid)
            {
                await CargarListasMantenimiento(
                    model.IdActivo,
                    model.IdTipo);

                return View(model);
            }


            // =====================================
            // VALIDAR ACTIVO
            // =====================================
            bool existeActivo =
                await _context.Activos
                    .AnyAsync(a =>
                        a.IdActivo ==
                        model.IdActivo);


            if (!existeActivo)
            {
                ModelState.AddModelError(
                    nameof(model.IdActivo),
                    "El activo seleccionado no existe.");
            }


            // =====================================
            // VALIDAR TIPO
            // =====================================
            bool existeTipo =
                await _context.TiposMantenimiento
                    .AnyAsync(t =>
                        t.IdTipo ==
                        model.IdTipo);


            if (!existeTipo)
            {
                ModelState.AddModelError(
                    nameof(model.IdTipo),
                    "El tipo de mantenimiento seleccionado no existe.");
            }


            // =====================================
            // SI HAY ERRORES, RECARGAR LISTAS
            // =====================================
            if (!ModelState.IsValid)
            {
                await CargarListasMantenimiento(
                    model.IdActivo,
                    model.IdTipo);

                return View(model);
            }


            // =====================================
            // CREAR MANTENIMIENTO
            // =====================================
            var mantenimiento =
                new Mantenimiento
                {
                    IdActivo =
                        model.IdActivo,

                    IdTipo =
                        model.IdTipo,

                    FechaMantenimiento =
                        model.FechaMantenimiento,

                    Descripcion =
                        model.Descripcion,

                    Responsable =
                        model.Responsable
                };


            _context.Mantenimientos
                .Add(mantenimiento);


            await _context
                .SaveChangesAsync();


            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // CARGAR LISTAS DEL CREATE
        // =========================================
        private async Task CargarListasMantenimiento(
            int? idActivo = null,
            int? idTipo = null)
        {
            // =====================================
            // ACTIVOS
            // =====================================
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


            // =====================================
            // TIPOS DE MANTENIMIENTO
            // =====================================
            var tipos =
                await _context.TiposMantenimiento
                    .AsNoTracking()
                    .OrderBy(t =>
                        t.Mantenimiento)
                    .ToListAsync();


            ViewBag.TiposMantenimiento =
                new SelectList(
                    tipos,
                    "IdTipo",
                    "Mantenimiento",
                    idTipo);
        }
    }
}