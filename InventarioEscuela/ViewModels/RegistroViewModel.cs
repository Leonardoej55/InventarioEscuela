using System.ComponentModel.DataAnnotations;

namespace InventarioEscuela.ViewModels
{
    public class RegistroViewModel
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }
}
    
