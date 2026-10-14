namespace InventarioEscuela.ViewModels
{
    public class ErrorOperacionViewModel
    {
        public string Titulo { get; set; } = "No se pudo completar la operación";
        public string Mensaje { get; set; } = string.Empty;
        public string UrlReintento { get; set; } = string.Empty;
    }
}
