namespace InventarioEscuela.Services.Reportes;

/// <summary>Resultado de una exportación, independiente de la respuesta HTTP.</summary>
public record ArchivoReporte(byte[] Contenido, string NombreArchivo, string TipoContenido);
