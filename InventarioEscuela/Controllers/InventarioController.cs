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
    public class InventarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventarioController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // INVENTARIO
        // FILTROS + PAGINACIÓN
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            string? serieFiltro,
            int? idUbicacionFiltro,
            int? idEstadoFiltro,
            int? idModoFiltro,
            DateTime? fechaFiltro,
            int pagina = 1)
        {
            const int tamanoPagina = 10;


            // =====================================
            // CONSULTA BASE
            // =====================================
            var consulta = _context.Activos
                .AsNoTracking()
                .Include(a => a.Marca)
                .Include(a => a.Ubicacion)
                .Include(a => a.EstadoActivo)
                .Include(a => a.ModoAdquisicion)
                .AsQueryable();


            // =====================================
            // FILTRO POR CÓDIGO
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                codigoFiltro))
            {
                codigoFiltro =
                    codigoFiltro.Trim();

                consulta = consulta.Where(a =>
                    a.NumeroIdentificacion
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

                consulta = consulta.Where(a =>
                    a.Descripcion
                        .Contains(descripcionFiltro));
            }


            // =====================================
            // FILTRO POR MARCA
            // =====================================
            if (idMarcaFiltro.HasValue)
            {
                consulta = consulta.Where(a =>
                    a.IdMarca ==
                    idMarcaFiltro.Value);
            }


            // =====================================
            // FILTRO POR SERIE
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                serieFiltro))
            {
                serieFiltro =
                    serieFiltro.Trim();

                consulta = consulta.Where(a =>
                    a.Serie != null &&
                    a.Serie.Contains(
                        serieFiltro));
            }


            // =====================================
            // FILTRO POR UBICACIÓN
            // =====================================
            if (idUbicacionFiltro.HasValue)
            {
                consulta = consulta.Where(a =>
                    a.IdUbicacion ==
                    idUbicacionFiltro.Value);
            }


            // =====================================
            // FILTRO POR ESTADO
            // =====================================
            if (idEstadoFiltro.HasValue)
            {
                consulta = consulta.Where(a =>
                    a.IdEstado ==
                    idEstadoFiltro.Value);
            }


            // =====================================
            // FILTRO POR MODO DE ADQUISICIÓN
            // =====================================
            if (idModoFiltro.HasValue)
            {
                consulta = consulta.Where(a =>
                    a.IdModo ==
                    idModoFiltro.Value);
            }


            // =====================================
            // FILTRO POR FECHA
            // =====================================
            if (fechaFiltro.HasValue)
            {
                DateTime inicio =
                    fechaFiltro.Value.Date;

                DateTime fin =
                    inicio.AddDays(1);

                consulta = consulta.Where(a =>
                    a.FechaRegistro >= inicio &&
                    a.FechaRegistro < fin);
            }


            // =====================================
            // TOTAL RESULTADOS FILTRADOS
            // =====================================
            int totalResultados =
                await consulta.CountAsync();


            // =====================================
            // TOTAL DE PÁGINAS
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
            // SOLO 10 ACTIVOS
            // =====================================
            var activos =
                await consulta
                    .OrderBy(a =>
                        a.NumeroIdentificacion)
                    .Skip(
                        (pagina - 1) *
                        tamanoPagina)
                    .Take(tamanoPagina)
                    .ToListAsync();


            // =====================================
            // RESUMEN GENERAL
            // =====================================
            int totalActivos =
                await _context.Activos
                    .CountAsync();


            int totalFuncionales =
                await _context.Activos
                    .CountAsync(a =>
                        a.EstadoActivo != null &&
                        a.EstadoActivo.Estado ==
                        "Funcional");


            int totalDanados =
                await _context.Activos
                    .CountAsync(a =>
                        a.EstadoActivo != null &&
                        a.EstadoActivo.Estado ==
                        "Dañado");


            int totalMantenimiento =
                await _context.Activos
                    .CountAsync(a =>
                        a.EstadoActivo != null &&
                        a.EstadoActivo.Estado ==
                        "En mantenimiento");


            int totalUbicacionesConActivos =
                await _context.Activos
                    .Select(a =>
                        a.IdUbicacion)
                    .Distinct()
                    .CountAsync();


            // =====================================
            // LISTAS DE FILTROS
            // =====================================
            var marcas =
                await _context.Marcas
                    .AsNoTracking()
                    .OrderBy(m =>
                        m.MarcaNombre)
                    .ToListAsync();


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


            var modosAdquisicion =
                await _context.ModosAdquisicion
                    .AsNoTracking()
                    .OrderBy(m =>
                        m.NombreModoAdquisicion)
                    .ToListAsync();


            // =====================================
            // VIEWMODEL
            // =====================================
            var viewModel =
                new InventarioIndexViewModel
                {
                    Activos = activos,

                    CodigoFiltro =
                        codigoFiltro,

                    DescripcionFiltro =
                        descripcionFiltro,

                    IdMarcaFiltro =
                        idMarcaFiltro,

                    SerieFiltro =
                        serieFiltro,

                    IdUbicacionFiltro =
                        idUbicacionFiltro,

                    IdEstadoFiltro =
                        idEstadoFiltro,

                    IdModoFiltro =
                        idModoFiltro,

                    FechaFiltro =
                        fechaFiltro,

                    Marcas =
                        marcas,

                    Ubicaciones =
                        ubicaciones,

                    Estados =
                        estados,

                    ModosAdquisicion =
                        modosAdquisicion,

                    TotalActivos =
                        totalActivos,

                    TotalFuncionales =
                        totalFuncionales,

                    TotalDanados =
                        totalDanados,

                    TotalMantenimiento =
                        totalMantenimiento,

                    TotalUbicacionesConActivos =
                        totalUbicacionesConActivos,

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
        // DETALLES
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            var activo =
                await _context.Activos
                    .AsNoTracking()
                    .Include(a => a.Marca)
                    .Include(a => a.EstadoActivo)
                    .Include(a => a.Ubicacion)
                    .Include(a => a.ModoAdquisicion)
                    .FirstOrDefaultAsync(a =>
                        a.IdActivo == id);


            if (activo == null)
            {
                return NotFound();
            }


            return View(activo);
        }


        // =========================================
        // CREAR
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarListas();

            return View();
        }


        // =========================================
        // CREAR POST
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Activo activo)
        {
            // =====================================
            // VALIDAR CÓDIGO DUPLICADO
            // =====================================
            bool existeCodigo =
                await _context.Activos
                    .AnyAsync(a =>
                        a.NumeroIdentificacion ==
                        activo.NumeroIdentificacion);


            if (existeCodigo)
            {
                ModelState.AddModelError(
                    nameof(activo.NumeroIdentificacion),
                    "Ya existe un activo con este número de identificación.");
            }


            if (!ModelState.IsValid)
            {
                await CargarListas(activo);

                return View(activo);
            }


            activo.FechaRegistro =
                DateTime.Now;


            _context.Activos.Add(activo);

            await _context.SaveChangesAsync();


            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // EDITAR
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Edit(
            int id)
        {
            var activo =
                await _context.Activos
                    .FindAsync(id);


            if (activo == null)
            {
                return NotFound();
            }


            await CargarListas(activo);

            return View(activo);
        }


        // =========================================
        // EDITAR POST
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Activo activo)
        {
            if (id != activo.IdActivo)
            {
                return NotFound();
            }


            bool codigoDuplicado =
                await _context.Activos
                    .AnyAsync(a =>
                        a.NumeroIdentificacion ==
                        activo.NumeroIdentificacion &&
                        a.IdActivo !=
                        activo.IdActivo);


            if (codigoDuplicado)
            {
                ModelState.AddModelError(
                    nameof(activo.NumeroIdentificacion),
                    "Ya existe otro activo con este número de identificación.");
            }


            if (!ModelState.IsValid)
            {
                await CargarListas(activo);

                return View(activo);
            }


            var activoOriginal =
                await _context.Activos
                    .FirstOrDefaultAsync(a =>
                        a.IdActivo == id);


            if (activoOriginal == null)
            {
                return NotFound();
            }


            // =====================================
            // ACTUALIZAR DATOS
            // =====================================
            activoOriginal.NumeroIdentificacion =
                activo.NumeroIdentificacion;

            activoOriginal.Tomo =
                activo.Tomo;

            activoOriginal.Folio =
                activo.Folio;

            activoOriginal.Asiento =
                activo.Asiento;

            activoOriginal.Descripcion =
                activo.Descripcion;

            activoOriginal.IdMarca =
                activo.IdMarca;

            activoOriginal.Modelo =
                activo.Modelo;

            activoOriginal.Serie =
                activo.Serie;

            activoOriginal.IdEstado =
                activo.IdEstado;

            activoOriginal.IdUbicacion =
                activo.IdUbicacion;

            activoOriginal.IdModo =
                activo.IdModo;

            activoOriginal.Observacion =
                activo.Observacion;


            // FechaRegistro se conserva


            await _context.SaveChangesAsync();


            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // CARGAR SELECTS
        // =========================================
        private async Task CargarListas(
            Activo? activo = null)
        {
            ViewBag.Marcas =
                new SelectList(
                    await _context.Marcas
                        .OrderBy(m =>
                            m.MarcaNombre)
                        .ToListAsync(),
                    "IdMarca",
                    "MarcaNombre",
                    activo?.IdMarca);


            ViewBag.Estados =
                new SelectList(
                    await _context.EstadosActivos
                        .OrderBy(e =>
                            e.Estado)
                        .ToListAsync(),
                    "IdEstado",
                    "Estado",
                    activo?.IdEstado);


            ViewBag.Ubicaciones =
                new SelectList(
                    await _context.Ubicaciones
                        .OrderBy(u =>
                            u.UbicacionNombre)
                        .ToListAsync(),
                    "IdUbicacion",
                    "UbicacionNombre",
                    activo?.IdUbicacion);


            ViewBag.ModosAdquisicion =
                new SelectList(
                    await _context.ModosAdquisicion
                        .OrderBy(m =>
                            m.NombreModoAdquisicion)
                        .ToListAsync(),
                    "IdModoAdquisicion",
                    "NombreModoAdquisicion",
                    activo?.IdModo);
        }
    }
}