using InventarioEscuela.Models;

namespace InventarioEscuela.ViewModels
{
    public class ReporteRecursosPorUbicacionViewModel
    {
        // =========================================
        // FILTROS
        // =========================================
        public int? IdUbicacionFiltro { get; set; }

        public string? CodigoFiltro { get; set; }

        public string? DescripcionFiltro { get; set; }

        public int? IdMarcaFiltro { get; set; }

        public int? IdEstadoFiltro { get; set; }

        public int? IdModoFiltro { get; set; }


        // =========================================
        // LISTAS PARA FILTROS
        // =========================================
        public List<Ubicacion> Ubicaciones { get; set; }
            = new List<Ubicacion>();

        public List<Marca> Marcas { get; set; }
            = new List<Marca>();

        public List<EstadoActivo> Estados { get; set; }
            = new List<EstadoActivo>();

        public List<ModoAdquisicion> ModosAdquisicion { get; set; }
            = new List<ModoAdquisicion>();


        // =========================================
        // RESULTADOS
        // =========================================
        public List<Activo> Activos { get; set; }
            = new List<Activo>();


        // =========================================
        // RESUMEN
        // =========================================
        public int TotalResultados { get; set; }

        public int TotalUbicaciones { get; set; }

        public int TotalMarcas { get; set; }

        public int TotalEstados { get; set; }


        // =========================================
        // PAGINACIÓN
        // =========================================
        public int PaginaActual { get; set; } = 1;

        public int TamanoPagina { get; set; } = 10;

        public int TotalPaginas { get; set; }
    }
}