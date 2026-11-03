using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace InventarioEscuela.ViewModels;

public abstract class ReporteBaseViewModel
{
    [BindNever] public List<string> FiltrosAplicados { get; set; } = [];
    [BindNever] public string Alcance { get; set; } = string.Empty;
    [BindNever] public int TotalResultados { get; set; }
    [BindNever] public int PaginaActual { get; set; } = 1;
    [BindNever] public int TamanoPagina { get; set; } = 10;
    [BindNever] public int TotalPaginas { get; set; }
}
