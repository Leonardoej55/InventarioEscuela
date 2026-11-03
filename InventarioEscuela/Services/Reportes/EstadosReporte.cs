using System.Linq.Expressions;
using InventarioEscuela.Models;

namespace InventarioEscuela.Services.Reportes;

public enum GrupoEstadoReporte
{
    Funcional,
    Danado,
    EnMantenimiento,
    Otro
}

/// <summary>Interpreta los nombres históricos del catálogo sin modificar los datos almacenados.</summary>
public static class EstadosReporte
{
    public static GrupoEstadoReporte Clasificar(string? estado) => estado?.Trim().ToUpperInvariant() switch
    {
        "BUENO" or "FUNCIONAL" => GrupoEstadoReporte.Funcional,
        "MALO" or "DAÑADO" => GrupoEstadoReporte.Danado,
        "EN MANTENIMIENTO" => GrupoEstadoReporte.EnMantenimiento,
        _ => GrupoEstadoReporte.Otro
    };

    // La expresión se ejecuta en la base. Replace conserva el comportamiento de ñ
    // también en proveedores cuyo UPPER solo transforma caracteres ASCII.
    public static Expression<Func<Activo, bool>> EsDanado => activo =>
        activo.EstadoActivo != null &&
        (activo.EstadoActivo.Estado.Trim().ToUpper().Replace("ñ", "Ñ") == "DAÑADO" ||
         activo.EstadoActivo.Estado.Trim().ToUpper() == "MALO");
}
