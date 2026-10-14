using InventarioEscuela.Models;
using InventarioEscuela.Services;
using InventarioEscuela.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventarioEscuela.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUsuarioService _usuarioService;

        public LoginController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        // =========================================
        // REGISTRO - GET
        // =========================================
        public IActionResult Registro()
        {
            return View();
        }

        // =========================================
        // REGISTRO - POST
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Password != model.ConfirmarPassword)
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden";
                return View(model);
            }

            Usuario usuario = new Usuario
            {
                Nombre = model.Nombre,
                Correo = model.Correo,
                PasswordHash = Utilidades.EncriptarClave(model.Password),
                Rol = "Usuario"
            };

            Usuario usuarioCreado = await _usuarioService.SaveUsuario(usuario);

            if (usuarioCreado != null && usuarioCreado.IdUsuario > 0)
            {
                return RedirectToAction(nameof(IniciarSesion));
            }

            ViewData["Mensaje"] = "No se pudo crear el usuario";

            return View(model);
        }

        // =========================================
        // INICIAR SESIÓN - GET
        // =========================================
        public IActionResult IniciarSesion()
        {
            return View();
        }

        // =========================================
        // INICIAR SESIÓN - POST
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IniciarSesion(string correo, string clave)
        {
            string claveEncriptada = Utilidades.EncriptarClave(clave);

            Usuario usuarioEncontrado =
                await _usuarioService.GetUsuario(correo, claveEncriptada);

            if (usuarioEncontrado == null)
            {
                ViewData["Mensaje"] = "Usuario o contraseña incorrectos";
                return View();
            }

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuarioEncontrado.Nombre),
                new Claim(ClaimTypes.Email, usuarioEncontrado.Correo),
                new Claim(ClaimTypes.Role, usuarioEncontrado.Rol)
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            AuthenticationProperties properties = new AuthenticationProperties
            {
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
            );

            return RedirectToAction("Index", "Home");
        }
    }
}