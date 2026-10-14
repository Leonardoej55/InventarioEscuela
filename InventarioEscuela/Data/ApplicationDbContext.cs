using InventarioEscuela.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioEscuela.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para cada tabla
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Activo> Activos { get; set; }
        public DbSet<EstadoActivo> EstadosActivos { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Ubicacion> Ubicaciones { get; set; }
        public DbSet<ModoAdquisicion> ModosAdquisicion { get; set; }
        public DbSet<TipoMantenimiento> TiposMantenimiento { get; set; }
        public DbSet<Mantenimiento> Mantenimientos { get; set; }
        public DbSet<MovimientoActivo> MovimientosActivos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar relaciones y restricciones adicionales si es necesario
            // Por ejemplo, la unicidad de NumeroIdentificacion ya está en la BD,
            // pero Entity Framework no la conoce automáticamente, así que la agregamos:
            modelBuilder.Entity<Activo>()
                .HasIndex(a => a.NumeroIdentificacion)
                .IsUnique();

            // Restricción de unicidad en Estado (EstadoActivo)
            modelBuilder.Entity<EstadoActivo>()
                .HasIndex(e => e.Estado)
                .IsUnique();

            // Y así para las demás que tengan UNIQUE en la BD...
            modelBuilder.Entity<Marca>()
                .HasIndex(m => m.MarcaNombre)
                .IsUnique();

            modelBuilder.Entity<ModoAdquisicion>()
                .HasIndex(m => m.NombreModoAdquisicion)
                .IsUnique();

            modelBuilder.Entity<TipoMantenimiento>()
                .HasIndex(t => t.Mantenimiento)
                .IsUnique();

            modelBuilder.Entity<Ubicacion>()
                .HasIndex(u => u.UbicacionNombre)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();
        }
    }
}