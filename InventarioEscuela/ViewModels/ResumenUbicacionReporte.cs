namespace InventarioEscuela.ViewModels;

public class ResumenUbicacionReporte
{
    public int IdUbicacion { get; set; }
    public string Ubicacion { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Funcionales { get; set; }
    public int Danados { get; set; }
    public int EnMantenimiento { get; set; }
    public int Otros { get; set; }
}
