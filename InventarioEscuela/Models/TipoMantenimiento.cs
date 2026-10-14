using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace InventarioEscuela.Models
{
    [Table("TipoMantenimiento")]
    public class TipoMantenimiento
    {
        [Key]
        [Column("IdTipo")]
        public int IdTipo { get; set; }

        [Required]
        [Column("Mantenimiento")]
        [StringLength(100)]
        public string Mantenimiento { get; set; }

        // Relación: un TipoMantenimiento tiene muchos Mantenimientos
        public virtual ICollection<Mantenimiento> Mantenimientos { get; set; }
    }
}