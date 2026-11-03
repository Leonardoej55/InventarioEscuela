using System.ComponentModel.DataAnnotations;
using InventarioEscuela.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InventarioEscuela.ViewModels;

public class ReporteHistorialMovimientosViewModel : ReporteBaseViewModel
{
    public string? CodigoFiltro { get; set; }
    public string? DescripcionFiltro { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una opción válida.")]
    public int? IdUbicacionOrigenFiltro { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una opción válida.")]
    public int? IdUbicacionDestinoFiltro { get; set; }
    public string? ResponsableFiltro { get; set; }
    public string? ObservacionFiltro { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }

    [BindNever] public List<Ubicacion> Ubicaciones { get; set; } = [];
    [BindNever] public List<MovimientoActivo> Movimientos { get; set; } = [];
    [BindNever] public int TotalMovimientos { get; set; }
    [BindNever] public int TotalActivosMovidos { get; set; }
    [BindNever] public int TotalUbicacionesOrigen { get; set; }
    [BindNever] public int TotalUbicacionesDestino { get; set; }
}
