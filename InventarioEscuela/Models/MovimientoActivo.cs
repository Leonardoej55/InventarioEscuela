using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioEscuela.Models
{
    [Table("MovimientoActivo")]
    public class MovimientoActivo
    {
        [Key]
        [Column("IdMovimiento")]
        public int IdMovimiento { get; set; }

        [Required]
        [Column("IdActivo")]
        public int IdActivo { get; set; }

        [Column("UbicacionOrigen")]
        public int? UbicacionOrigen { get; set; }

        [Required]
        [Column("UbicacionDestino")]
        public int UbicacionDestino { get; set; }

        [Column("FechaMovimiento")]
        public DateTime FechaMovimiento { get; set; } = DateTime.Now;

        [Column("Responsable")]
        [StringLength(150)]
        public string Responsable { get; set; }

        [Column("Observacion")]
        [StringLength(300)]
        public string Observacion { get; set; }

        // Propiedades de navegación
        [ForeignKey("IdActivo")]
        public virtual Activo Activo { get; set; }

        [ForeignKey("UbicacionOrigen")]
        public virtual Ubicacion MovimientosOrigen { get; set; }

        [ForeignKey("UbicacionDestino")]
        public virtual Ubicacion MovimientosDestino { get; set; }
    }
}