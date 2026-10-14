using InventarioEscuela.Models;

namespace InventarioEscuela.ViewModels
{
    public class ReporteInventarioGeneralViewModel
    {
        // =========================================
        // FILTROS
        // =========================================
        public string? CodigoFiltro { get; set; }

        public string? DescripcionFiltro { get; set; }

        public int? IdMarcaFiltro { get; set; }

        public string? SerieFiltro { get; set; }

        public int? IdUbicacionFiltro { get; set; }

        public int? IdEstadoFiltro { get; set; }

        public int? IdModoFiltro { get; set; }

        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }


        // =========================================
        // LISTAS PARA FILTROS
        // =========================================
        public List<Marca> Marcas { get; set; }
            = new List<Marca>();

        public List<Ubicacion> Ubicaciones { get; set; }
            = new List<Ubicacion>();

        public List<EstadoActivo> Estados { get; set; }
            = new List<EstadoActivo>();

        public List<ModoAdquisicion> ModosAdquisicion { get; set; }
            = new List<ModoAdquisicion>();


        // =========================================
        // RESULTADOS DE LA PÁGINA ACTUAL
        // =========================================
        public List<Activo> Activos { get; set; }
            = new List<Activo>();


        // =========================================
        // RESUMEN DEL REPORTE
        // =========================================
        public int TotalResultados { get; set; }

        public int TotalFuncionales { get; set; }

        public int TotalDanados { get; set; }

        public int TotalMantenimiento { get; set; }


        // =========================================
        // PAGINACIÓN
        // =========================================
        public int PaginaActual { get; set; } = 1;

        public int TamanoPagina { get; set; } = 10;

        public int TotalPaginas { get; set; }
    }
}