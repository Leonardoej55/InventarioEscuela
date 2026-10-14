using InventarioEscuela.Models;
namespace InventarioEscuela.Services
{
    public interface IUsuarioService
    {
        Task<Usuario?> GetUsuario(string correo, string clave);
        Task<bool> ExisteCorreo(string correo);
        Task<Usuario> SaveUsuario(Usuario usuario, string clave);
    }
}
