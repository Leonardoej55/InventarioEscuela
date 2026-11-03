using InventarioEscuela.Data;
using InventarioEscuela.Models;
using InventarioEscuela.Services.Reportes;
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
    [Authorize(Roles = "Administrador,Secretario,Usuario")]
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
                consulta = consulta.Where(a =>
                    a.IdMarca == idMarcaFiltro.Value);

            if (idUbicacionFiltro.HasValue)
                consulta = consulta.Where(a =>
                    a.IdUbicacion == idUbicacionFiltro.Value);

            if (idEstadoFiltro.HasValue)
                consulta = consulta.Where(a =>
                    a.IdEstado == idEstadoFiltro.Value);

            if (fechaFiltro.HasValue)
            {
                DateTime inicio = fechaFiltro.Value.Date;
                DateTime fin = inicio.AddDays(1);

                consulta = consulta.Where(a =>
                    a.FechaRegistro >= inicio &&
                    a.FechaRegistro < fin);
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

            int totalFuncionales = await _context.Activos
                .CountAsync(EstadosReporte.EsFuncional);

            int totalDanados = await _context.Activos
                .CountAsync(EstadosReporte.EsDanado);

            int totalMantenimiento = await _context.Activos
                .CountAsync(EstadosReporte.EsEnMantenimiento);

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

            return View(new IndexViewModel
            {
                Activos = activos,
                NombreUsuario = nombreUsuario,
                TotalActivos = totalActivos,
                TotalFuncionales = totalFuncionales,
                TotalDanados = totalDanados,
                TotalMantenimiento = totalMantenimiento,
                TotalUbicacionesConActivos = totalUbicacionesConActivos,
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
            });
        }

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
                    Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("IniciarSesion", "Login");
        }
    }
}