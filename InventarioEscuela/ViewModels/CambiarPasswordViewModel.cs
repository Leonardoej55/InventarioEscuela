using System.ComponentModel.DataAnnotations;

namespace InventarioEscuela.ViewModels;

public class CambiarPasswordViewModel
{
    [Required(ErrorMessage = "Ingrese su contraseña actual.")]
    [DataType(DataType.Password)]
    public string PasswordActual { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingrese la nueva contraseña.")]
    [StringLength(
        128,
        MinimumLength = 12,
        ErrorMessage = "La nueva contraseña debe tener entre 12 y 128 caracteres.")]
    [DataType(DataType.Password)]
    public string NuevoPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirme la nueva contraseña.")]
    [StringLength(
        128,
        ErrorMessage = "La confirmación no puede superar los 128 caracteres.")]
    [Compare(
        nameof(NuevoPassword),
        ErrorMessage = "Las contraseñas nuevas no coinciden.")]
    [DataType(DataType.Password)]
    public string ConfirmarPassword { get; set; } = string.Empty;
}