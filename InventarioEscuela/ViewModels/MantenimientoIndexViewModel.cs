using InventarioEscuela.Models;

namespace InventarioEscuela.ViewModels
{
    public class MantenimientoIndexViewModel
    {
        // =========================================
        // MANTENIMIENTOS DE LA PÁGINA ACTUAL
        // =========================================
        public List<Mantenimiento> Mantenimientos { get; set; }
            = new List<Mantenimiento>();


        // =========================================
        // FILTROS
        // =========================================
        public string? CodigoFiltro { get; set; }

        public string? DescripcionActivoFiltro { get; set; }

        public int? IdUbicacionFiltro { get; set; }

        public int? IdEstadoFiltro { get; set; }

        public int? IdTipoFiltro { get; set; }

        public string? ResponsableFiltro { get; set; }

        public string? DescripcionMantenimientoFiltro { get; set; }


        // =========================================
        // LISTAS PARA FILTROS
        // =========================================
        public List<Ubicacion> Ubicaciones { get; set; }
            = new List<Ubicacion>();

        public List<EstadoActivo> Estados { get; set; }
            = new List<EstadoActivo>();

        public List<TipoMantenimiento> TiposMantenimiento { get; set; }
            = new List<TipoMantenimiento>();


        // =========================================
        // DASHBOARD
        // =========================================
        public int TotalMantenimientos { get; set; }

        public int ActivosConMantenimiento { get; set; }

        public int MantenimientosEsteMes { get; set; }

        public int ActivosDanados { get; set; }

        public int ActivosEnMantenimiento { get; set; }


        // =========================================
        // PAGINACIÓN
        // =========================================
        public int PaginaActual { get; set; } = 1;

        public int TamanoPagina { get; set; } = 10;

        public int TotalResultados { get; set; }

        public int TotalPaginas { get; set; }
    }
}