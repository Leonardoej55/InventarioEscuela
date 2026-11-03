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

public static class EstadosReporte
{
    public static GrupoEstadoReporte Clasificar(string? estado) =>
        estado?.Trim().ToUpperInvariant() switch
        {
            "BUENO" or "FUNCIONAL" => GrupoEstadoReporte.Funcional,
            "MALO" or "DAÑADO" => GrupoEstadoReporte.Danado,
            "EN MANTENIMIENTO" => GrupoEstadoReporte.EnMantenimiento,
            _ => GrupoEstadoReporte.Otro
        };

    public static Expression<Func<Activo, bool>> EsFuncional => activo =>
        activo.EstadoActivo != null &&
        (activo.EstadoActivo.Estado.Trim().ToUpper() == "BUENO" ||
         activo.EstadoActivo.Estado.Trim().ToUpper() == "FUNCIONAL");

    // Replace mantiene el tratamiento de ñ en proveedores como SQLite.
    public static Expression<Func<Activo, bool>> EsDanado => activo =>
        activo.EstadoActivo != null &&
        (activo.EstadoActivo.Estado.Trim().ToUpper().Replace("ñ", "Ñ") == "DAÑADO" ||
         activo.EstadoActivo.Estado.Trim().ToUpper() == "MALO");

    public static Expression<Func<Activo, bool>> EsEnMantenimiento => activo =>
        activo.EstadoActivo != null &&
        activo.EstadoActivo.Estado.Trim().ToUpper() == "EN MANTENIMIENTO";
}