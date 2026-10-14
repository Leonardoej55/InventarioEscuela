using InventarioEscuela.Models;
using InventarioEscuela.Services;
using InventarioEscuela.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace InventarioEscuela.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly ILogger<LoginController> _logger;

        public LoginController(IUsuarioService usuarioService, ILogger<LoginController> logger)
        {
            _usuarioService = usuarioService;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public IActionResult Registro()
        {
            return View(new RegistroViewModel());
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return MostrarErroresRegistro(model);
            }

            model.Correo = model.Correo.Trim().ToLowerInvariant();
            try
            {
                if (await _usuarioService.ExisteCorreo(model.Correo))
                {
                    ModelState.AddModelError(nameof(model.Correo), "Ya existe una cuenta con ese correo.");
                    return MostrarErroresRegistro(model);
                }

                var usuario = new Usuario
                {
                    Nombre = model.Nombre.Trim(),
                    Correo = model.Correo,
                    Rol = model.Rol
                };
                await _usuarioService.SaveUsuario(usuario, model.Password);
            }
            catch (Exception ex) when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
            {
                _logger.LogError(ex, "Error de base de datos al registrar una cuenta.");
                if (ErroresBaseDeDatos.EsDuplicado(ex))
                {
                    ModelState.AddModelError(nameof(model.Correo), "Ya existe una cuenta con ese correo.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ErroresBaseDeDatos.MensajeGuardado(ex));
                }
                return MostrarErroresRegistro(model);
            }

            TempData["RegistroExitoso"] = "La cuenta se creó correctamente.";
            return RedirectToAction(nameof(Registro));
        }

        private ViewResult MostrarErroresRegistro(RegistroViewModel model)
        {
            model.Password = string.Empty;
            model.ConfirmarPassword = string.Empty;
            ModelState.SetModelValue(nameof(model.Password), null, string.Empty);
            ModelState.SetModelValue(nameof(model.ConfirmarPassword), null, string.Empty);
            return View("Registro", model);
        }

        [HttpGet]
        public IActionResult IniciarSesion()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarSesion(string? correo, string? clave)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrEmpty(clave))
            {
                ViewData["Mensaje"] = "Usuario o contraseña incorrectos";
                return View();
            }

            Usuario? usuario;
            try
            {
                usuario = await _usuarioService.GetUsuario(correo, clave);
            }
            catch (Exception ex) when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
            {
                _logger.LogError(ex, "Error de base de datos al iniciar sesión.");
                ViewData["Mensaje"] = "No se pudo consultar la cuenta en este momento. Intente nuevamente en unos momentos.";
                return View();
            }
            if (usuario == null ||
                usuario.Rol is not ("Administrador" or "Secretario" or "Usuario"))
            {
                ViewData["Mensaje"] = "Usuario o contraseña incorrectos";
                return View();
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            ClaimsIdentity identity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties { AllowRefresh = true });

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }
    }
}
