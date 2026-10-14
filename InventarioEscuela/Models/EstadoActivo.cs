using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace InventarioEscuela.Models
{
    [Table("EstadoActivo")]
    public class EstadoActivo
    {
        [Key]
        [Column("IdEstado")]
        public int IdEstado { get; set; }

        [Required]
        [Column("Estado")]
        [StringLength(50)]
        public string Estado { get; set; }

        // Relación: un EstadoActivo tiene muchos Activos
        public virtual ICollection<Activo> Activos { get; set; }
    }
}