using InventarioEscuela.Services;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;

namespace InventarioEscuela.ViewModels
{
    public class MantenimientoCreateViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Debe seleccionar un activo.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un activo.")]
        public int IdActivo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un tipo de mantenimiento.")]
        [Range(1, int.MaxValue,
            ErrorMessage = "Debe seleccionar un tipo de mantenimiento.")]
        public int IdTipo { get; set; }

        [Required(ErrorMessage = "La fecha de mantenimiento es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime? FechaMantenimiento { get; set; }

        [Required(
            ErrorMessage = "Debe ingresar una observación del mantenimiento.")]
        [StringLength(300,
            ErrorMessage = "La observación no puede superar los 300 caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(
            ErrorMessage = "Debe indicar el responsable del mantenimiento.")]
        [StringLength(150,
            ErrorMessage = "El responsable no puede superar los 150 caracteres.")]
        public string Responsable { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(
            ValidationContext validationContext)
        {
            if (!FechaMantenimiento.HasValue)
                yield break;

            var reloj = validationContext.GetRequiredService<RelojSistema>();

            if (FechaMantenimiento.Value.Date > reloj.Hoy)
            {
                yield return new ValidationResult(
                    "La fecha de mantenimiento no puede ser futura.",
                    new[] { nameof(FechaMantenimiento) });
            }
        }
    }
}