using System.ComponentModel.DataAnnotations;
using InventarioEscuela.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InventarioEscuela.ViewModels;

public class ReporteEquiposDanadosViewModel : ReporteBaseViewModel
{
    public string? CodigoFiltro { get; set; }
    public string? DescripcionFiltro { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una opción válida.")]
    public int? IdMarcaFiltro { get; set; }
    public string? ModeloFiltro { get; set; }
    public string? SerieFiltro { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una opción válida.")]
    public int? IdUbicacionFiltro { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una opción válida.")]
    public int? IdModoFiltro { get; set; }

    [BindNever] public List<Marca> Marcas { get; set; } = [];
    [BindNever] public List<Ubicacion> Ubicaciones { get; set; } = [];
    [BindNever] public List<ModoAdquisicion> ModosAdquisicion { get; set; } = [];
    [BindNever] public List<Activo> Activos { get; set; } = [];
    [BindNever] public int TotalUbicaciones { get; set; }
    [BindNever] public int TotalMarcas { get; set; }
    [BindNever] public int TotalConObservacion { get; set; }
}
