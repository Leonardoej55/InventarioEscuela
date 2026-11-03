using InventarioEscuela.Data;
using InventarioEscuela.Models;
using InventarioEscuela.Services;
using InventarioEscuela.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InventarioEscuela.Controllers
{
    [Authorize(Roles = "Administrador,Secretario,Usuario")]
    public class PerfilController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;
        private readonly ILogger<PerfilController> _logger;

        public PerfilController(
            ApplicationDbContext context,
            IPasswordHasher<Usuario> passwordHasher,
            ILogger<PerfilController> logger)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return MostrarPerfil(new CambiarPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(
            CambiarPasswordViewModel model)
        {
            if (!int.TryParse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier),
                    out int idUsuario) ||
                idUsuario <= 0)
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);

                return RedirectToAction("IniciarSesion", "Login");
            }

            if (!ModelState.IsValid)
            {
                return MostrarErrores(model);
            }

            try
            {
                var usuario = await _context.Usuarios
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

                if (usuario == null)
                {
                    await HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme);

                    return RedirectToAction("IniciarSesion", "Login");
                }

                var resultado = PasswordVerificationResult.Failed;

                if (!string.IsNullOrEmpty(usuario.PasswordHash))
                {
                    try
                    {
                        resultado = _passwordHasher.VerifyHashedPassword(
                            usuario,
                            usuario.PasswordHash,
                            model.PasswordActual);
                    }
                    catch (FormatException)
                    {
                        resultado = PasswordVerificationResult.Failed;
                    }
                }

                if (resultado == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError(
                        nameof(model.PasswordActual),
                        "La contraseña actual no es correcta.");

                    return MostrarErrores(model);
                }

                if (string.Equals(
                    model.PasswordActual,
                    model.NuevoPassword,
                    StringComparison.Ordinal))
                {
                    ModelState.AddModelError(
                        nameof(model.NuevoPassword),
                        "La nueva contraseña debe ser diferente de la actual.");

                    return MostrarErrores(model);
                }

                string hashAnterior = usuario.PasswordHash;

                string nuevoHash = _passwordHasher.HashPassword(
                    usuario,
                    model.NuevoPassword);

                // Actualizar únicamente si la contraseña no cambió entretanto.
                int filasActualizadas = await _context.Usuarios
                    .Where(u =>
                        u.IdUsuario == idUsuario &&
                        EF.Functions.Collate(
                            u.PasswordHash,
                            "Latin1_General_100_BIN2") == hashAnterior)
                    .ExecuteUpdateAsync(setters =>
                        setters.SetProperty(
                            u => u.PasswordHash,
                            nuevoHash));

                if (filasActualizadas != 1)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "La cuenta o la contraseña cambiaron mientras se guardaba. " +
                        "Vuelva a intentarlo con la contraseña vigente.");

                    return MostrarErrores(model);
                }

                TempData["PasswordActualizado"] =
                    "Su contraseña se actualizó correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
                when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
            {
                _logger.LogError(
                    ex,
                    "Error de base de datos al cambiar una contraseña.");

                ModelState.AddModelError(
                    string.Empty,
                    "No se pudo confirmar el cambio de contraseña. " +
                    "Compruebe con cuál puede iniciar sesión antes de " +
                    "intentarlo nuevamente.");

                return MostrarErrores(model);
            }
        }

        private ViewResult MostrarPerfil(CambiarPasswordViewModel model)
        {
            ViewData["NombreUsuario"] =
                User.FindFirst(ClaimTypes.Name)?.Value ?? "Usuario";

            return View("Index", model);
        }

        private ViewResult MostrarErrores(CambiarPasswordViewModel model)
        {
            model.PasswordActual = string.Empty;
            model.NuevoPassword = string.Empty;
            model.ConfirmarPassword = string.Empty;

            foreach (string campo in new[]
            {
                nameof(model.PasswordActual),
                nameof(model.NuevoPassword),
                nameof(model.ConfirmarPassword)
            })
            {
                ModelState.SetModelValue(campo, null, string.Empty);
            }

            return MostrarPerfil(model);
        }
    }
}