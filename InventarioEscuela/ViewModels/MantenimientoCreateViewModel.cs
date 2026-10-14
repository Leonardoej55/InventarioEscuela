using System.ComponentModel.DataAnnotations;

namespace InventarioEscuela.ViewModels
{
    public class MantenimientoCreateViewModel
    {
        // =========================================
        // ACTIVO
        // =========================================
        [Required(
            ErrorMessage = "Debe seleccionar un activo.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Debe seleccionar un activo.")]
        public int IdActivo { get; set; }


        // =========================================
        // TIPO DE MANTENIMIENTO
        // =========================================
        [Required(
            ErrorMessage = "Debe seleccionar un tipo de mantenimiento.")]
        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Debe seleccionar un tipo de mantenimiento.")]
        public int IdTipo { get; set; }


        // =========================================
        // FECHA
        // =========================================
        [Required(
            ErrorMessage = "La fecha de mantenimiento es obligatoria.")]
        [DataType(DataType.Date)]
        public DateTime FechaMantenimiento { get; set; }
            = DateTime.Today;


        // =========================================
        // OBSERVACIONES
        // Se guarda en Mantenimiento.Descripcion
        // =========================================
        [Required(
            ErrorMessage = "Debe ingresar una observación del mantenimiento.")]
        [StringLength(
            300,
            ErrorMessage = "La observación no puede superar los 300 caracteres.")]
        public string Descripcion { get; set; }
            = string.Empty;


        // =========================================
        // RESPONSABLE
        // =========================================
        [StringLength(
            150,
            ErrorMessage = "El responsable no puede superar los 150 caracteres.")]
        public string? Responsable { get; set; }
    }
}