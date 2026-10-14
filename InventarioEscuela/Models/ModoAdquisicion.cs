using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace InventarioEscuela.Models
{
    [Table("ModoAdquisicion")]
    public class ModoAdquisicion
    {
        [Key]
        [Column("IdModoAdquisicion")]
        public int IdModoAdquisicion { get; set; }

        [Required]
        [Column("NombreModoAdquisicion")]
        [StringLength(100)]
        public string NombreModoAdquisicion { get; set; }

        // Relación: un ModoAdquisicion tiene muchos Activos
        public virtual ICollection<Activo> Activos { get; set; }
    }
}