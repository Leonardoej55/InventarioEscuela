using System.Globalization;

namespace InventarioEscuela.ViewModels;

/// <summary>Reconstruye enlaces desde filtros ya interpretados, sin reutilizar parámetros de ruta arbitrarios.</summary>
public static class RutasReporte
{
    public static Dictionary<string, string> Crear(ReporteBaseViewModel modelo)
    {
        Dictionary<string, object?> valores = modelo switch
        {
            ReporteInventarioGeneralViewModel f => new()
            {
                ["codigoFiltro"] = f.CodigoFiltro,
                ["descripcionFiltro"] = f.DescripcionFiltro,
                ["idMarcaFiltro"] = f.IdMarcaFiltro,
                ["serieFiltro"] = f.SerieFiltro,
                ["idUbicacionFiltro"] = f.IdUbicacionFiltro,
                ["idEstadoFiltro"] = f.IdEstadoFiltro,
                ["idModoFiltro"] = f.IdModoFiltro,
                ["fechaDesde"] = f.FechaDesde,
                ["fechaHasta"] = f.FechaHasta,
            },
            ReporteRecursosPorUbicacionViewModel f => new()
            {
                ["idUbicacionFiltro"] = f.IdUbicacionFiltro,
                ["codigoFiltro"] = f.CodigoFiltro,
                ["descripcionFiltro"] = f.DescripcionFiltro,
                ["idMarcaFiltro"] = f.IdMarcaFiltro,
                ["idEstadoFiltro"] = f.IdEstadoFiltro,
                ["idModoFiltro"] = f.IdModoFiltro,
            },
            ReporteEquiposDanadosViewModel f => new()
            {
                ["codigoFiltro"] = f.CodigoFiltro,
                ["descripcionFiltro"] = f.DescripcionFiltro,
                ["idMarcaFiltro"] = f.IdMarcaFiltro,
                ["modeloFiltro"] = f.ModeloFiltro,
                ["serieFiltro"] = f.SerieFiltro,
                ["idUbicacionFiltro"] = f.IdUbicacionFiltro,
                ["idModoFiltro"] = f.IdModoFiltro,
            },
            ReporteEquiposMantenimientoViewModel f => new()
            {
                ["codigoFiltro"] = f.CodigoFiltro,
                ["descripcionFiltro"] = f.DescripcionFiltro,
                ["idMarcaFiltro"] = f.IdMarcaFiltro,
                ["modeloFiltro"] = f.ModeloFiltro,
                ["serieFiltro"] = f.SerieFiltro,
                ["idUbicacionFiltro"] = f.IdUbicacionFiltro,
                ["idTipoFiltro"] = f.IdTipoFiltro,
                ["responsableFiltro"] = f.ResponsableFiltro,
                ["fechaDesde"] = f.FechaDesde,
                ["fechaHasta"] = f.FechaHasta,
            },
            ReporteHistorialMovimientosViewModel f => new()
            {
                ["codigoFiltro"] = f.CodigoFiltro,
                ["descripcionFiltro"] = f.DescripcionFiltro,
                ["idUbicacionOrigenFiltro"] = f.IdUbicacionOrigenFiltro,
                ["idUbicacionDestinoFiltro"] = f.IdUbicacionDestinoFiltro,
                ["responsableFiltro"] = f.ResponsableFiltro,
                ["observacionFiltro"] = f.ObservacionFiltro,
                ["fechaDesde"] = f.FechaDesde,
                ["fechaHasta"] = f.FechaHasta,
            },
            _ => throw new ArgumentException("El tipo de reporte no tiene filtros de navegación definidos.", nameof(modelo))
        };
        return valores.Where(v => v.Value is not null && v.Value is not string { Length: 0 })
            .ToDictionary(v => v.Key, v => v.Value switch
            {
                DateTime fecha => fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                IFormattable valor => valor.ToString(null, CultureInfo.InvariantCulture),
                _ => v.Value!.ToString()!
            });
    }
}
