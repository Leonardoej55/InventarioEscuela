using System.ComponentModel.DataAnnotations;

namespace InventarioEscuela.ViewModels
{
    public class MovimientoCreateViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un activo.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un activo.")]
        public int IdActivo { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una ubicación de destino.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una ubicación de destino.")]
        public int UbicacionDestino { get; set; }

        [StringLength(150)]
        public string? Responsable { get; set; }

        [StringLength(300)]
        public string? Observacion { get; set; }
    }
}