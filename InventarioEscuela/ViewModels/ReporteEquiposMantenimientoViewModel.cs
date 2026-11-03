using System.ComponentModel.DataAnnotations;
using InventarioEscuela.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InventarioEscuela.ViewModels;

public class ReporteEquiposMantenimientoViewModel : ReporteBaseViewModel
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
    public int? IdTipoFiltro { get; set; }
    public string? ResponsableFiltro { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }

    [BindNever] public List<Marca> Marcas { get; set; } = [];
    [BindNever] public List<Ubicacion> Ubicaciones { get; set; } = [];
    [BindNever] public List<TipoMantenimiento> TiposMantenimiento { get; set; } = [];
    [BindNever] public List<Mantenimiento> Mantenimientos { get; set; } = [];
    [BindNever] public int TotalActivos { get; set; }
    [BindNever] public int TotalUbicaciones { get; set; }
    [BindNever] public int TotalTiposMantenimiento { get; set; }
    [BindNever] public int TotalResponsables { get; set; }
}
