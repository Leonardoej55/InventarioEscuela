using InventarioEscuela.Data;
using InventarioEscuela.Models;
using InventarioEscuela.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace InventarioEscuela.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ApplicationDbContext context,
            ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }


        // =========================================
        // DASHBOARD + FILTROS + PAGINACIÓN
        // =========================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            int? idUbicacionFiltro,
            int? idEstadoFiltro,
            DateTime? fechaFiltro,
            int pagina = 1)
        {
            const int tamanoPagina = 10;

            string nombreUsuario =
                User.FindFirst(ClaimTypes.Name)?.Value ?? "";


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
            if (!string.IsNullOrWhiteSpace(codigoFiltro))
            {
                codigoFiltro = codigoFiltro.Trim();

                consulta = consulta.Where(a =>
                    a.NumeroIdentificacion.Contains(codigoFiltro));
            }


            // =====================================
            // FILTRO POR DESCRIPCIÓN
            // =====================================
            if (!string.IsNullOrWhiteSpace(descripcionFiltro))
            {
                descripcionFiltro = descripcionFiltro.Trim();

                consulta = consulta.Where(a =>
                    a.Descripcion.Contains(descripcionFiltro));
            }


            // =====================================
            // FILTRO POR MARCA
            // =====================================
            if (idMarcaFiltro.HasValue)
            {
                consulta = consulta.Where(a =>
                    a.IdMarca == idMarcaFiltro.Value);
            }


            // =====================================
            // FILTRO POR UBICACIÓN
            // =====================================
            if (idUbicacionFiltro.HasValue)
            {
                consulta = consulta.Where(a =>
                    a.IdUbicacion == idUbicacionFiltro.Value);
            }


            // =====================================
            // FILTRO POR ESTADO
            // =====================================
            if (idEstadoFiltro.HasValue)
            {
                consulta = consulta.Where(a =>
                    a.IdEstado == idEstadoFiltro.Value);
            }


            // =====================================
            // FILTRO POR FECHA
            // =====================================
            if (fechaFiltro.HasValue)
            {
                DateTime fechaInicio =
                    fechaFiltro.Value.Date;

                DateTime fechaFin =
                    fechaInicio.AddDays(1);

                consulta = consulta.Where(a =>
                    a.FechaRegistro >= fechaInicio &&
                    a.FechaRegistro < fechaFin);
            }


            // =====================================
            // TOTAL DE RESULTADOS FILTRADOS
            // =====================================
            int totalResultados =
                await consulta.CountAsync();


            // =====================================
            // CALCULAR TOTAL DE PÁGINAS
            // =====================================
            int totalPaginas =
                (int)Math.Ceiling(
                    totalResultados / (double)tamanoPagina);


            // Evitar página 0 o negativa
            if (pagina < 1)
            {
                pagina = 1;
            }


            // Evitar pedir una página que no existe
            if (totalPaginas > 0 &&
                pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }


            // =====================================
            // OBTENER SOLAMENTE 20 REGISTROS
            // =====================================
            var activos = await consulta
                .OrderBy(a => a.NumeroIdentificacion)
                .Skip((pagina - 1) * tamanoPagina)
                .Take(tamanoPagina)
                .ToListAsync();


            // =====================================
            // DASHBOARD GENERAL
            // =====================================
            int totalActivos =
                await _context.Activos.CountAsync();


            int totalFuncionales =
                await _context.Activos.CountAsync(a =>
                    a.EstadoActivo != null &&
                    a.EstadoActivo.Estado == "Funcional");


            int totalDanados =
                await _context.Activos.CountAsync(a =>
                    a.EstadoActivo != null &&
                    a.EstadoActivo.Estado == "Dañado");


            int totalMantenimiento =
                await _context.Activos.CountAsync(a =>
                    a.EstadoActivo != null &&
                    a.EstadoActivo.Estado == "En mantenimiento");


            int totalUbicacionesConActivos =
                await _context.Activos
                    .Select(a => a.IdUbicacion)
                    .Distinct()
                    .CountAsync();


            // =====================================
            // LISTAS PARA FILTROS
            // =====================================
            var marcas =
                await _context.Marcas
                    .AsNoTracking()
                    .OrderBy(m => m.MarcaNombre)
                    .ToListAsync();


            var ubicaciones =
                await _context.Ubicaciones
                    .AsNoTracking()
                    .OrderBy(u => u.UbicacionNombre)
                    .ToListAsync();


            var estados =
                await _context.EstadosActivos
                    .AsNoTracking()
                    .OrderBy(e => e.Estado)
                    .ToListAsync();


            // =====================================
            // VIEWMODEL
            // =====================================
            var viewModel = new IndexViewModel
            {
                Activos = activos,

                NombreUsuario = nombreUsuario,

                TotalActivos = totalActivos,
                TotalFuncionales = totalFuncionales,
                TotalDanados = totalDanados,
                TotalMantenimiento = totalMantenimiento,
                TotalUbicacionesConActivos =
                    totalUbicacionesConActivos,

                CodigoFiltro = codigoFiltro,
                DescripcionFiltro = descripcionFiltro,
                IdMarcaFiltro = idMarcaFiltro,
                IdUbicacionFiltro = idUbicacionFiltro,
                IdEstadoFiltro = idEstadoFiltro,
                FechaFiltro = fechaFiltro,

                Marcas = marcas,
                Ubicaciones = ubicaciones,
                Estados = estados,

                PaginaActual = pagina,
                TamanoPagina = tamanoPagina,
                TotalResultados = totalResultados,
                TotalPaginas = totalPaginas
            };


            return View(viewModel);
        }


        // =========================================
        // ERROR
        // =========================================
        [AllowAnonymous]
        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId =
                    Activity.Current?.Id ??
                    HttpContext.TraceIdentifier
            });
        }


        // =========================================
        // CERRAR SESIÓN
        // =========================================
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(
                "IniciarSesion",
                "Login");
        }
    }
}