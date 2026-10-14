using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace InventarioEscuela.Models
{
    [Table("Marca")]
    public class Marca
    {
        [Key]
        [Column("IdMarca")]
        public int IdMarca { get; set; }

        [Required]
        [Column("Marca")]
        [StringLength(100)]
        public string MarcaNombre { get; set; }  // Uso "MarcaNombre" para evitar conflicto con el nombre de la clase

        // Relación: una Marca tiene muchos Activos
        public virtual ICollection<Activo> Activos { get; set; }
    }
}