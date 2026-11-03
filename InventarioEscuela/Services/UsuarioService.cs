using InventarioEscuela.Data;
using InventarioEscuela.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace InventarioEscuela.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<Usuario> _passwordHasher;

        public UsuarioService(
            ApplicationDbContext context,
            IPasswordHasher<Usuario> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<Usuario?> GetUsuario(
            string correo,
            string clave)
        {
            if (string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrEmpty(clave))
            {
                return null;
            }

            string correoNormalizado =
                correo.Trim().ToLowerInvariant();

            Usuario? usuario =
                await _context.Usuarios.FirstOrDefaultAsync(
                    u => u.Correo.Trim().ToLower() ==
                         correoNormalizado);

            if (usuario == null ||
                string.IsNullOrEmpty(usuario.PasswordHash))
            {
                return null;
            }

            PasswordVerificationResult resultado;

            try
            {
                resultado =
                    _passwordHasher.VerifyHashedPassword(
                        usuario,
                        usuario.PasswordHash,
                        clave);
            }
            catch (FormatException)
            {
                return null;
            }

            if (resultado ==
                PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (resultado ==
                PasswordVerificationResult.SuccessRehashNeeded)
            {
                usuario.PasswordHash =
                    _passwordHasher.HashPassword(
                        usuario,
                        clave);

                await _context.SaveChangesAsync();
            }

            return usuario;
        }

        public async Task<bool> ExisteCorreo(
            string correo)
        {
            string correoNormalizado =
                correo.Trim().ToLowerInvariant();

            return await _context.Usuarios.AnyAsync(
                u => u.Correo.Trim().ToLower() ==
                     correoNormalizado);
        }

        public Task<bool> HayUsuarios()
        {
            return _context.Usuarios.AnyAsync();
        }

        public async Task<bool> CrearPrimerAdministrador(
            Usuario usuario,
            string clave)
        {
            /*
             * Serializable evita que dos solicitudes
             * creen simultáneamente dos administradores
             * iniciales.
             */
            await using var transaccion =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            if (await HayUsuarios())
            {
                return false;
            }

            usuario.Rol = "Administrador";

            await SaveUsuario(
                usuario,
                clave);

            await transaccion.CommitAsync();

            return true;
        }

        public async Task<Usuario> SaveUsuario(
            Usuario usuario,
            string clave)
        {
            if (usuario.Rol is not
                ("Administrador" or "Secretario" or "Usuario"))
            {
                throw new ArgumentException(
                    "El rol seleccionado no es válido.",
                    nameof(usuario));
            }

            usuario.Nombre =
                usuario.Nombre.Trim();

            usuario.Correo =
                usuario.Correo.Trim().ToLowerInvariant();

            usuario.PasswordHash =
                _passwordHasher.HashPassword(
                    usuario,
                    clave);

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            return usuario;
        }
    }
}