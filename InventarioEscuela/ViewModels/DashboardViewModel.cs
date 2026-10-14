using System.Collections.Generic;

namespace InventarioEscuela.ViewModels
{
    public class DashboardViewModel
    {
        // Totales para las tarjetas
        public int TotalActivos { get; set; }
        public int TotalFuncionales { get; set; }
        public int TotalDanados { get; set; }
        public int TotalMantenimiento { get; set; }
        public int TotalUbicacionesConActivos { get; set; }

        // Lista de movimientos recientes (por ejemplo, últimos 10)
        public List<MovimientoRecienteViewModel> MovimientosRecientes { get; set; }
    }

    public class MovimientoRecienteViewModel
    {
        public string ActivoNumeroIdentificacion { get; set; }
        public string ActivoDescripcion { get; set; }
        public string UbicacionOrigenNombre { get; set; }
        public string UbicacionDestinoNombre { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string Responsable { get; set; }
    }
}