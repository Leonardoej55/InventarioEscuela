using System.ComponentModel.DataAnnotations;
namespace InventarioEscuela.ViewModels
{
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
        [StringLength(100, ErrorMessage = "El correo no puede superar los 100 caracteres.")]
        public string Correo { get; set; } = string.Empty;
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(128, MinimumLength = 12,
            ErrorMessage = "La contraseña debe tener entre 12 y 128 caracteres.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Confirma la contraseña.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } = string.Empty;
        [Required(ErrorMessage = "Selecciona un rol.")]
        [RegularExpression("^(Administrador|Secretario|Usuario)$",
            ErrorMessage = "El rol seleccionado no es válido.")]
        public string Rol { get; set; } = "Usuario";
    }
}