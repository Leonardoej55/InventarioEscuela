using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioEscuela.Models
{
    [Table("Mantenimiento")]
    public class Mantenimiento
    {
        [Key]
        [Column("IdMantenimiento")]
        public int IdMantenimiento { get; set; }

        [Required]
        [Column("IdActivo")]
        public int IdActivo { get; set; }

        [Required]
        [Column("IdTipo")]
        public int IdTipo { get; set; }

        [Required]
        [Column("FechaMantenimiento")]
        public DateTime FechaMantenimiento { get; set; }

        [Column("Descripcion")]
        [StringLength(300)]
        public string Descripcion { get; set; }

        [Column("Responsable")]
        [StringLength(150)]
        public string Responsable { get; set; }

        // Propiedades de navegación
        [ForeignKey("IdActivo")]
        public virtual Activo Activo { get; set; }

        [ForeignKey("IdTipo")]
        public virtual TipoMantenimiento TipoMantenimiento { get; set; }
    }
}