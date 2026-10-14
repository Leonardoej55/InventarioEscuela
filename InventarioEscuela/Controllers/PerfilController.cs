using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InventarioEscuela.Controllers
{
    [Authorize]
    public class PerfilController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewData["NombreUsuario"] =
                User.FindFirst(ClaimTypes.Name)?.Value ?? "Usuario";

            return View();
        }
    }
}