using InventarioEscuela.Models;

namespace InventarioEscuela.ViewModels
{
    public class ReporteEquiposDanadosViewModel
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

        public int? IdModoFiltro { get; set; }


        // =========================================
        // LISTAS PARA FILTROS
        // =========================================
        public List<Marca> Marcas { get; set; }
            = new List<Marca>();

        public List<Ubicacion> Ubicaciones { get; set; }
            = new List<Ubicacion>();

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

        public int TotalConObservacion { get; set; }


        // =========================================
        // PAGINACIÓN
        // =========================================
        public int PaginaActual { get; set; } = 1;

        public int TamanoPagina { get; set; } = 10;

        public int TotalPaginas { get; set; }
    }
}