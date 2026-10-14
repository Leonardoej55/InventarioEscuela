    using InventarioEscuela.Models;

    namespace InventarioEscuela.Services
    {
        public interface IUsuarioService
        {
            Task<Usuario> GetUsuario(String correo, String clave);
            Task<Usuario> SaveUsuario(Usuario usuario);

        }
    }
