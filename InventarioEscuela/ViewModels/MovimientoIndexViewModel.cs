using InventarioEscuela.Models;

namespace InventarioEscuela.ViewModels
{
    public class MovimientoIndexViewModel
    {
        // =========================================
        // MOVIMIENTOS DE LA PÁGINA ACTUAL
        // =========================================
        public List<MovimientoActivo> Movimientos { get; set; }
            = new List<MovimientoActivo>();


        // =========================================
        // FILTROS
        // =========================================
        public string? CodigoFiltro { get; set; }

        public string? DescripcionFiltro { get; set; }

        public int? IdUbicacionOrigenFiltro { get; set; }

        public int? IdUbicacionDestinoFiltro { get; set; }

        public DateTime? FechaFiltro { get; set; }

        public int? IdEstadoFiltro { get; set; }

        public string? ResponsableFiltro { get; set; }


        // =========================================
        // LISTAS PARA FILTROS
        // =========================================
        public List<Ubicacion> Ubicaciones { get; set; }
            = new List<Ubicacion>();

        public List<EstadoActivo> Estados { get; set; }
            = new List<EstadoActivo>();


        // =========================================
        // DASHBOARD
        // =========================================
        public int TotalMovimientos { get; set; }

        public int MovimientosHoy { get; set; }

        public int ActivosMovidos { get; set; }

        public int UbicacionesDestino { get; set; }


        // =========================================
        // PAGINACIÓN
        // =========================================
        public int PaginaActual { get; set; } = 1;

        public int TamanoPagina { get; set; } = 10;

        public int TotalResultados { get; set; }

        public int TotalPaginas { get; set; }
    }
}