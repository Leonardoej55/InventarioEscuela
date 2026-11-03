using InventarioEscuela.Models;

namespace InventarioEscuela.Services
{
    public interface IUsuarioService
    {
        Task<Usuario?> GetUsuario(
            string correo,
            string clave);

        Task<bool> ExisteCorreo(
            string correo);

        Task<bool> HayUsuarios();

        Task<bool> CrearPrimerAdministrador(
            Usuario usuario,
            string clave);

        Task<Usuario> SaveUsuario(
            Usuario usuario,
            string clave);
    }
}