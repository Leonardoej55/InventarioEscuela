using InventarioEscuela.Models;

namespace InventarioEscuela.ViewModels
{
    public class ReporteEquiposMantenimientoViewModel
    {
        // =========================================
        // FILTROS
        // =========================================
        public string? CodigoFiltro { get; set; }

        public string? DescripcionFiltro { get; set; }

        public int? IdMarcaFiltro { get; set; }

        public string? ModeloFiltro { get; set; }

        public string? SerieFiltro { get; set; }

        public int? IdUbicacionFiltro { get; set; }

        public int? IdTipoFiltro { get; set; }

        public string? ResponsableFiltro { get; set; }


        // =========================================
        // LISTAS PARA FILTROS
        // =========================================
        public List<Marca> Marcas { get; set; }
            = new List<Marca>();

        public List<Ubicacion> Ubicaciones { get; set; }
            = new List<Ubicacion>();

        public List<TipoMantenimiento> TiposMantenimiento { get; set; }
            = new List<TipoMantenimiento>();


        // =========================================
        // RESULTADOS
        // =========================================
        public List<Mantenimiento> Mantenimientos { get; set; }
            = new List<Mantenimiento>();


        // =========================================
        // RESUMEN
        // =========================================
        public int TotalResultados { get; set; }

        public int TotalUbicaciones { get; set; }

        public int TotalTiposMantenimiento { get; set; }

        public int TotalResponsables { get; set; }


        // =========================================
        // PAGINACIÓN
        // =========================================
        public int PaginaActual { get; set; } = 1;

        public int TamanoPagina { get; set; } = 10;

        public int TotalPaginas { get; set; }
    }
}