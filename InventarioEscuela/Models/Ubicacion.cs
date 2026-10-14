using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioEscuela.Models
{
    [Table("Ubicacion")]
    public class Ubicacion
    {
        [Key]
        [Column("IdUbicacion")]
        public int IdUbicacion { get; set; }

        [Required]
        [Column("Ubicacion")]
        [StringLength(100)]
        public string UbicacionNombre { get; set; }

        // Activos que están en esta ubicación
        public virtual ICollection<Activo> Activos { get; set; }

        // Movimientos donde esta ubicación es el origen
        [InverseProperty("MovimientosOrigen")]
        public virtual ICollection<MovimientoActivo> MovimientosOrigen { get; set; }

        // Movimientos donde esta ubicación es el destino
        [InverseProperty("MovimientosDestino")]
        public virtual ICollection<MovimientoActivo> MovimientosDestino { get; set; }
    }
}