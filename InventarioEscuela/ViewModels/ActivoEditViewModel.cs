using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace InventarioEscuela.ViewModels
{
    public class ActivoEditViewModel
    {
        [Range(1, int.MaxValue)]
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

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una marca.")]
        public int IdMarca { get; set; }

        [StringLength(100)]
        public string? Modelo { get; set; }

        [StringLength(100)]
        public string? Serie { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un estado.")]
        public int IdEstado { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un modo de adquisición.")]
        public int IdModo { get; set; }

        [StringLength(500)]
        public string? Observacion { get; set; }

        // Datos de consulta: siempre se cargan desde la base de datos.
        [BindNever]
        [ValidateNever]
        public string UbicacionActual { get; set; } = string.Empty;

        [BindNever]
        public DateTime FechaRegistro { get; set; }
    }
}
