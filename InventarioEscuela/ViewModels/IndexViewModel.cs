using System.Collections.Generic;

namespace InventarioEscuela.Models
{
    public class IndexViewModel
    {
        // =========================================
        // ACTIVOS MOSTRADOS EN LA PÁGINA ACTUAL
        // =========================================
        public List<Activo> Activos { get; set; }
            = new List<Activo>();

        public string NombreUsuario { get; set; }
            = string.Empty;


        // =========================================
        // DASHBOARD
        // =========================================
        public int TotalActivos { get; set; }

        public int TotalFuncionales { get; set; }

        public int TotalDanados { get; set; }

        public int TotalMantenimiento { get; set; }

        public int TotalUbicacionesConActivos { get; set; }


        // =========================================
        // FILTROS
        // =========================================
        public string? CodigoFiltro { get; set; }

        public string? DescripcionFiltro { get; set; }

        public int? IdMarcaFiltro { get; set; }

        public int? IdUbicacionFiltro { get; set; }

        public int? IdEstadoFiltro { get; set; }

        public DateTime? FechaFiltro { get; set; }


        // =========================================
        // LISTAS PARA SELECT
        // =========================================
        public List<Marca> Marcas { get; set; }
            = new List<Marca>();

        public List<Ubicacion> Ubicaciones { get; set; }
            = new List<Ubicacion>();

        public List<EstadoActivo> Estados { get; set; }
            = new List<EstadoActivo>();


        // =========================================
        // PAGINACIÓN
        // =========================================
        public int PaginaActual { get; set; } = 1;

        public int TamanoPagina { get; set; } = 20;

        public int TotalResultados { get; set; }

        public int TotalPaginas { get; set; }
    }
}