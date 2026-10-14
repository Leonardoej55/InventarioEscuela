using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventarioEscuela.Models
{
    [Table("Activo")]
    public class Activo
    {
        [Key]
        [Column("IdActivo")]
        public int IdActivo { get; set; }

        [Required(ErrorMessage = "El número de identificación es obligatorio.")]
        [StringLength(50)]
        public string NumeroIdentificacion { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Tomo { get; set; }

        [StringLength(20)]
        public string? Folio { get; set; }

        [StringLength(20)]
        public string? Asiento { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(250)]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar una marca.")]
        public int IdMarca { get; set; }

        // =========================================
        // MODELO DEL ACTIVO
        // =========================================
        [StringLength(100)]
        public string? Modelo { get; set; }

        [StringLength(100)]
        public string? Serie { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estado.")]
        public int IdEstado { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una ubicación.")]
        public int IdUbicacion { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un modo de adquisición.")]
        public int IdModo { get; set; }

        [StringLength(500)]
        public string? Observacion { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;


        // =========================================
        // PROPIEDADES DE NAVEGACIÓN
        // =========================================

        [ForeignKey(nameof(IdMarca))]
        [ValidateNever]
        public Marca Marca { get; set; } = null!;

        [ForeignKey(nameof(IdEstado))]
        [ValidateNever]
        public EstadoActivo EstadoActivo { get; set; } = null!;

        [ForeignKey(nameof(IdUbicacion))]
        [ValidateNever]
        public Ubicacion Ubicacion { get; set; } = null!;

        [ForeignKey(nameof(IdModo))]
        [ValidateNever]
        public ModoAdquisicion ModoAdquisicion { get; set; } = null!;


        // =========================================
        // RELACIONES
        // =========================================

        [ValidateNever]
        public ICollection<Mantenimiento> Mantenimientos { get; set; }
            = new List<Mantenimiento>();

        [ValidateNever]
        public ICollection<MovimientoActivo> Movimientos { get; set; }
            = new List<MovimientoActivo>();
    }
}
