using System.ComponentModel.DataAnnotations;
using InventarioEscuela.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InventarioEscuela.ViewModels;

public class ReporteInventarioGeneralViewModel : ReporteBaseViewModel
{
    public string? CodigoFiltro { get; set; }
    public string? DescripcionFiltro { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una opción válida.")]
    public int? IdMarcaFiltro { get; set; }
    public string? SerieFiltro { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una opción válida.")]
    public int? IdUbicacionFiltro { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una opción válida.")]
    public int? IdEstadoFiltro { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una opción válida.")]
    public int? IdModoFiltro { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }

    [BindNever] public List<Marca> Marcas { get; set; } = [];
    [BindNever] public List<Ubicacion> Ubicaciones { get; set; } = [];
    [BindNever] public List<EstadoActivo> Estados { get; set; } = [];
    [BindNever] public List<ModoAdquisicion> ModosAdquisicion { get; set; } = [];
    [BindNever] public List<Activo> Activos { get; set; } = [];
    [BindNever] public int TotalFuncionales { get; set; }
    [BindNever] public int TotalDanados { get; set; }
    [BindNever] public int TotalMantenimiento { get; set; }
    [BindNever] public int TotalOtrosEstados { get; set; }
}
