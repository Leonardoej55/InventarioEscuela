using System.Globalization;
using InventarioEscuela.Data;
using InventarioEscuela.Models;
using InventarioEscuela.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace InventarioEscuela.Services.Reportes;

/// <summary>Una definición de filtros, orden y totales para vista previa y exportaciones.</summary>
public sealed class ReporteConsultaService(ApplicationDbContext context) : IReporteConsultaService
{
    private const int FilasPorPagina = 10;
    private const string AlcanceInventario = "Estado y ubicación actuales de los activos. Las fechas filtran la fecha de registro; no reconstruyen el inventario de una fecha pasada.";

    public async Task<ReporteInventarioGeneralViewModel> PrepararInventarioGeneralAsync(ReporteInventarioGeneralViewModel filtros, CancellationToken ct = default)
    {
        var model = new ReporteInventarioGeneralViewModel
        {
            CodigoFiltro = Texto(filtros.CodigoFiltro),
            DescripcionFiltro = Texto(filtros.DescripcionFiltro),
            IdMarcaFiltro = filtros.IdMarcaFiltro,
            SerieFiltro = Texto(filtros.SerieFiltro),
            IdUbicacionFiltro = filtros.IdUbicacionFiltro,
            IdEstadoFiltro = filtros.IdEstadoFiltro,
            IdModoFiltro = filtros.IdModoFiltro,
            FechaDesde = filtros.FechaDesde?.Date,
            FechaHasta = filtros.FechaHasta?.Date,
            Alcance = AlcanceInventario,
            Marcas = await MarcasAsync(ct),
            Ubicaciones = await UbicacionesAsync(ct),
            Estados = await EstadosAsync(ct),
            ModosAdquisicion = await ModosAsync(ct)
        };
        FiltrosActivo(model.FiltrosAplicados, model.CodigoFiltro, model.DescripcionFiltro, model.IdMarcaFiltro,
            model.Marcas, model.IdUbicacionFiltro, model.Ubicaciones);
        AgregarTexto(model.FiltrosAplicados, "Serie", model.SerieFiltro);
        AgregarCatalogo(model.FiltrosAplicados, "Estado", model.IdEstadoFiltro, model.Estados, e => e.IdEstado, e => e.Estado);
        AgregarCatalogo(model.FiltrosAplicados, "Adquisición", model.IdModoFiltro, model.ModosAdquisicion, m => m.IdModoAdquisicion, m => m.NombreModoAdquisicion);
        AgregarFechas(model.FiltrosAplicados, "Registro", model.FechaDesde, model.FechaHasta);
        return model;
    }

    public async Task<ReporteInventarioGeneralViewModel> ConsultarInventarioGeneralAsync(ReporteInventarioGeneralViewModel filtros, int? pagina = 1, CancellationToken ct = default)
    {
        ValidarFechas(filtros.FechaDesde, filtros.FechaHasta);
        var model = await PrepararInventarioGeneralAsync(filtros, ct);
        var consulta = FiltrarActivos(Activos(), model.CodigoFiltro, model.DescripcionFiltro, model.IdMarcaFiltro,
            null, model.SerieFiltro, model.IdUbicacionFiltro, model.IdEstadoFiltro, model.IdModoFiltro);
        consulta = FechasRegistro(consulta, model.FechaDesde, model.FechaHasta);
        if (!pagina.HasValue)
        {
            model.Activos = await OrdenarActivos(consulta).ToListAsync(ct);
            model.TotalResultados = model.Activos.Count;
            model.TotalFuncionales = model.Activos.Count(a => EstadosReporte.Clasificar(a.EstadoActivo?.Estado) == GrupoEstadoReporte.Funcional);
            model.TotalDanados = model.Activos.Count(a => EstadosReporte.Clasificar(a.EstadoActivo?.Estado) == GrupoEstadoReporte.Danado);
            model.TotalMantenimiento = model.Activos.Count(a => EstadosReporte.Clasificar(a.EstadoActivo?.Estado) == GrupoEstadoReporte.EnMantenimiento);
            model.TotalOtrosEstados = model.TotalResultados - model.TotalFuncionales - model.TotalDanados - model.TotalMantenimiento;
            ActualizarPaginacion(model, null);
            return model;
        }
        var estados = await consulta.GroupBy(a => a.EstadoActivo.Estado)
            .Select(g => new { Estado = g.Key, Total = g.Count() }).ToListAsync(ct);
        model.TotalResultados = estados.Sum(e => e.Total);
        model.TotalFuncionales = estados.Where(e => EstadosReporte.Clasificar(e.Estado) == GrupoEstadoReporte.Funcional).Sum(e => e.Total);
        model.TotalDanados = estados.Where(e => EstadosReporte.Clasificar(e.Estado) == GrupoEstadoReporte.Danado).Sum(e => e.Total);
        model.TotalMantenimiento = estados.Where(e => EstadosReporte.Clasificar(e.Estado) == GrupoEstadoReporte.EnMantenimiento).Sum(e => e.Total);
        model.TotalOtrosEstados = model.TotalResultados - model.TotalFuncionales - model.TotalDanados - model.TotalMantenimiento;
        model.Activos = await Paginar(OrdenarActivos(consulta), model, pagina).ToListAsync(ct);
        return model;
    }

    public async Task<ReporteRecursosPorUbicacionViewModel> PrepararRecursosPorUbicacionAsync(ReporteRecursosPorUbicacionViewModel filtros, CancellationToken ct = default)
    {
        var model = new ReporteRecursosPorUbicacionViewModel
        {
            IdUbicacionFiltro = filtros.IdUbicacionFiltro,
            CodigoFiltro = Texto(filtros.CodigoFiltro),
            DescripcionFiltro = Texto(filtros.DescripcionFiltro),
            IdMarcaFiltro = filtros.IdMarcaFiltro,
            IdEstadoFiltro = filtros.IdEstadoFiltro,
            IdModoFiltro = filtros.IdModoFiltro,
            Alcance = "Distribución de los activos por su ubicación y estado actuales. Incluye todos los años de registro salvo los demás filtros seleccionados.",
            Ubicaciones = await UbicacionesAsync(ct),
            Marcas = await MarcasAsync(ct),
            Estados = await EstadosAsync(ct),
            ModosAdquisicion = await ModosAsync(ct)
        };
        FiltrosActivo(model.FiltrosAplicados, model.CodigoFiltro, model.DescripcionFiltro, model.IdMarcaFiltro,
            model.Marcas, model.IdUbicacionFiltro, model.Ubicaciones);
        AgregarCatalogo(model.FiltrosAplicados, "Estado", model.IdEstadoFiltro, model.Estados, e => e.IdEstado, e => e.Estado);
        AgregarCatalogo(model.FiltrosAplicados, "Adquisición", model.IdModoFiltro, model.ModosAdquisicion, m => m.IdModoAdquisicion, m => m.NombreModoAdquisicion);
        return model;
    }

    public async Task<ReporteRecursosPorUbicacionViewModel> ConsultarRecursosPorUbicacionAsync(ReporteRecursosPorUbicacionViewModel filtros, int? pagina = 1, CancellationToken ct = default)
    {
        var model = await PrepararRecursosPorUbicacionAsync(filtros, ct);
        var consulta = FiltrarActivos(Activos(), model.CodigoFiltro, model.DescripcionFiltro, model.IdMarcaFiltro,
            null, null, model.IdUbicacionFiltro, model.IdEstadoFiltro, model.IdModoFiltro);
        if (!pagina.HasValue)
        {
            model.Activos = await OrdenarActivos(consulta).ToListAsync(ct);
            model.TotalResultados = model.Activos.Count;
            model.TotalUbicaciones = model.Activos.Select(a => a.IdUbicacion).Distinct().Count();
            model.TotalMarcas = model.Activos.Select(a => a.IdMarca).Distinct().Count();
            model.TotalEstados = model.Activos.Select(a => a.IdEstado).Distinct().Count();
            model.ResumenUbicaciones = model.Activos.GroupBy(a => a.IdUbicacion)
                .Select(g => new ResumenUbicacionReporte
                {
                    IdUbicacion = g.Key,
                    Ubicacion = g.First().Ubicacion?.UbicacionNombre ?? "Sin ubicación",
                    Total = g.Count(),
                    Funcionales = g.Count(a => EstadosReporte.Clasificar(a.EstadoActivo?.Estado) == GrupoEstadoReporte.Funcional),
                    Danados = g.Count(a => EstadosReporte.Clasificar(a.EstadoActivo?.Estado) == GrupoEstadoReporte.Danado),
                    EnMantenimiento = g.Count(a => EstadosReporte.Clasificar(a.EstadoActivo?.Estado) == GrupoEstadoReporte.EnMantenimiento),
                    Otros = g.Count(a => EstadosReporte.Clasificar(a.EstadoActivo?.Estado) == GrupoEstadoReporte.Otro)
                }).OrderBy(g => g.Ubicacion).ThenBy(g => g.IdUbicacion).ToList();
            ActualizarPaginacion(model, null);
            return model;
        }
        var grupos = await consulta.GroupBy(a => new { a.IdUbicacion, a.Ubicacion.UbicacionNombre, a.EstadoActivo.Estado })
            .Select(g => new { g.Key.IdUbicacion, g.Key.UbicacionNombre, g.Key.Estado, Total = g.Count() }).ToListAsync(ct);
        model.ResumenUbicaciones = grupos.GroupBy(g => new { g.IdUbicacion, g.UbicacionNombre })
            .OrderBy(g => g.Key.UbicacionNombre).ThenBy(g => g.Key.IdUbicacion)
            .Select(g => new ResumenUbicacionReporte
            {
                IdUbicacion = g.Key.IdUbicacion,
                Ubicacion = g.Key.UbicacionNombre,
                Total = g.Sum(e => e.Total),
                Funcionales = g.Where(e => EstadosReporte.Clasificar(e.Estado) == GrupoEstadoReporte.Funcional).Sum(e => e.Total),
                Danados = g.Where(e => EstadosReporte.Clasificar(e.Estado) == GrupoEstadoReporte.Danado).Sum(e => e.Total),
                EnMantenimiento = g.Where(e => EstadosReporte.Clasificar(e.Estado) == GrupoEstadoReporte.EnMantenimiento).Sum(e => e.Total),
                Otros = g.Where(e => EstadosReporte.Clasificar(e.Estado) == GrupoEstadoReporte.Otro).Sum(e => e.Total)
            }).ToList();
        model.TotalResultados = model.ResumenUbicaciones.Sum(g => g.Total);
        model.TotalUbicaciones = model.ResumenUbicaciones.Count;
        model.TotalMarcas = await consulta.Select(a => a.IdMarca).Distinct().CountAsync(ct);
        model.TotalEstados = await consulta.Select(a => a.IdEstado).Distinct().CountAsync(ct);
        model.Activos = await Paginar(OrdenarActivos(consulta), model, pagina).ToListAsync(ct);
        return model;
    }

    public async Task<ReporteEquiposDanadosViewModel> PrepararEquiposDanadosAsync(ReporteEquiposDanadosViewModel filtros, CancellationToken ct = default)
    {
        var model = new ReporteEquiposDanadosViewModel
        {
            CodigoFiltro = Texto(filtros.CodigoFiltro),
            DescripcionFiltro = Texto(filtros.DescripcionFiltro),
            IdMarcaFiltro = filtros.IdMarcaFiltro,
            ModeloFiltro = Texto(filtros.ModeloFiltro),
            SerieFiltro = Texto(filtros.SerieFiltro),
            IdUbicacionFiltro = filtros.IdUbicacionFiltro,
            IdModoFiltro = filtros.IdModoFiltro,
            Alcance = "Activos cuyo estado actual es Dañado o Malo, con su ubicación actual. No indica la fecha ni la gravedad de la avería.",
            Marcas = await MarcasAsync(ct),
            Ubicaciones = await UbicacionesAsync(ct),
            ModosAdquisicion = await ModosAsync(ct)
        };
        FiltrosActivo(model.FiltrosAplicados, model.CodigoFiltro, model.DescripcionFiltro, model.IdMarcaFiltro,
            model.Marcas, model.IdUbicacionFiltro, model.Ubicaciones);
        AgregarTexto(model.FiltrosAplicados, "Modelo", model.ModeloFiltro);
        AgregarTexto(model.FiltrosAplicados, "Serie", model.SerieFiltro);
        AgregarCatalogo(model.FiltrosAplicados, "Adquisición", model.IdModoFiltro, model.ModosAdquisicion, m => m.IdModoAdquisicion, m => m.NombreModoAdquisicion);
        return model;
    }

    public async Task<ReporteEquiposDanadosViewModel> ConsultarEquiposDanadosAsync(ReporteEquiposDanadosViewModel filtros, int? pagina = 1, CancellationToken ct = default)
    {
        var model = await PrepararEquiposDanadosAsync(filtros, ct);
        var consulta = FiltrarActivos(Activos().Where(EstadosReporte.EsDanado), model.CodigoFiltro, model.DescripcionFiltro,
            model.IdMarcaFiltro, model.ModeloFiltro, model.SerieFiltro, model.IdUbicacionFiltro, null, model.IdModoFiltro);
        if (!pagina.HasValue)
        {
            model.Activos = await OrdenarActivos(consulta).ToListAsync(ct);
            model.TotalResultados = model.Activos.Count;
            model.TotalUbicaciones = model.Activos.Select(a => a.IdUbicacion).Distinct().Count();
            model.TotalMarcas = model.Activos.Select(a => a.IdMarca).Distinct().Count();
            model.TotalConObservacion = model.Activos.Count(a => !string.IsNullOrWhiteSpace(a.Observacion));
            ActualizarPaginacion(model, null);
            return model;
        }
        model.TotalResultados = await consulta.CountAsync(ct);
        model.TotalUbicaciones = await consulta.Select(a => a.IdUbicacion).Distinct().CountAsync(ct);
        model.TotalMarcas = await consulta.Select(a => a.IdMarca).Distinct().CountAsync(ct);
        model.TotalConObservacion = await consulta.CountAsync(a => a.Observacion != null && a.Observacion.Trim() != "", ct);
        model.Activos = await Paginar(OrdenarActivos(consulta), model, pagina).ToListAsync(ct);
        return model;
    }

    public async Task<ReporteEquiposMantenimientoViewModel> PrepararEquiposMantenimientoAsync(ReporteEquiposMantenimientoViewModel filtros, CancellationToken ct = default)
    {
        var model = new ReporteEquiposMantenimientoViewModel
        {
            CodigoFiltro = Texto(filtros.CodigoFiltro),
            DescripcionFiltro = Texto(filtros.DescripcionFiltro),
            IdMarcaFiltro = filtros.IdMarcaFiltro,
            ModeloFiltro = Texto(filtros.ModeloFiltro),
            SerieFiltro = Texto(filtros.SerieFiltro),
            IdUbicacionFiltro = filtros.IdUbicacionFiltro,
            IdTipoFiltro = filtros.IdTipoFiltro,
            ResponsableFiltro = Texto(filtros.ResponsableFiltro),
            FechaDesde = filtros.FechaDesde?.Date,
            FechaHasta = filtros.FechaHasta?.Date,
            Alcance = "Historial de intervenciones registradas, con la ubicación actual del activo. Cada fila representa un mantenimiento; no indica si sigue abierto ni que el activo esté actualmente en mantenimiento.",
            Marcas = await MarcasAsync(ct),
            Ubicaciones = await UbicacionesAsync(ct),
            TiposMantenimiento = await context.TiposMantenimiento.AsNoTracking().OrderBy(t => t.Mantenimiento).ToListAsync(ct)
        };
        FiltrosActivo(model.FiltrosAplicados, model.CodigoFiltro, model.DescripcionFiltro, model.IdMarcaFiltro,
            model.Marcas, model.IdUbicacionFiltro, model.Ubicaciones);
        AgregarTexto(model.FiltrosAplicados, "Modelo", model.ModeloFiltro);
        AgregarTexto(model.FiltrosAplicados, "Serie", model.SerieFiltro);
        AgregarCatalogo(model.FiltrosAplicados, "Tipo de mantenimiento", model.IdTipoFiltro, model.TiposMantenimiento, t => t.IdTipo, t => t.Mantenimiento);
        AgregarTexto(model.FiltrosAplicados, "Responsable", model.ResponsableFiltro);
        AgregarFechas(model.FiltrosAplicados, "Mantenimiento", model.FechaDesde, model.FechaHasta);
        return model;
    }

    public async Task<ReporteEquiposMantenimientoViewModel> ConsultarEquiposMantenimientoAsync(ReporteEquiposMantenimientoViewModel filtros, int? pagina = 1, CancellationToken ct = default)
    {
        ValidarFechas(filtros.FechaDesde, filtros.FechaHasta);
        var model = await PrepararEquiposMantenimientoAsync(filtros, ct);
        var consulta = context.Mantenimientos.AsNoTracking()
            .Include(m => m.Activo).ThenInclude(a => a.Marca)
            .Include(m => m.Activo).ThenInclude(a => a.Ubicacion)
            .Include(m => m.Activo).ThenInclude(a => a.EstadoActivo)
            .Include(m => m.TipoMantenimiento).AsQueryable();
        if (model.CodigoFiltro is { } codigo) consulta = consulta.Where(m => m.Activo.NumeroIdentificacion.Contains(codigo));
        if (model.DescripcionFiltro is { } descripcion) consulta = consulta.Where(m => m.Activo.Descripcion.Contains(descripcion));
        if (model.IdMarcaFiltro is { } marca) consulta = consulta.Where(m => m.Activo.IdMarca == marca);
        if (model.ModeloFiltro is { } modelo) consulta = consulta.Where(m => m.Activo.Modelo != null && m.Activo.Modelo.Contains(modelo));
        if (model.SerieFiltro is { } serie) consulta = consulta.Where(m => m.Activo.Serie != null && m.Activo.Serie.Contains(serie));
        if (model.IdUbicacionFiltro is { } ubicacion) consulta = consulta.Where(m => m.Activo.IdUbicacion == ubicacion);
        if (model.IdTipoFiltro is { } tipo) consulta = consulta.Where(m => m.IdTipo == tipo);
        if (model.ResponsableFiltro is { } responsable) consulta = consulta.Where(m => m.Responsable != null && m.Responsable.Contains(responsable));
        if (model.FechaDesde is { } desde) consulta = consulta.Where(m => m.FechaMantenimiento >= desde);
        if (model.FechaHasta is { } hasta && hasta < DateTime.MaxValue.Date)
        {
            var fin = hasta.AddDays(1);
            consulta = consulta.Where(m => m.FechaMantenimiento < fin);
        }
        if (!pagina.HasValue)
        {
            model.Mantenimientos = await consulta.OrderByDescending(m => m.FechaMantenimiento).ThenByDescending(m => m.IdMantenimiento).ToListAsync(ct);
            model.TotalResultados = model.Mantenimientos.Count;
            model.TotalActivos = model.Mantenimientos.Select(m => m.IdActivo).Distinct().Count();
            model.TotalUbicaciones = model.Mantenimientos.Select(m => m.Activo.IdUbicacion).Distinct().Count();
            model.TotalTiposMantenimiento = model.Mantenimientos.Select(m => m.IdTipo).Distinct().Count();
            model.TotalResponsables = model.Mantenimientos.Where(m => !string.IsNullOrWhiteSpace(m.Responsable))
                .Select(m => m.Responsable.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).Count();
            ActualizarPaginacion(model, null);
            return model;
        }
        model.TotalResultados = await consulta.CountAsync(ct);
        model.TotalActivos = await consulta.Select(m => m.IdActivo).Distinct().CountAsync(ct);
        model.TotalUbicaciones = await consulta.Select(m => m.Activo.IdUbicacion).Distinct().CountAsync(ct);
        model.TotalTiposMantenimiento = await consulta.Select(m => m.IdTipo).Distinct().CountAsync(ct);
        model.TotalResponsables = await consulta.Where(m => m.Responsable != null && m.Responsable.Trim() != "")
            .Select(m => m.Responsable.Trim()).Distinct().CountAsync(ct);
        model.Mantenimientos = await Paginar(consulta.OrderByDescending(m => m.FechaMantenimiento).ThenByDescending(m => m.IdMantenimiento), model, pagina).ToListAsync(ct);
        return model;
    }

    public async Task<ReporteHistorialMovimientosViewModel> PrepararHistorialMovimientosAsync(ReporteHistorialMovimientosViewModel filtros, CancellationToken ct = default)
    {
        var model = new ReporteHistorialMovimientosViewModel
        {
            CodigoFiltro = Texto(filtros.CodigoFiltro),
            DescripcionFiltro = Texto(filtros.DescripcionFiltro),
            IdUbicacionOrigenFiltro = filtros.IdUbicacionOrigenFiltro,
            IdUbicacionDestinoFiltro = filtros.IdUbicacionDestinoFiltro,
            ResponsableFiltro = Texto(filtros.ResponsableFiltro),
            ObservacionFiltro = Texto(filtros.ObservacionFiltro),
            FechaDesde = filtros.FechaDesde?.Date,
            FechaHasta = filtros.FechaHasta?.Date,
            Alcance = "Movimientos registrados en el período indicado, con el origen y destino de cada movimiento. La identificación, descripción y nombres de ubicaciones son los actuales.",
            Ubicaciones = await UbicacionesAsync(ct)
        };
        AgregarTexto(model.FiltrosAplicados, "Código", model.CodigoFiltro);
        AgregarTexto(model.FiltrosAplicados, "Descripción", model.DescripcionFiltro);
        AgregarCatalogo(model.FiltrosAplicados, "Origen", model.IdUbicacionOrigenFiltro, model.Ubicaciones, u => u.IdUbicacion, u => u.UbicacionNombre);
        AgregarCatalogo(model.FiltrosAplicados, "Destino", model.IdUbicacionDestinoFiltro, model.Ubicaciones, u => u.IdUbicacion, u => u.UbicacionNombre);
        AgregarTexto(model.FiltrosAplicados, "Responsable", model.ResponsableFiltro);
        AgregarTexto(model.FiltrosAplicados, "Observación", model.ObservacionFiltro);
        AgregarFechas(model.FiltrosAplicados, "Movimiento", model.FechaDesde, model.FechaHasta);
        return model;
    }

    public async Task<ReporteHistorialMovimientosViewModel> ConsultarHistorialMovimientosAsync(ReporteHistorialMovimientosViewModel filtros, int? pagina = 1, CancellationToken ct = default)
    {
        ValidarFechas(filtros.FechaDesde, filtros.FechaHasta);
        var model = await PrepararHistorialMovimientosAsync(filtros, ct);
        var consulta = context.MovimientosActivos.AsNoTracking().Include(m => m.Activo).ThenInclude(a => a.Marca)
            .Include(m => m.MovimientosOrigen).Include(m => m.MovimientosDestino).AsQueryable();
        if (model.CodigoFiltro is { } codigo) consulta = consulta.Where(m => m.Activo.NumeroIdentificacion.Contains(codigo));
        if (model.DescripcionFiltro is { } descripcion) consulta = consulta.Where(m => m.Activo.Descripcion.Contains(descripcion));
        if (model.IdUbicacionOrigenFiltro is { } origen) consulta = consulta.Where(m => m.UbicacionOrigen == origen);
        if (model.IdUbicacionDestinoFiltro is { } destino) consulta = consulta.Where(m => m.UbicacionDestino == destino);
        if (model.ResponsableFiltro is { } responsable) consulta = consulta.Where(m => m.Responsable != null && m.Responsable.Contains(responsable));
        if (model.ObservacionFiltro is { } observacion) consulta = consulta.Where(m => m.Observacion != null && m.Observacion.Contains(observacion));
        if (model.FechaDesde is { } desde) consulta = consulta.Where(m => m.FechaMovimiento >= desde);
        if (model.FechaHasta is { } hasta && hasta < DateTime.MaxValue.Date)
        {
            var fin = hasta.AddDays(1);
            consulta = consulta.Where(m => m.FechaMovimiento < fin);
        }
        if (!pagina.HasValue)
        {
            model.Movimientos = await consulta.OrderByDescending(m => m.FechaMovimiento).ThenByDescending(m => m.IdMovimiento).ToListAsync(ct);
            model.TotalMovimientos = model.TotalResultados = model.Movimientos.Count;
            model.TotalActivosMovidos = model.Movimientos.Select(m => m.IdActivo).Distinct().Count();
            model.TotalUbicacionesOrigen = model.Movimientos.Where(m => m.UbicacionOrigen.HasValue).Select(m => m.UbicacionOrigen).Distinct().Count();
            model.TotalUbicacionesDestino = model.Movimientos.Select(m => m.UbicacionDestino).Distinct().Count();
            ActualizarPaginacion(model, null);
            return model;
        }
        model.TotalMovimientos = model.TotalResultados = await consulta.CountAsync(ct);
        model.TotalActivosMovidos = await consulta.Select(m => m.IdActivo).Distinct().CountAsync(ct);
        model.TotalUbicacionesOrigen = await consulta.Where(m => m.UbicacionOrigen != null).Select(m => m.UbicacionOrigen).Distinct().CountAsync(ct);
        model.TotalUbicacionesDestino = await consulta.Select(m => m.UbicacionDestino).Distinct().CountAsync(ct);
        model.Movimientos = await Paginar(consulta.OrderByDescending(m => m.FechaMovimiento).ThenByDescending(m => m.IdMovimiento), model, pagina).ToListAsync(ct);
        return model;
    }

    private IQueryable<Activo> Activos() => context.Activos.AsNoTracking().Include(a => a.Marca)
        .Include(a => a.Ubicacion).Include(a => a.EstadoActivo).Include(a => a.ModoAdquisicion);

    private static IOrderedQueryable<Activo> OrdenarActivos(IQueryable<Activo> consulta) =>
        consulta.OrderBy(a => a.Ubicacion.UbicacionNombre).ThenBy(a => a.NumeroIdentificacion).ThenBy(a => a.IdActivo);

    private static IQueryable<Activo> FiltrarActivos(IQueryable<Activo> consulta, string? codigo, string? descripcion,
        int? marca, string? modelo, string? serie, int? ubicacion, int? estado, int? modo)
    {
        if (codigo != null) consulta = consulta.Where(a => a.NumeroIdentificacion.Contains(codigo));
        if (descripcion != null) consulta = consulta.Where(a => a.Descripcion.Contains(descripcion));
        if (marca.HasValue) consulta = consulta.Where(a => a.IdMarca == marca.Value);
        if (modelo != null) consulta = consulta.Where(a => a.Modelo != null && a.Modelo.Contains(modelo));
        if (serie != null) consulta = consulta.Where(a => a.Serie != null && a.Serie.Contains(serie));
        if (ubicacion.HasValue) consulta = consulta.Where(a => a.IdUbicacion == ubicacion.Value);
        if (estado.HasValue) consulta = consulta.Where(a => a.IdEstado == estado.Value);
        if (modo.HasValue) consulta = consulta.Where(a => a.IdModo == modo.Value);
        return consulta;
    }

    private static IQueryable<Activo> FechasRegistro(IQueryable<Activo> consulta, DateTime? desde, DateTime? hasta)
    {
        if (desde.HasValue) consulta = consulta.Where(a => a.FechaRegistro >= desde.Value);
        if (hasta.HasValue && hasta.Value < DateTime.MaxValue.Date)
        {
            var fin = hasta.Value.AddDays(1);
            consulta = consulta.Where(a => a.FechaRegistro < fin);
        }
        return consulta;
    }

    private static IQueryable<T> Paginar<T>(IQueryable<T> consulta, ReporteBaseViewModel model, int? pagina)
    {
        ActualizarPaginacion(model, pagina);
        return pagina.HasValue
            ? consulta.Skip((model.PaginaActual - 1) * FilasPorPagina).Take(FilasPorPagina)
            : consulta;
    }

    private static void ActualizarPaginacion(ReporteBaseViewModel model, int? pagina)
    {
        model.TamanoPagina = FilasPorPagina;
        model.TotalPaginas = (int)Math.Ceiling(model.TotalResultados / (double)FilasPorPagina);
        model.PaginaActual = Math.Clamp(pagina ?? 1, 1, Math.Max(1, model.TotalPaginas));
    }

    private Task<List<Marca>> MarcasAsync(CancellationToken ct) => context.Marcas.AsNoTracking().OrderBy(m => m.MarcaNombre).ToListAsync(ct);
    private Task<List<Ubicacion>> UbicacionesAsync(CancellationToken ct) => context.Ubicaciones.AsNoTracking().OrderBy(u => u.UbicacionNombre).ToListAsync(ct);
    private Task<List<EstadoActivo>> EstadosAsync(CancellationToken ct) => context.EstadosActivos.AsNoTracking().OrderBy(e => e.Estado).ToListAsync(ct);
    private Task<List<ModoAdquisicion>> ModosAsync(CancellationToken ct) => context.ModosAdquisicion.AsNoTracking().OrderBy(m => m.NombreModoAdquisicion).ToListAsync(ct);

    private static string? Texto(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    private static void ValidarFechas(DateTime? desde, DateTime? hasta)
    {
        if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
            throw new ArgumentException("La fecha inicial no puede ser posterior a la fecha final.");
    }

    private static void FiltrosActivo(List<string> filtros, string? codigo, string? descripcion, int? marca,
        List<Marca> marcas, int? ubicacion, List<Ubicacion> ubicaciones)
    {
        AgregarTexto(filtros, "Código", codigo);
        AgregarTexto(filtros, "Descripción", descripcion);
        AgregarCatalogo(filtros, "Marca", marca, marcas, m => m.IdMarca, m => m.MarcaNombre);
        AgregarCatalogo(filtros, "Ubicación actual", ubicacion, ubicaciones, u => u.IdUbicacion, u => u.UbicacionNombre);
    }

    private static void AgregarTexto(List<string> filtros, string etiqueta, string? valor)
    {
        if (valor != null) filtros.Add($"{etiqueta}: {valor}");
    }

    private static void AgregarCatalogo<T>(List<string> filtros, string etiqueta, int? id, List<T> catalogo, Func<T, int> obtenerId, Func<T, string> nombre)
    {
        if (!id.HasValue) return;
        var elemento = catalogo.FirstOrDefault(e => obtenerId(e) == id.Value);
        filtros.Add($"{etiqueta}: {(elemento is null ? $"ID {id.Value} (no encontrado)" : nombre(elemento))}");
    }

    private static void AgregarFechas(List<string> filtros, string etiqueta, DateTime? desde, DateTime? hasta)
    {
        if (desde.HasValue) filtros.Add($"{etiqueta} desde: {desde.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}");
        if (hasta.HasValue) filtros.Add($"{etiqueta} hasta: {hasta.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} (inclusive)");
    }
}
