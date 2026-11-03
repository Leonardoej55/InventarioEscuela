using InventarioEscuela.Models;
using Microsoft.EntityFrameworkCore;

namespace InventarioEscuela.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

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

            modelBuilder.Entity<Activo>()
                .HasIndex(a => a.NumeroIdentificacion)
                .IsUnique();

            // La combinación es única cuando los tres campos tienen valor.
            modelBuilder.Entity<Activo>()
                .HasIndex(a => new { a.Tomo, a.Folio, a.Asiento })
                .HasDatabaseName("UX_Activo_Tomo_Folio_Asiento")
                .IsUnique()
                .HasFilter(
                    "[Tomo] IS NOT NULL AND " +
                    "[Folio] IS NOT NULL AND " +
                    "[Asiento] IS NOT NULL");

            modelBuilder.Entity<EstadoActivo>()
                .HasIndex(e => e.Estado)
                .IsUnique();

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