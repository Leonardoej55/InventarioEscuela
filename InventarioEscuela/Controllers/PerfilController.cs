using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace InventarioEscuela.Controllers
{
    [Authorize(Roles = "Administrador,Secretario,Usuario")]
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