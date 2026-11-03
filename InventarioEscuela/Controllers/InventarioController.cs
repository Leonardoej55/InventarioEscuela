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
    public class InventarioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventarioController> _logger;
        private readonly RelojSistema _reloj;

        private const string MensajeRegistroDuplicado =
            "Ya existe un activo con la misma combinación de tomo, folio y asiento.";

        public InventarioController(
            ApplicationDbContext context,
            ILogger<InventarioController> logger,
            RelojSistema reloj)
        {
            _context = context;
            _logger = logger;
            _reloj = reloj;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            string? serieFiltro,
            string? tomoFiltro,
            string? folioFiltro,
            string? asientoFiltro,
            int? idUbicacionFiltro,
            int? idEstadoFiltro,
            int? idModoFiltro,
            DateTime? fechaFiltro,
            int pagina = 1)
        {
            const int tamanoPagina = 10;

            var consulta = _context.Activos
                .AsNoTracking()
                .Include(a => a.Marca)
                .Include(a => a.Ubicacion)
                .Include(a => a.EstadoActivo)
                .Include(a => a.ModoAdquisicion)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(codigoFiltro))
            {
                codigoFiltro = codigoFiltro.Trim();
                consulta = consulta.Where(a =>
                    a.NumeroIdentificacion.Contains(codigoFiltro));
            }

            if (!string.IsNullOrWhiteSpace(descripcionFiltro))
            {
                descripcionFiltro = descripcionFiltro.Trim();
                consulta = consulta.Where(a =>
                    a.Descripcion.Contains(descripcionFiltro));
            }

            if (idMarcaFiltro.HasValue)
                consulta = consulta.Where(a => a.IdMarca == idMarcaFiltro.Value);

            if (!string.IsNullOrWhiteSpace(serieFiltro))
            {
                serieFiltro = serieFiltro.Trim();
                consulta = consulta.Where(a =>
                    a.Serie != null && a.Serie.Contains(serieFiltro));
            }

            tomoFiltro = tomoFiltro?.Trim();
            folioFiltro = folioFiltro?.Trim();
            asientoFiltro = asientoFiltro?.Trim();

            if (!string.IsNullOrWhiteSpace(tomoFiltro))
                consulta = consulta.Where(a =>
                    a.Tomo != null && a.Tomo.Contains(tomoFiltro));

            if (!string.IsNullOrWhiteSpace(folioFiltro))
                consulta = consulta.Where(a =>
                    a.Folio != null && a.Folio.Contains(folioFiltro));

            if (!string.IsNullOrWhiteSpace(asientoFiltro))
                consulta = consulta.Where(a =>
                    a.Asiento != null && a.Asiento.Contains(asientoFiltro));

            if (idUbicacionFiltro.HasValue)
                consulta = consulta.Where(a =>
                    a.IdUbicacion == idUbicacionFiltro.Value);

            if (idEstadoFiltro.HasValue)
                consulta = consulta.Where(a => a.IdEstado == idEstadoFiltro.Value);

            if (idModoFiltro.HasValue)
                consulta = consulta.Where(a => a.IdModo == idModoFiltro.Value);

            if (fechaFiltro.HasValue)
            {
                DateTime inicio = fechaFiltro.Value.Date;
                DateTime fin = inicio.AddDays(1);
                consulta = consulta.Where(a =>
                    a.FechaRegistro >= inicio && a.FechaRegistro < fin);
            }

            int totalResultados = await consulta.CountAsync();
            int totalPaginas = (int)Math.Ceiling(
                totalResultados / (double)tamanoPagina);

            if (pagina < 1)
                pagina = 1;

            if (totalPaginas > 0 && pagina > totalPaginas)
                pagina = totalPaginas;

            var activos = await consulta
                .OrderBy(a => a.NumeroIdentificacion)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();

            int totalActivos = await _context.Activos.CountAsync();

            int totalFuncionales = await _context.Activos.CountAsync(a =>
                a.EstadoActivo != null &&
                a.EstadoActivo.Estado == "Funcional");

            int totalDanados = await _context.Activos.CountAsync(a =>
                a.EstadoActivo != null &&
                a.EstadoActivo.Estado == "Dañado");

            int totalMantenimiento = await _context.Activos.CountAsync(a =>
                a.EstadoActivo != null &&
                a.EstadoActivo.Estado == "En mantenimiento");

            int totalUbicacionesConActivos = await _context.Activos
                .Select(a => a.IdUbicacion)
                .Distinct()
                .CountAsync();

            var marcas = await _context.Marcas
                .AsNoTracking()
                .OrderBy(m => m.MarcaNombre)
                .ToListAsync();

            var ubicaciones = await _context.Ubicaciones
                .AsNoTracking()
                .OrderBy(u => u.UbicacionNombre)
                .ToListAsync();

            var estados = await _context.EstadosActivos
                .AsNoTracking()
                .OrderBy(e => e.Estado)
                .ToListAsync();

            var modosAdquisicion = await _context.ModosAdquisicion
                .AsNoTracking()
                .OrderBy(m => m.NombreModoAdquisicion)
                .ToListAsync();

            return View(new InventarioIndexViewModel
            {
                Activos = activos,
                CodigoFiltro = codigoFiltro,
                DescripcionFiltro = descripcionFiltro,
                IdMarcaFiltro = idMarcaFiltro,
                SerieFiltro = serieFiltro,
                TomoFiltro = tomoFiltro,
                FolioFiltro = folioFiltro,
                AsientoFiltro = asientoFiltro,
                IdUbicacionFiltro = idUbicacionFiltro,
                IdEstadoFiltro = idEstadoFiltro,
                IdModoFiltro = idModoFiltro,
                FechaFiltro = fechaFiltro,
                Marcas = marcas,
                Ubicaciones = ubicaciones,
                Estados = estados,
                ModosAdquisicion = modosAdquisicion,
                TotalActivos = totalActivos,
                TotalFuncionales = totalFuncionales,
                TotalDanados = totalDanados,
                TotalMantenimiento = totalMantenimiento,
                TotalUbicacionesConActivos = totalUbicacionesConActivos,
                PaginaActual = pagina,
                TamanoPagina = tamanoPagina,
                TotalResultados = totalResultados,
                TotalPaginas = totalPaginas
            });
        }

        [HttpGet]
        public async Task<IActionResult> Details(
            int id,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] =
                !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? returnUrl : null;

            var activo = await _context.Activos
                .AsNoTracking()
                .Include(a => a.Marca)
                .Include(a => a.EstadoActivo)
                .Include(a => a.Ubicacion)
                .Include(a => a.ModoAdquisicion)
                .FirstOrDefaultAsync(a => a.IdActivo == id);

            if (activo == null)
                return NotFound();

            return View(activo);
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Secretario")]
        public async Task<IActionResult> Create()
        {
            await CargarListas();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Secretario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Activo activo)
        {
            activo.NumeroIdentificacion =
                activo.NumeroIdentificacion?.Trim() ?? string.Empty;

            activo.Tomo = NormalizarDatoRegistral(activo.Tomo);
            activo.Folio = NormalizarDatoRegistral(activo.Folio);
            activo.Asiento = NormalizarDatoRegistral(activo.Asiento);

            if (!ModelState.IsValid)
            {
                await CargarListas(activo);
                return View(activo);
            }

            try
            {
                if (await _context.Activos.AnyAsync(a =>
                    a.NumeroIdentificacion.Trim() == activo.NumeroIdentificacion))
                {
                    ModelState.AddModelError(
                        nameof(activo.NumeroIdentificacion),
                        "Ya existe un activo con este número de identificación.");
                }

                await ValidarRegistroUnico(
                    activo.Tomo, activo.Folio, activo.Asiento);

                await ValidarReferencias(
                    activo.IdMarca, activo.IdEstado,
                    activo.IdModo, activo.IdUbicacion);

                if (!ModelState.IsValid)
                {
                    await CargarListas(activo);
                    return View(activo);
                }

                var nuevoActivo = new Activo
                {
                    NumeroIdentificacion = activo.NumeroIdentificacion,
                    Descripcion = activo.Descripcion,
                    Tomo = activo.Tomo,
                    Folio = activo.Folio,
                    Asiento = activo.Asiento,
                    IdMarca = activo.IdMarca,
                    Modelo = activo.Modelo,
                    Serie = activo.Serie,
                    IdEstado = activo.IdEstado,
                    IdUbicacion = activo.IdUbicacion,
                    IdModo = activo.IdModo,
                    Observacion = activo.Observacion,
                    FechaRegistro = _reloj.FechaHora
                };

                _context.Activos.Add(nuevoActivo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
                when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
            {
                _logger.LogError(
                    ex, "Error de base de datos al registrar un activo.");

                _context.ChangeTracker.Clear();
                AgregarErrorGuardadoActivo(ex);
                await CargarListas(activo);
                return View(activo);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Secretario")]
        public async Task<IActionResult> Edit(
            int id,
            string? returnUrl = null)
        {
            var retorno =
                !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? returnUrl : null;

            ViewData["ReturnUrl"] = retorno;

            try
            {
                var activo = await _context.Activos
                    .AsNoTracking()
                    .Include(a => a.Ubicacion)
                    .FirstOrDefaultAsync(a => a.IdActivo == id);

                if (activo == null)
                    return NotFound();

                var model = new ActivoEditViewModel
                {
                    IdActivo = activo.IdActivo,
                    NumeroIdentificacion = activo.NumeroIdentificacion,
                    Tomo = activo.Tomo,
                    Folio = activo.Folio,
                    Asiento = activo.Asiento,
                    Descripcion = activo.Descripcion,
                    IdMarca = activo.IdMarca,
                    Modelo = activo.Modelo,
                    Serie = activo.Serie,
                    IdEstado = activo.IdEstado,
                    IdModo = activo.IdModo,
                    Observacion = activo.Observacion
                };

                await PrepararEdicion(model, activo);
                return View(model);
            }
            catch (Exception ex)
                when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
            {
                _logger.LogError(
                    ex,
                    "Error de base de datos al consultar un activo para edición.");

                Response.StatusCode = StatusCodes.Status503ServiceUnavailable;

                return View("ErrorOperacion", new ErrorOperacionViewModel
                {
                    Titulo = "No se pudo cargar el activo",
                    Mensaje =
                        "No se pudo consultar la información en este momento. " +
                        "Intente cargarla nuevamente.",
                    UrlReintento = Url.Action(
                        nameof(Edit), "Inventario",
                        new { id, returnUrl = retorno }) ?? string.Empty
                });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Secretario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ActivoEditViewModel model,
            string? returnUrl = null)
        {
            var retorno =
                !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                    ? returnUrl : null;

            ViewData["ReturnUrl"] = retorno;

            if (id != model.IdActivo)
                return NotFound();

            model.NumeroIdentificacion =
                model.NumeroIdentificacion?.Trim() ?? string.Empty;

            model.Tomo = NormalizarDatoRegistral(model.Tomo);
            model.Folio = NormalizarDatoRegistral(model.Folio);
            model.Asiento = NormalizarDatoRegistral(model.Asiento);

            try
            {
                var activoOriginal = await _context.Activos
                    .Include(a => a.Ubicacion)
                    .FirstOrDefaultAsync(a => a.IdActivo == id);

                if (activoOriginal == null)
                    return NotFound();

                model.UbicacionActual =
                    activoOriginal.Ubicacion?.UbicacionNombre ?? "Sin ubicación";

                model.FechaRegistro = activoOriginal.FechaRegistro;

                if (await _context.Activos.AnyAsync(a =>
                    a.NumeroIdentificacion.Trim() == model.NumeroIdentificacion &&
                    a.IdActivo != id))
                {
                    ModelState.AddModelError(
                        nameof(model.NumeroIdentificacion),
                        "Ya existe otro activo con este número de identificación.");
                }

                await ValidarRegistroUnico(
                    model.Tomo, model.Folio, model.Asiento, id);

                await ValidarReferencias(
                    model.IdMarca, model.IdEstado, model.IdModo);

                if (!ModelState.IsValid)
                {
                    await PrepararEdicion(model, activoOriginal);
                    return View(model);
                }

                activoOriginal.NumeroIdentificacion = model.NumeroIdentificacion;
                activoOriginal.Tomo = model.Tomo;
                activoOriginal.Folio = model.Folio;
                activoOriginal.Asiento = model.Asiento;
                activoOriginal.Descripcion = model.Descripcion;
                activoOriginal.IdMarca = model.IdMarca;
                activoOriginal.Modelo = model.Modelo;
                activoOriginal.Serie = model.Serie;
                activoOriginal.IdEstado = model.IdEstado;
                activoOriginal.IdModo = model.IdModo;
                activoOriginal.Observacion = model.Observacion;

                await _context.SaveChangesAsync();

                if (!string.IsNullOrEmpty(retorno) && Url.IsLocalUrl(retorno))
                    return LocalRedirect(retorno);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
                when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
            {
                _logger.LogError(
                    ex, "Error de base de datos al editar un activo.");

                _context.ChangeTracker.Clear();
                AgregarErrorGuardadoActivo(ex);

                await CargarListas(new Activo
                {
                    IdMarca = model.IdMarca,
                    IdEstado = model.IdEstado,
                    IdModo = model.IdModo
                });

                return View(model);
            }
        }

        private static string? NormalizarDatoRegistral(string? valor) =>
            string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

        private async Task ValidarRegistroUnico(
            string? tomo,
            string? folio,
            string? asiento,
            int idExcluir = 0)
        {
            if (tomo == null || folio == null || asiento == null)
                return;

            bool existe = await _context.Activos.AnyAsync(a =>
                a.IdActivo != idExcluir &&
                a.Tomo != null && a.Tomo.Trim() == tomo &&
                a.Folio != null && a.Folio.Trim() == folio &&
                a.Asiento != null && a.Asiento.Trim() == asiento);

            if (existe)
                ModelState.AddModelError(string.Empty, MensajeRegistroDuplicado);
        }

        private static bool EsDuplicadoRegistro(Exception error)
        {
            for (Exception? causa = error;
                 causa != null;
                 causa = causa.InnerException)
            {
                if (causa.Message.Contains(
                    "UX_Activo_Tomo_Folio_Asiento",
                    StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private void AgregarErrorGuardadoActivo(Exception error)
        {
            if (ErroresBaseDeDatos.EsDuplicado(error))
            {
                if (EsDuplicadoRegistro(error))
                    ModelState.AddModelError(
                        string.Empty, MensajeRegistroDuplicado);
                else
                    ModelState.AddModelError(
                        nameof(Activo.NumeroIdentificacion),
                        "Ya existe un activo con este número de identificación.");
            }
            else
            {
                ModelState.AddModelError(
                    string.Empty, ErroresBaseDeDatos.MensajeGuardado(error));
            }
        }

        private async Task ValidarReferencias(
            int idMarca,
            int idEstado,
            int idModo,
            int? idUbicacion = null)
        {
            if (idMarca <= 0 ||
                !await _context.Marcas.AnyAsync(m => m.IdMarca == idMarca))
                ModelState.AddModelError(
                    nameof(Activo.IdMarca), "La marca seleccionada no existe.");

            if (idEstado <= 0 ||
                !await _context.EstadosActivos.AnyAsync(e =>
                    e.IdEstado == idEstado))
                ModelState.AddModelError(
                    nameof(Activo.IdEstado), "El estado seleccionado no existe.");

            if (idModo <= 0 ||
                !await _context.ModosAdquisicion.AnyAsync(m =>
                    m.IdModoAdquisicion == idModo))
                ModelState.AddModelError(
                    nameof(Activo.IdModo),
                    "El modo de adquisición seleccionado no existe.");

            if (idUbicacion.HasValue &&
                (idUbicacion <= 0 ||
                 !await _context.Ubicaciones.AnyAsync(u =>
                     u.IdUbicacion == idUbicacion.Value)))
                ModelState.AddModelError(
                    nameof(Activo.IdUbicacion),
                    "La ubicación seleccionada no existe.");
        }

        private async Task PrepararEdicion(
            ActivoEditViewModel model,
            Activo activo)
        {
            model.UbicacionActual =
                activo.Ubicacion?.UbicacionNombre ?? "Sin ubicación";

            model.FechaRegistro = activo.FechaRegistro;

            await CargarListas(new Activo
            {
                IdMarca = model.IdMarca,
                IdEstado = model.IdEstado,
                IdModo = model.IdModo
            });
        }

        private async Task CargarListas(Activo? activo = null)
        {
            try
            {
                ViewBag.Marcas = new SelectList(
                    await _context.Marcas.AsNoTracking()
                        .OrderBy(m => m.MarcaNombre).ToListAsync(),
                    "IdMarca", "MarcaNombre", activo?.IdMarca);

                ViewBag.Estados = new SelectList(
                    await _context.EstadosActivos.AsNoTracking()
                        .OrderBy(e => e.Estado).ToListAsync(),
                    "IdEstado", "Estado", activo?.IdEstado);

                ViewBag.Ubicaciones = new SelectList(
                    await _context.Ubicaciones.AsNoTracking()
                        .OrderBy(u => u.UbicacionNombre).ToListAsync(),
                    "IdUbicacion", "UbicacionNombre", activo?.IdUbicacion);

                ViewBag.ModosAdquisicion = new SelectList(
                    await _context.ModosAdquisicion.AsNoTracking()
                        .OrderBy(m => m.NombreModoAdquisicion).ToListAsync(),
                    "IdModoAdquisicion", "NombreModoAdquisicion", activo?.IdModo);
            }
            catch (Exception ex)
                when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
            {
                _logger.LogError(
                    ex, "Error al recargar las opciones de inventario.");

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudieron cargar todas las opciones. " +
                    "Intente cargar el formulario nuevamente.");

                ViewData["Marcas"] ??= OpcionesAnteriores(activo?.IdMarca);
                ViewData["Estados"] ??= OpcionesAnteriores(activo?.IdEstado);
                ViewData["Ubicaciones"] ??= OpcionesAnteriores(activo?.IdUbicacion);
                ViewData["ModosAdquisicion"] ??= OpcionesAnteriores(activo?.IdModo);
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