using InventarioEscuela.Data;
using InventarioEscuela.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioEscuela.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _context;

        public UsuarioService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario> GetUsuario(string correo, string clave)
        {
            Usuario usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Correo == correo &&
                    u.PasswordHash == clave);

            return usuario;
        }

        public async Task<Usuario> SaveUsuario(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return usuario;
        }
    }
}