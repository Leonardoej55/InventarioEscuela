using InventarioEscuela.Models;

namespace InventarioEscuela.ViewModels
{
    public class ReporteHistorialMovimientosViewModel
    {
        // =========================================
        // FILTROS
        // =========================================
        public string? CodigoFiltro { get; set; }

        public string? DescripcionFiltro { get; set; }

        public int? IdUbicacionOrigenFiltro { get; set; }

        public int? IdUbicacionDestinoFiltro { get; set; }

        public string? ResponsableFiltro { get; set; }

        public string? ObservacionFiltro { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }


        // =========================================
        // LISTAS
        // =========================================
        public List<Ubicacion> Ubicaciones { get; set; }
            = new List<Ubicacion>();


        // =========================================
        // RESULTADOS
        // =========================================
        public List<MovimientoActivo> Movimientos { get; set; }
            = new List<MovimientoActivo>();


        // =========================================
        // RESUMEN
        // =========================================
        public int TotalMovimientos { get; set; }

        public int TotalActivosMovidos { get; set; }

        public int TotalUbicacionesOrigen { get; set; }

        public int TotalUbicacionesDestino { get; set; }


        // =========================================
        // PAGINACIÓN
        // =========================================
        public int PaginaActual { get; set; } = 1;

        public int TamanoPagina { get; set; } = 10;

        public int TotalPaginas { get; set; }
    }
}