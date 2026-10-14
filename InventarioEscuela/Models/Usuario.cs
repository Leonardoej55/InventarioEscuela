using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioEscuela.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        [Column("IdUsuario")]
        public int IdUsuario { get; set; }

        [Required]
        [Column("Nombre")]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [Column("Correo")]
        [StringLength(100)]
        public string Correo { get; set; }

        [Required]
        [Column("PasswordHash")]
        [StringLength(255)]
        public string PasswordHash { get; set; }

        [Required]
        [Column("Rol")]
        [StringLength(50)]
        public string Rol { get; set; }
    }
}