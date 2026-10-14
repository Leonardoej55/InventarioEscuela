using InventarioEscuela.Data;
using InventarioEscuela.Models;
using InventarioEscuela.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Security.Claims;

namespace InventarioEscuela.Controllers
{
    [Authorize(Roles = "Administrador,Secretario,Usuario")]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportesController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =========================================
        // MENÚ DE REPORTES
        // =========================================
        public IActionResult Index()
        {
            return View();
        }


        // =========================================
        // CONFIGURAR INVENTARIO GENERAL
        // =========================================
        [HttpGet]
        public async Task<IActionResult> InventarioGeneral(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            string? serieFiltro,
            int? idUbicacionFiltro,
            int? idEstadoFiltro,
            int? idModoFiltro,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            var model =
                new ReporteInventarioGeneralViewModel
                {
                    CodigoFiltro =
                        codigoFiltro,

                    DescripcionFiltro =
                        descripcionFiltro,

                    IdMarcaFiltro =
                        idMarcaFiltro,

                    SerieFiltro =
                        serieFiltro,

                    IdUbicacionFiltro =
                        idUbicacionFiltro,

                    IdEstadoFiltro =
                        idEstadoFiltro,

                    IdModoFiltro =
                        idModoFiltro,

                    FechaDesde =
                        fechaDesde,

                    FechaHasta =
                        fechaHasta
                };


            await CargarListasInventario(
                model);


            return View(model);
        }


        // =========================================
        // VISTA PREVIA INVENTARIO GENERAL
        // =========================================
        [HttpGet]
        public async Task<IActionResult> InventarioGeneralPreview(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            string? serieFiltro,
            int? idUbicacionFiltro,
            int? idEstadoFiltro,
            int? idModoFiltro,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int pagina = 1)
        {
            const int tamanoPagina = 10;


            // =====================================
            // VALIDAR FECHAS
            // =====================================
            if (fechaDesde.HasValue &&
                fechaHasta.HasValue &&
                fechaDesde.Value.Date >
                fechaHasta.Value.Date)
            {
                TempData["ErrorReporte"] =
                    "La fecha inicial no puede ser posterior a la fecha final.";

                return RedirectToAction(
                    nameof(InventarioGeneral));
            }


            // =====================================
            // CONSULTA BASE
            // =====================================
            var consulta =
                ConstruirConsultaInventario(
                    codigoFiltro,
                    descripcionFiltro,
                    idMarcaFiltro,
                    serieFiltro,
                    idUbicacionFiltro,
                    idEstadoFiltro,
                    idModoFiltro,
                    fechaDesde,
                    fechaHasta);


            // =====================================
            // TOTAL RESULTADOS FILTRADOS
            // =====================================
            int totalResultados =
                await consulta.CountAsync();


            // =====================================
            // TOTAL PÁGINAS
            // =====================================
            int totalPaginas =
                (int)Math.Ceiling(
                    totalResultados /
                    (double)tamanoPagina);


            if (pagina < 1)
            {
                pagina = 1;
            }


            if (totalPaginas > 0 &&
                pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }


            // =====================================
            // SOLO 10 ACTIVOS PARA LA VISTA PREVIA
            // =====================================
            var activos =
                await consulta
                    .OrderBy(a =>
                        a.NumeroIdentificacion)
                    .Skip(
                        (pagina - 1) *
                        tamanoPagina)
                    .Take(tamanoPagina)
                    .ToListAsync();


            // =====================================
            // TOTALES DEL REPORTE COMPLETO
            // =====================================
            int totalFuncionales =
                await consulta
                    .CountAsync(a =>
                        a.EstadoActivo != null &&
                        a.EstadoActivo.Estado ==
                        "Funcional");


            int totalDanados =
                await consulta
                    .CountAsync(a =>
                        a.EstadoActivo != null &&
                        a.EstadoActivo.Estado ==
                        "Dañado");


            int totalMantenimiento =
                await consulta
                    .CountAsync(a =>
                        a.EstadoActivo != null &&
                        a.EstadoActivo.Estado ==
                        "En mantenimiento");


            // =====================================
            // VIEWMODEL
            // =====================================
            var model =
                new ReporteInventarioGeneralViewModel
                {
                    CodigoFiltro =
                        codigoFiltro,

                    DescripcionFiltro =
                        descripcionFiltro,

                    IdMarcaFiltro =
                        idMarcaFiltro,

                    SerieFiltro =
                        serieFiltro,

                    IdUbicacionFiltro =
                        idUbicacionFiltro,

                    IdEstadoFiltro =
                        idEstadoFiltro,

                    IdModoFiltro =
                        idModoFiltro,

                    FechaDesde =
                        fechaDesde,

                    FechaHasta =
                        fechaHasta,

                    Activos =
                        activos,

                    TotalResultados =
                        totalResultados,

                    TotalFuncionales =
                        totalFuncionales,

                    TotalDanados =
                        totalDanados,

                    TotalMantenimiento =
                        totalMantenimiento,

                    PaginaActual =
                        pagina,

                    TamanoPagina =
                        tamanoPagina,

                    TotalPaginas =
                        totalPaginas
                };


            await CargarListasInventario(
                model);


            return View(model);
        }


        // =========================================
        // GENERAR PDF INVENTARIO GENERAL
        // =========================================
        [HttpGet]
        public async Task<IActionResult> GenerarInventarioGeneralPdf(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            string? serieFiltro,
            int? idUbicacionFiltro,
            int? idEstadoFiltro,
            int? idModoFiltro,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            // =====================================
            // VALIDAR FECHAS
            // =====================================
            if (fechaDesde.HasValue &&
                fechaHasta.HasValue &&
                fechaDesde.Value.Date >
                fechaHasta.Value.Date)
            {
                return BadRequest(
                    "La fecha inicial no puede ser posterior a la fecha final.");
            }


            // =====================================
            // CONSULTA COMPLETA
            // =====================================
            var consulta =
                ConstruirConsultaInventario(
                    codigoFiltro,
                    descripcionFiltro,
                    idMarcaFiltro,
                    serieFiltro,
                    idUbicacionFiltro,
                    idEstadoFiltro,
                    idModoFiltro,
                    fechaDesde,
                    fechaHasta);


            var activos =
                await consulta
                    .OrderBy(a =>
                        a.NumeroIdentificacion)
                    .ToListAsync();


            if (!activos.Any())
            {
                TempData["ErrorReporte"] =
                    "No existen activos para los filtros seleccionados.";

                return RedirectToAction(
                    nameof(InventarioGeneral));
            }


            // =====================================
            // DATOS GENERALES
            // =====================================
            string usuario =
                User.FindFirst(
                    ClaimTypes.Name)?.Value
                ?? "Usuario";


            DateTime fechaGeneracion =
                DateTime.Now;


            int totalFuncionales =
                activos.Count(a =>
                    a.EstadoActivo?.Estado ==
                    "Funcional");


            int totalDanados =
                activos.Count(a =>
                    a.EstadoActivo?.Estado ==
                    "Dañado");


            int totalMantenimiento =
                activos.Count(a =>
                    a.EstadoActivo?.Estado ==
                    "En mantenimiento");


            // =====================================
            // ESTILOS DE CELDAS
            // =====================================
            IContainer CeldaEncabezado(
                IContainer container)
            {
                return container
                    .Background(
                        Colors.Blue.Darken3)
                    .Border(0.5f)
                    .BorderColor(
                        Colors.Grey.Medium)
                    .Padding(5)
                    .AlignMiddle();
            }


            IContainer CeldaContenido(
                IContainer container)
            {
                return container
                    .BorderBottom(0.5f)
                    .BorderColor(
                        Colors.Grey.Lighten1)
                    .Padding(4)
                    .AlignMiddle();
            }


            // =====================================
            // CREAR PDF
            // =====================================
            var documento =
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(
                            PageSizes.A4
                                .Landscape());

                        page.Margin(25);

                        page.DefaultTextStyle(
                            text =>
                                text.FontSize(8));


                        // =================================
                        // ENCABEZADO
                        // =================================
                        page.Header()
                            .Column(column =>
                            {
                                column.Spacing(3);


                                column.Item()
                                    .Text(
                                        "ESCUELA PROYECTO PACUARE")
                                    .FontSize(16)
                                    .Bold()
                                    .FontColor(
                                        Colors.Blue
                                            .Darken3);


                                column.Item()
                                    .Text(
                                        "Sistema de Inventario Institucional")
                                    .FontSize(10)
                                    .FontColor(
                                        Colors.Grey
                                            .Darken2);


                                column.Item()
                                    .PaddingTop(7)
                                    .Text(
                                        "REPORTE DE INVENTARIO GENERAL")
                                    .FontSize(13)
                                    .Bold();


                                column.Item()
                                    .Text(
                                        $"Fecha de generación: " +
                                        $"{fechaGeneracion:dd/MM/yyyy HH:mm}");


                                column.Item()
                                    .Text(
                                        $"Generado por: {usuario}");


                                column.Item()
                                    .PaddingTop(5)
                                    .LineHorizontal(1)
                                    .LineColor(
                                        Colors.Grey
                                            .Lighten1);
                            });


                        // =================================
                        // CONTENIDO
                        // =================================
                        page.Content()
                            .PaddingVertical(10)
                            .Column(column =>
                            {
                                column.Spacing(10);


                                // =========================
                                // RESUMEN
                                // =========================
                                column.Item()
                                    .Row(row =>
                                    {
                                        row.RelativeItem()
                                            .Border(1)
                                            .BorderColor(
                                                Colors.Grey
                                                    .Lighten1)
                                            .Padding(8)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text(
                                                        activos.Count
                                                            .ToString())
                                                    .FontSize(15)
                                                    .Bold();

                                                c.Item()
                                                    .Text(
                                                        "Total activos")
                                                    .FontSize(8);
                                            });


                                        row.RelativeItem()
                                            .Border(1)
                                            .BorderColor(
                                                Colors.Grey
                                                    .Lighten1)
                                            .Padding(8)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text(
                                                        totalFuncionales
                                                            .ToString())
                                                    .FontSize(15)
                                                    .Bold();

                                                c.Item()
                                                    .Text(
                                                        "Funcionales")
                                                    .FontSize(8);
                                            });


                                        row.RelativeItem()
                                            .Border(1)
                                            .BorderColor(
                                                Colors.Grey
                                                    .Lighten1)
                                            .Padding(8)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text(
                                                        totalDanados
                                                            .ToString())
                                                    .FontSize(15)
                                                    .Bold();

                                                c.Item()
                                                    .Text(
                                                        "Dañados")
                                                    .FontSize(8);
                                            });


                                        row.RelativeItem()
                                            .Border(1)
                                            .BorderColor(
                                                Colors.Grey
                                                    .Lighten1)
                                            .Padding(8)
                                            .Column(c =>
                                            {
                                                c.Item()
                                                    .Text(
                                                        totalMantenimiento
                                                            .ToString())
                                                    .FontSize(15)
                                                    .Bold();

                                                c.Item()
                                                    .Text(
                                                        "En mantenimiento")
                                                    .FontSize(8);
                                            });
                                    });


                                // =========================
                                // TABLA
                                // =========================
                                column.Item()
                                    .Table(table =>
                                    {
                                        table.ColumnsDefinition(
                                            columns =>
                                            {
                                                columns
                                                    .RelativeColumn(
                                                        1.1f);

                                                columns
                                                    .RelativeColumn(
                                                        2.1f);

                                                columns
                                                    .RelativeColumn(
                                                        1.2f);

                                                columns
                                                    .RelativeColumn(
                                                        1.3f);

                                                columns
                                                    .RelativeColumn(
                                                        1.5f);

                                                columns
                                                    .RelativeColumn(
                                                        1.2f);

                                                columns
                                                    .RelativeColumn(
                                                        1.5f);

                                                columns
                                                    .RelativeColumn(
                                                        1.1f);
                                            });


                                        // =================
                                        // CABECERA
                                        // =================
                                        table.Header(header =>
                                        {
                                            header.Cell()
                                                .Element(
                                                    CeldaEncabezado)
                                                .Text("Código")
                                                .FontColor(
                                                    Colors.White)
                                                .Bold();


                                            header.Cell()
                                                .Element(
                                                    CeldaEncabezado)
                                                .Text(
                                                    "Descripción")
                                                .FontColor(
                                                    Colors.White)
                                                .Bold();


                                            header.Cell()
                                                .Element(
                                                    CeldaEncabezado)
                                                .Text("Marca")
                                                .FontColor(
                                                    Colors.White)
                                                .Bold();


                                            header.Cell()
                                                .Element(
                                                    CeldaEncabezado)
                                                .Text("Serie")
                                                .FontColor(
                                                    Colors.White)
                                                .Bold();


                                            header.Cell()
                                                .Element(
                                                    CeldaEncabezado)
                                                .Text(
                                                    "Ubicación")
                                                .FontColor(
                                                    Colors.White)
                                                .Bold();


                                            header.Cell()
                                                .Element(
                                                    CeldaEncabezado)
                                                .Text("Estado")
                                                .FontColor(
                                                    Colors.White)
                                                .Bold();


                                            header.Cell()
                                                .Element(
                                                    CeldaEncabezado)
                                                .Text(
                                                    "Adquisición")
                                                .FontColor(
                                                    Colors.White)
                                                .Bold();


                                            header.Cell()
                                                .Element(
                                                    CeldaEncabezado)
                                                .Text(
                                                    "Registro")
                                                .FontColor(
                                                    Colors.White)
                                                .Bold();
                                        });


                                        // =================
                                        // FILAS
                                        // =================
                                        foreach (
                                            var activo
                                            in activos)
                                        {
                                            table.Cell()
                                                .Element(
                                                    CeldaContenido)
                                                .Text(
                                                    activo
                                                        .NumeroIdentificacion);


                                            table.Cell()
                                                .Element(
                                                    CeldaContenido)
                                                .Text(
                                                    activo
                                                        .Descripcion);


                                            table.Cell()
                                                .Element(
                                                    CeldaContenido)
                                                .Text(
                                                    activo.Marca?
                                                        .MarcaNombre
                                                    ?? "N/I");


                                            table.Cell()
                                                .Element(
                                                    CeldaContenido)
                                                .Text(
                                                    string
                                                        .IsNullOrWhiteSpace(
                                                            activo.Serie)
                                                    ? "N/I"
                                                    : activo.Serie);


                                            table.Cell()
                                                .Element(
                                                    CeldaContenido)
                                                .Text(
                                                    activo.Ubicacion?
                                                        .UbicacionNombre
                                                    ?? "N/I");


                                            table.Cell()
                                                .Element(
                                                    CeldaContenido)
                                                .Text(
                                                    activo.EstadoActivo?
                                                        .Estado
                                                    ?? "N/I");


                                            table.Cell()
                                                .Element(
                                                    CeldaContenido)
                                                .Text(
                                                    activo
                                                        .ModoAdquisicion?
                                                        .NombreModoAdquisicion
                                                    ?? "N/I");


                                            table.Cell()
                                                .Element(
                                                    CeldaContenido)
                                                .Text(
                                                    activo
                                                        .FechaRegistro
                                                        .ToString(
                                                            "dd/MM/yyyy"));
                                        }
                                    });
                            });


                        // =================================
                        // PIE DE PÁGINA
                        // =================================
                        page.Footer()
                            .AlignCenter()
                            .Text(text =>
                            {
                                text.DefaultTextStyle(
                                    x =>
                                        x.FontSize(8)
                                            .FontColor(
                                                Colors.Grey
                                                    .Darken1));


                                text.Span(
                                    "Inventario Institucional - Página ");

                                text.CurrentPageNumber();

                                text.Span(" de ");

                                text.TotalPages();
                            });
                    });
                });


            byte[] pdf =
                documento.GeneratePdf();


            string nombreArchivo =
                $"InventarioGeneral_" +
                $"{DateTime.Now:yyyyMMdd_HHmm}.pdf";


            return File(
                pdf,
                "application/pdf",
                nombreArchivo);
        }


        // =========================================
        // CONSULTA REUTILIZABLE
        // =========================================
        private IQueryable<Activo>
            ConstruirConsultaInventario(
                string? codigoFiltro,
                string? descripcionFiltro,
                int? idMarcaFiltro,
                string? serieFiltro,
                int? idUbicacionFiltro,
                int? idEstadoFiltro,
                int? idModoFiltro,
                DateTime? fechaDesde,
                DateTime? fechaHasta)
        {
            var consulta =
                _context.Activos
                    .AsNoTracking()
                    .Include(a => a.Marca)
                    .Include(a => a.Ubicacion)
                    .Include(a => a.EstadoActivo)
                    .Include(a => a.ModoAdquisicion)
                    .AsQueryable();


            // CÓDIGO
            if (!string.IsNullOrWhiteSpace(
                codigoFiltro))
            {
                codigoFiltro =
                    codigoFiltro.Trim();

                consulta =
                    consulta.Where(a =>
                        a.NumeroIdentificacion
                            .Contains(
                                codigoFiltro));
            }


            // DESCRIPCIÓN
            if (!string.IsNullOrWhiteSpace(
                descripcionFiltro))
            {
                descripcionFiltro =
                    descripcionFiltro.Trim();

                consulta =
                    consulta.Where(a =>
                        a.Descripcion.Contains(
                            descripcionFiltro));
            }


            // MARCA
            if (idMarcaFiltro.HasValue)
            {
                consulta =
                    consulta.Where(a =>
                        a.IdMarca ==
                        idMarcaFiltro.Value);
            }


            // SERIE
            if (!string.IsNullOrWhiteSpace(
                serieFiltro))
            {
                serieFiltro =
                    serieFiltro.Trim();

                consulta =
                    consulta.Where(a =>
                        a.Serie != null &&
                        a.Serie.Contains(
                            serieFiltro));
            }


            // UBICACIÓN
            if (idUbicacionFiltro.HasValue)
            {
                consulta =
                    consulta.Where(a =>
                        a.IdUbicacion ==
                        idUbicacionFiltro.Value);
            }


            // ESTADO
            if (idEstadoFiltro.HasValue)
            {
                consulta =
                    consulta.Where(a =>
                        a.IdEstado ==
                        idEstadoFiltro.Value);
            }


            // MODO
            if (idModoFiltro.HasValue)
            {
                consulta =
                    consulta.Where(a =>
                        a.IdModo ==
                        idModoFiltro.Value);
            }


            // DESDE
            if (fechaDesde.HasValue)
            {
                DateTime inicio =
                    fechaDesde.Value.Date;

                consulta =
                    consulta.Where(a =>
                        a.FechaRegistro >= inicio);
            }


            // HASTA
            if (fechaHasta.HasValue)
            {
                DateTime fin =
                    fechaHasta.Value
                        .Date
                        .AddDays(1);

                consulta =
                    consulta.Where(a =>
                        a.FechaRegistro < fin);
            }


            return consulta;
        }


        // =========================================
        // LISTAS
        // =========================================
        private async Task CargarListasInventario(
            ReporteInventarioGeneralViewModel model)
        {
            model.Marcas =
                await _context.Marcas
                    .AsNoTracking()
                    .OrderBy(m =>
                        m.MarcaNombre)
                    .ToListAsync();


            model.Ubicaciones =
                await _context.Ubicaciones
                    .AsNoTracking()
                    .OrderBy(u =>
                        u.UbicacionNombre)
                    .ToListAsync();


            model.Estados =
                await _context.EstadosActivos
                    .AsNoTracking()
                    .OrderBy(e =>
                        e.Estado)
                    .ToListAsync();


            model.ModosAdquisicion =
                await _context.ModosAdquisicion
                    .AsNoTracking()
                    .OrderBy(m =>
                        m.NombreModoAdquisicion)
                    .ToListAsync();
        }

        // ============================================================
        // RECURSOS POR UBICACIÓN
        // ============================================================


        // =========================================
        // CONFIGURAR REPORTE
        // =========================================
        [HttpGet]
        public async Task<IActionResult> RecursosPorUbicacion(
            int? idUbicacionFiltro,
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            int? idEstadoFiltro,
            int? idModoFiltro)
        {
            var model =
                new ReporteRecursosPorUbicacionViewModel
                {
                    IdUbicacionFiltro =
                        idUbicacionFiltro,

                    CodigoFiltro =
                        codigoFiltro,

                    DescripcionFiltro =
                        descripcionFiltro,

                    IdMarcaFiltro =
                        idMarcaFiltro,

                    IdEstadoFiltro =
                        idEstadoFiltro,

                    IdModoFiltro =
                        idModoFiltro
                };


            await CargarListasRecursosUbicacion(
                model);


            return View(model);
        }


        // =========================================
        // VISTA PREVIA
        // =========================================
        [HttpGet]
        public async Task<IActionResult> RecursosPorUbicacionPreview(
            int? idUbicacionFiltro,
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            int? idEstadoFiltro,
            int? idModoFiltro,
            int pagina = 1)
        {
            const int tamanoPagina = 10;


            // =====================================
            // CONSULTA
            // =====================================
            var consulta =
                ConstruirConsultaRecursosUbicacion(
                    idUbicacionFiltro,
                    codigoFiltro,
                    descripcionFiltro,
                    idMarcaFiltro,
                    idEstadoFiltro,
                    idModoFiltro);


            // =====================================
            // TOTAL
            // =====================================
            int totalResultados =
                await consulta.CountAsync();


            // =====================================
            // TOTAL PÁGINAS
            // =====================================
            int totalPaginas =
                (int)Math.Ceiling(
                    totalResultados /
                    (double)tamanoPagina);


            if (pagina < 1)
            {
                pagina = 1;
            }


            if (totalPaginas > 0 &&
                pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }


            // =====================================
            // SOLO 10 PARA LA VISTA PREVIA
            // =====================================
            var activos =
                await consulta
                    .OrderBy(a =>
                        a.Ubicacion.UbicacionNombre)
                    .ThenBy(a =>
                        a.NumeroIdentificacion)
                    .Skip(
                        (pagina - 1) *
                        tamanoPagina)
                    .Take(tamanoPagina)
                    .ToListAsync();


            // =====================================
            // RESUMEN COMPLETO
            // =====================================
            int totalUbicaciones =
                await consulta
                    .Select(a => a.IdUbicacion)
                    .Distinct()
                    .CountAsync();


            int totalMarcas =
                await consulta
                    .Select(a => a.IdMarca)
                    .Distinct()
                    .CountAsync();


            int totalEstados =
                await consulta
                    .Select(a => a.IdEstado)
                    .Distinct()
                    .CountAsync();


            // =====================================
            // VIEWMODEL
            // =====================================
            var model =
                new ReporteRecursosPorUbicacionViewModel
                {
                    IdUbicacionFiltro =
                        idUbicacionFiltro,

                    CodigoFiltro =
                        codigoFiltro,

                    DescripcionFiltro =
                        descripcionFiltro,

                    IdMarcaFiltro =
                        idMarcaFiltro,

                    IdEstadoFiltro =
                        idEstadoFiltro,

                    IdModoFiltro =
                        idModoFiltro,

                    Activos =
                        activos,

                    TotalResultados =
                        totalResultados,

                    TotalUbicaciones =
                        totalUbicaciones,

                    TotalMarcas =
                        totalMarcas,

                    TotalEstados =
                        totalEstados,

                    PaginaActual =
                        pagina,

                    TamanoPagina =
                        tamanoPagina,

                    TotalPaginas =
                        totalPaginas
                };


            await CargarListasRecursosUbicacion(
                model);


            return View(model);
        }


        // =========================================
        // GENERAR PDF
        // =========================================
        [HttpGet]
        public async Task<IActionResult> GenerarRecursosPorUbicacionPdf(
            int? idUbicacionFiltro,
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            int? idEstadoFiltro,
            int? idModoFiltro)
        {
            // =====================================
            // CONSULTA COMPLETA
            // =====================================
            var consulta =
                ConstruirConsultaRecursosUbicacion(
                    idUbicacionFiltro,
                    codigoFiltro,
                    descripcionFiltro,
                    idMarcaFiltro,
                    idEstadoFiltro,
                    idModoFiltro);


            var activos =
                await consulta
                    .OrderBy(a =>
                        a.Ubicacion.UbicacionNombre)
                    .ThenBy(a =>
                        a.NumeroIdentificacion)
                    .ToListAsync();


            if (!activos.Any())
            {
                TempData["ErrorReporte"] =
                    "No existen activos para los filtros seleccionados.";

                return RedirectToAction(
                    nameof(RecursosPorUbicacion));
            }


            // =====================================
            // DATOS GENERALES
            // =====================================
            string usuario =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.Name)?
                    .Value
                ?? "Usuario";


            DateTime fechaGeneracion =
                DateTime.Now;


            int totalUbicaciones =
                activos
                    .Select(a => a.IdUbicacion)
                    .Distinct()
                    .Count();


            // =====================================
            // AGRUPAR POR UBICACIÓN
            // =====================================
            var grupos =
                activos
                    .GroupBy(a =>
                        a.Ubicacion?.UbicacionNombre
                        ?? "Sin ubicación")
                    .OrderBy(g => g.Key)
                    .ToList();


            // =====================================
            // ESTILOS
            // =====================================
            IContainer CeldaEncabezado(
                IContainer container)
            {
                return container
                    .Background(
                        QuestPDF.Helpers.Colors
                            .Blue.Darken3)
                    .Border(0.5f)
                    .BorderColor(
                        QuestPDF.Helpers.Colors
                            .Grey.Medium)
                    .Padding(4)
                    .AlignMiddle();
            }


            IContainer CeldaContenido(
                IContainer container)
            {
                return container
                    .BorderBottom(0.5f)
                    .BorderColor(
                        QuestPDF.Helpers.Colors
                            .Grey.Lighten1)
                    .Padding(4)
                    .AlignMiddle();
            }


            // =====================================
            // DOCUMENTO
            // =====================================
            var documento =
                QuestPDF.Fluent.Document.Create(
                    container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(
                                QuestPDF.Helpers.PageSizes
                                    .A4.Landscape());


                            page.Margin(25);


                            page.DefaultTextStyle(
                                text =>
                                    text.FontSize(8));


                            // =============================
                            // ENCABEZADO
                            // =============================
                            page.Header()
                                .Column(column =>
                                {
                                    column.Spacing(3);


                                    column.Item()
                                        .Text(
                                            "ESCUELA PROYECTO PACUARE")
                                        .FontSize(16)
                                        .Bold()
                                        .FontColor(
                                            QuestPDF.Helpers.Colors
                                                .Blue.Darken3);


                                    column.Item()
                                        .Text(
                                            "Sistema de Inventario Institucional")
                                        .FontSize(10)
                                        .FontColor(
                                            QuestPDF.Helpers.Colors
                                                .Grey.Darken2);


                                    column.Item()
                                        .PaddingTop(7)
                                        .Text(
                                            "REPORTE DE RECURSOS POR UBICACIÓN")
                                        .FontSize(13)
                                        .Bold();


                                    column.Item()
                                        .Text(
                                            $"Fecha de generación: " +
                                            $"{fechaGeneracion:dd/MM/yyyy HH:mm}");


                                    column.Item()
                                        .Text(
                                            $"Generado por: {usuario}");


                                    column.Item()
                                        .PaddingTop(5)
                                        .LineHorizontal(1)
                                        .LineColor(
                                            QuestPDF.Helpers.Colors
                                                .Grey.Lighten1);
                                });


                            // =============================
                            // CONTENIDO
                            // =============================
                            page.Content()
                                .PaddingVertical(10)
                                .Column(column =>
                                {
                                    column.Spacing(12);


                                    // =====================
                                    // RESUMEN
                                    // =====================
                                    column.Item()
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Border(1)
                                                .BorderColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Lighten1)
                                                .Padding(8)
                                                .Column(c =>
                                                {
                                                    c.Item()
                                                        .Text(
                                                            activos.Count
                                                                .ToString())
                                                        .FontSize(15)
                                                        .Bold();

                                                    c.Item()
                                                        .Text(
                                                            "Activos incluidos");
                                                });


                                            row.RelativeItem()
                                                .Border(1)
                                                .BorderColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Lighten1)
                                                .Padding(8)
                                                .Column(c =>
                                                {
                                                    c.Item()
                                                        .Text(
                                                            totalUbicaciones
                                                                .ToString())
                                                        .FontSize(15)
                                                        .Bold();

                                                    c.Item()
                                                        .Text(
                                                            "Ubicaciones");
                                                });
                                        });


                                    // =====================
                                    // CADA UBICACIÓN
                                    // =====================
                                    foreach (var grupo
                                        in grupos)
                                    {
                                        column.Item()
                                            .PaddingTop(5)
                                            .Background(
                                                QuestPDF.Helpers
                                                    .Colors.Grey
                                                    .Lighten3)
                                            .Padding(7)
                                            .Row(row =>
                                            {
                                                row.RelativeItem()
                                                    .Text(
                                                        grupo.Key)
                                                    .FontSize(11)
                                                    .Bold();


                                                row.AutoItem()
                                                    .Text(
                                                        $"{grupo.Count()} activos")
                                                    .FontSize(9);
                                            });


                                        column.Item()
                                            .Table(table =>
                                            {
                                                table.ColumnsDefinition(
                                                    columns =>
                                                    {
                                                        columns
                                                            .RelativeColumn(
                                                                1.1f);

                                                        columns
                                                            .RelativeColumn(
                                                                2.2f);

                                                        columns
                                                            .RelativeColumn(
                                                                1.2f);

                                                        columns
                                                            .RelativeColumn(
                                                                1.3f);

                                                        columns
                                                            .RelativeColumn(
                                                                1.3f);

                                                        columns
                                                            .RelativeColumn(
                                                                1.2f);

                                                        columns
                                                            .RelativeColumn(
                                                                1.5f);
                                                    });


                                                // =================
                                                // CABECERA
                                                // =================
                                                table.Header(header =>
                                                {
                                                    header.Cell()
                                                        .Element(
                                                            CeldaEncabezado)
                                                        .Text("Código")
                                                        .FontColor(
                                                            QuestPDF.Helpers
                                                                .Colors.White)
                                                        .Bold();


                                                    header.Cell()
                                                        .Element(
                                                            CeldaEncabezado)
                                                        .Text("Descripción")
                                                        .FontColor(
                                                            QuestPDF.Helpers
                                                                .Colors.White)
                                                        .Bold();


                                                    header.Cell()
                                                        .Element(
                                                            CeldaEncabezado)
                                                        .Text("Marca")
                                                        .FontColor(
                                                            QuestPDF.Helpers
                                                                .Colors.White)
                                                        .Bold();


                                                    header.Cell()
                                                        .Element(
                                                            CeldaEncabezado)
                                                        .Text("Modelo")
                                                        .FontColor(
                                                            QuestPDF.Helpers
                                                                .Colors.White)
                                                        .Bold();


                                                    header.Cell()
                                                        .Element(
                                                            CeldaEncabezado)
                                                        .Text("Serie")
                                                        .FontColor(
                                                            QuestPDF.Helpers
                                                                .Colors.White)
                                                        .Bold();


                                                    header.Cell()
                                                        .Element(
                                                            CeldaEncabezado)
                                                        .Text("Estado")
                                                        .FontColor(
                                                            QuestPDF.Helpers
                                                                .Colors.White)
                                                        .Bold();


                                                    header.Cell()
                                                        .Element(
                                                            CeldaEncabezado)
                                                        .Text("Adquisición")
                                                        .FontColor(
                                                            QuestPDF.Helpers
                                                                .Colors.White)
                                                        .Bold();
                                                });


                                                // =================
                                                // FILAS
                                                // =================
                                                foreach (
                                                    var activo
                                                    in grupo)
                                                {
                                                    table.Cell()
                                                        .Element(
                                                            CeldaContenido)
                                                        .Text(
                                                            activo
                                                                .NumeroIdentificacion);


                                                    table.Cell()
                                                        .Element(
                                                            CeldaContenido)
                                                        .Text(
                                                            activo
                                                                .Descripcion);


                                                    table.Cell()
                                                        .Element(
                                                            CeldaContenido)
                                                        .Text(
                                                            activo.Marca?
                                                                .MarcaNombre
                                                            ?? "N/I");


                                                    table.Cell()
                                                        .Element(
                                                            CeldaContenido)
                                                        .Text(
                                                            string.IsNullOrWhiteSpace(
                                                                activo.Modelo)
                                                            ? "N/I"
                                                            : activo.Modelo);


                                                    table.Cell()
                                                        .Element(
                                                            CeldaContenido)
                                                        .Text(
                                                            string.IsNullOrWhiteSpace(
                                                                activo.Serie)
                                                            ? "N/I"
                                                            : activo.Serie);


                                                    table.Cell()
                                                        .Element(
                                                            CeldaContenido)
                                                        .Text(
                                                            activo.EstadoActivo?
                                                                .Estado
                                                            ?? "N/I");


                                                    table.Cell()
                                                        .Element(
                                                            CeldaContenido)
                                                        .Text(
                                                            activo.ModoAdquisicion?
                                                                .NombreModoAdquisicion
                                                            ?? "N/I");
                                                }
                                            });
                                    }
                                });


                            // =============================
                            // FOOTER
                            // =============================
                            page.Footer()
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.DefaultTextStyle(
                                        x =>
                                            x.FontSize(8)
                                                .FontColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Darken1));


                                    text.Span(
                                        "Inventario Institucional - Página ");

                                    text.CurrentPageNumber();

                                    text.Span(" de ");

                                    text.TotalPages();
                                });
                        });
                    });


            byte[] pdf =
                documento.GeneratePdf();


            string nombreArchivo =
                $"RecursosPorUbicacion_" +
                $"{DateTime.Now:yyyyMMdd_HHmm}.pdf";


            return File(
                pdf,
                "application/pdf",
                nombreArchivo);
        }


        // =========================================
        // CONSULTA REUTILIZABLE
        // =========================================
        private IQueryable<Activo>
            ConstruirConsultaRecursosUbicacion(
                int? idUbicacionFiltro,
                string? codigoFiltro,
                string? descripcionFiltro,
                int? idMarcaFiltro,
                int? idEstadoFiltro,
                int? idModoFiltro)
        {
            var consulta =
                _context.Activos
                    .AsNoTracking()
                    .Include(a => a.Marca)
                    .Include(a => a.Ubicacion)
                    .Include(a => a.EstadoActivo)
                    .Include(a => a.ModoAdquisicion)
                    .AsQueryable();


            // =====================================
            // UBICACIÓN
            // =====================================
            if (idUbicacionFiltro.HasValue)
            {
                consulta =
                    consulta.Where(a =>
                        a.IdUbicacion ==
                        idUbicacionFiltro.Value);
            }


            // =====================================
            // CÓDIGO
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                codigoFiltro))
            {
                codigoFiltro =
                    codigoFiltro.Trim();

                consulta =
                    consulta.Where(a =>
                        a.NumeroIdentificacion
                            .Contains(codigoFiltro));
            }


            // =====================================
            // DESCRIPCIÓN
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                descripcionFiltro))
            {
                descripcionFiltro =
                    descripcionFiltro.Trim();

                consulta =
                    consulta.Where(a =>
                        a.Descripcion.Contains(
                            descripcionFiltro));
            }


            // =====================================
            // MARCA
            // =====================================
            if (idMarcaFiltro.HasValue)
            {
                consulta =
                    consulta.Where(a =>
                        a.IdMarca ==
                        idMarcaFiltro.Value);
            }


            // =====================================
            // ESTADO
            // =====================================
            if (idEstadoFiltro.HasValue)
            {
                consulta =
                    consulta.Where(a =>
                        a.IdEstado ==
                        idEstadoFiltro.Value);
            }


            // =====================================
            // MODO DE ADQUISICIÓN
            // =====================================
            if (idModoFiltro.HasValue)
            {
                consulta =
                    consulta.Where(a =>
                        a.IdModo ==
                        idModoFiltro.Value);
            }


            return consulta;
        }


        // =========================================
        // CARGAR LISTAS
        // =========================================
        private async Task
            CargarListasRecursosUbicacion(
                ReporteRecursosPorUbicacionViewModel model)
        {
            model.Ubicaciones =
                await _context.Ubicaciones
                    .AsNoTracking()
                    .OrderBy(u =>
                        u.UbicacionNombre)
                    .ToListAsync();


            model.Marcas =
                await _context.Marcas
                    .AsNoTracking()
                    .OrderBy(m =>
                        m.MarcaNombre)
                    .ToListAsync();


            model.Estados =
                await _context.EstadosActivos
                    .AsNoTracking()
                    .OrderBy(e =>
                        e.Estado)
                    .ToListAsync();


            model.ModosAdquisicion =
                await _context.ModosAdquisicion
                    .AsNoTracking()
                    .OrderBy(m =>
                        m.NombreModoAdquisicion)
                    .ToListAsync();
        }

        // ============================================================
        // REPORTE: EQUIPOS DAÑADOS
        // ============================================================


        // =========================================
        // CONFIGURAR REPORTE
        // =========================================
        [HttpGet]
        public async Task<IActionResult> EquiposDanados(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            string? modeloFiltro,
            string? serieFiltro,
            int? idUbicacionFiltro,
            int? idModoFiltro)
        {
            var model =
                new ReporteEquiposDanadosViewModel
                {
                    CodigoFiltro =
                        codigoFiltro,

                    DescripcionFiltro =
                        descripcionFiltro,

                    IdMarcaFiltro =
                        idMarcaFiltro,

                    ModeloFiltro =
                        modeloFiltro,

                    SerieFiltro =
                        serieFiltro,

                    IdUbicacionFiltro =
                        idUbicacionFiltro,

                    IdModoFiltro =
                        idModoFiltro
                };


            await CargarListasEquiposDanados(
                model);


            return View(model);
        }


        // =========================================
        // VISTA PREVIA
        // =========================================
        [HttpGet]
        public async Task<IActionResult> EquiposDanadosPreview(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            string? modeloFiltro,
            string? serieFiltro,
            int? idUbicacionFiltro,
            int? idModoFiltro,
            int pagina = 1)
        {
            const int tamanoPagina = 10;


            // =====================================
            // CONSULTA
            // =====================================
            var consulta =
                ConstruirConsultaEquiposDanados(
                    codigoFiltro,
                    descripcionFiltro,
                    idMarcaFiltro,
                    modeloFiltro,
                    serieFiltro,
                    idUbicacionFiltro,
                    idModoFiltro);


            // =====================================
            // TOTAL RESULTADOS
            // =====================================
            int totalResultados =
                await consulta.CountAsync();


            // =====================================
            // TOTAL PÁGINAS
            // =====================================
            int totalPaginas =
                (int)Math.Ceiling(
                    totalResultados /
                    (double)tamanoPagina);


            if (pagina < 1)
            {
                pagina = 1;
            }


            if (totalPaginas > 0 &&
                pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }


            // =====================================
            // SOLO 10 PARA LA VISTA PREVIA
            // =====================================
            var activos =
                await consulta
                    .OrderBy(a =>
                        a.NumeroIdentificacion)
                    .Skip(
                        (pagina - 1) *
                        tamanoPagina)
                    .Take(tamanoPagina)
                    .ToListAsync();


            // =====================================
            // RESUMEN COMPLETO
            // =====================================
            int totalUbicaciones =
                await consulta
                    .Select(a =>
                        a.IdUbicacion)
                    .Distinct()
                    .CountAsync();


            int totalMarcas =
                await consulta
                    .Select(a =>
                        a.IdMarca)
                    .Distinct()
                    .CountAsync();


            int totalConObservacion =
                await consulta
                    .CountAsync(a =>
                        a.Observacion != null &&
                        a.Observacion != "");


            // =====================================
            // VIEWMODEL
            // =====================================
            var model =
                new ReporteEquiposDanadosViewModel
                {
                    CodigoFiltro =
                        codigoFiltro,

                    DescripcionFiltro =
                        descripcionFiltro,

                    IdMarcaFiltro =
                        idMarcaFiltro,

                    ModeloFiltro =
                        modeloFiltro,

                    SerieFiltro =
                        serieFiltro,

                    IdUbicacionFiltro =
                        idUbicacionFiltro,

                    IdModoFiltro =
                        idModoFiltro,

                    Activos =
                        activos,

                    TotalResultados =
                        totalResultados,

                    TotalUbicaciones =
                        totalUbicaciones,

                    TotalMarcas =
                        totalMarcas,

                    TotalConObservacion =
                        totalConObservacion,

                    PaginaActual =
                        pagina,

                    TamanoPagina =
                        tamanoPagina,

                    TotalPaginas =
                        totalPaginas
                };


            await CargarListasEquiposDanados(
                model);


            return View(model);
        }


        // =========================================
        // GENERAR PDF
        // =========================================
        [HttpGet]
        public async Task<IActionResult> GenerarEquiposDanadosPdf(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            string? modeloFiltro,
            string? serieFiltro,
            int? idUbicacionFiltro,
            int? idModoFiltro)
        {
            // =====================================
            // CONSULTA COMPLETA
            // =====================================
            var consulta =
                ConstruirConsultaEquiposDanados(
                    codigoFiltro,
                    descripcionFiltro,
                    idMarcaFiltro,
                    modeloFiltro,
                    serieFiltro,
                    idUbicacionFiltro,
                    idModoFiltro);


            var activos =
                await consulta
                    .OrderBy(a =>
                        a.Ubicacion.UbicacionNombre)
                    .ThenBy(a =>
                        a.NumeroIdentificacion)
                    .ToListAsync();


            if (!activos.Any())
            {
                TempData["ErrorReporte"] =
                    "No existen equipos dañados para los filtros seleccionados.";

                return RedirectToAction(
                    nameof(EquiposDanados));
            }


            // =====================================
            // DATOS GENERALES
            // =====================================
            string usuario =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.Name)?
                    .Value
                ?? "Usuario";


            DateTime fechaGeneracion =
                DateTime.Now;


            int totalUbicaciones =
                activos
                    .Select(a =>
                        a.IdUbicacion)
                    .Distinct()
                    .Count();


            int totalConObservacion =
                activos.Count(a =>
                    !string.IsNullOrWhiteSpace(
                        a.Observacion));


            // =====================================
            // ESTILOS
            // =====================================
            IContainer CeldaEncabezado(
                IContainer container)
            {
                return container
                    .Background(
                        QuestPDF.Helpers
                            .Colors.Red.Darken2)
                    .Border(0.5f)
                    .BorderColor(
                        QuestPDF.Helpers
                            .Colors.Grey.Medium)
                    .Padding(4)
                    .AlignMiddle();
            }


            IContainer CeldaContenido(
                IContainer container)
            {
                return container
                    .BorderBottom(0.5f)
                    .BorderColor(
                        QuestPDF.Helpers
                            .Colors.Grey.Lighten1)
                    .Padding(4)
                    .AlignMiddle();
            }


            // =====================================
            // DOCUMENTO
            // =====================================
            var documento =
                QuestPDF.Fluent.Document.Create(
                    container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(
                                QuestPDF.Helpers.PageSizes
                                    .A4.Landscape());


                            page.Margin(25);


                            page.DefaultTextStyle(
                                text =>
                                    text.FontSize(8));


                            // =============================
                            // ENCABEZADO
                            // =============================
                            page.Header()
                                .Column(column =>
                                {
                                    column.Spacing(3);


                                    column.Item()
                                        .Text(
                                            "ESCUELA PROYECTO PACUARE")
                                        .FontSize(16)
                                        .Bold()
                                        .FontColor(
                                            QuestPDF.Helpers
                                                .Colors.Blue.Darken3);


                                    column.Item()
                                        .Text(
                                            "Sistema de Inventario Institucional")
                                        .FontSize(10)
                                        .FontColor(
                                            QuestPDF.Helpers
                                                .Colors.Grey.Darken2);


                                    column.Item()
                                        .PaddingTop(7)
                                        .Text(
                                            "REPORTE DE EQUIPOS DAÑADOS")
                                        .FontSize(13)
                                        .Bold();


                                    column.Item()
                                        .Text(
                                            $"Fecha de generación: " +
                                            $"{fechaGeneracion:dd/MM/yyyy HH:mm}");


                                    column.Item()
                                        .Text(
                                            $"Generado por: {usuario}");


                                    column.Item()
                                        .PaddingTop(5)
                                        .LineHorizontal(1)
                                        .LineColor(
                                            QuestPDF.Helpers
                                                .Colors.Grey.Lighten1);
                                });


                            // =============================
                            // CONTENIDO
                            // =============================
                            page.Content()
                                .PaddingVertical(10)
                                .Column(column =>
                                {
                                    column.Spacing(10);


                                    // =====================
                                    // RESUMEN
                                    // =====================
                                    column.Item()
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Border(1)
                                                .BorderColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Lighten1)
                                                .Padding(8)
                                                .Column(c =>
                                                {
                                                    c.Item()
                                                        .Text(
                                                            activos.Count
                                                                .ToString())
                                                        .FontSize(15)
                                                        .Bold();

                                                    c.Item()
                                                        .Text(
                                                            "Equipos dañados");
                                                });


                                            row.RelativeItem()
                                                .Border(1)
                                                .BorderColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Lighten1)
                                                .Padding(8)
                                                .Column(c =>
                                                {
                                                    c.Item()
                                                        .Text(
                                                            totalUbicaciones
                                                                .ToString())
                                                        .FontSize(15)
                                                        .Bold();

                                                    c.Item()
                                                        .Text(
                                                            "Ubicaciones afectadas");
                                                });


                                            row.RelativeItem()
                                                .Border(1)
                                                .BorderColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Lighten1)
                                                .Padding(8)
                                                .Column(c =>
                                                {
                                                    c.Item()
                                                        .Text(
                                                            totalConObservacion
                                                                .ToString())
                                                        .FontSize(15)
                                                        .Bold();

                                                    c.Item()
                                                        .Text(
                                                            "Con observación");
                                                });
                                        });


                                    // =====================
                                    // TABLA
                                    // =====================
                                    column.Item()
                                        .Table(table =>
                                        {
                                            table.ColumnsDefinition(
                                                columns =>
                                                {
                                                    columns
                                                        .RelativeColumn(
                                                            1.0f);

                                                    columns
                                                        .RelativeColumn(
                                                            2.0f);

                                                    columns
                                                        .RelativeColumn(
                                                            1.1f);

                                                    columns
                                                        .RelativeColumn(
                                                            1.2f);

                                                    columns
                                                        .RelativeColumn(
                                                            1.2f);

                                                    columns
                                                        .RelativeColumn(
                                                            1.5f);

                                                    columns
                                                        .RelativeColumn(
                                                            1.0f);

                                                    columns
                                                        .RelativeColumn(
                                                            2.0f);
                                                });


                                            // =================
                                            // CABECERA
                                            // =================
                                            table.Header(header =>
                                            {
                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Código")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Descripción")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Marca")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Modelo")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Serie")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Ubicación")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Estado")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Observación")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();
                                            });


                                            // =================
                                            // FILAS
                                            // =================
                                            foreach (
                                                var activo
                                                in activos)
                                            {
                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        activo
                                                            .NumeroIdentificacion);


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        activo
                                                            .Descripcion);


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        activo.Marca?
                                                            .MarcaNombre
                                                        ?? "N/I");


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        string.IsNullOrWhiteSpace(
                                                            activo.Modelo)
                                                        ? "N/I"
                                                        : activo.Modelo);


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        string.IsNullOrWhiteSpace(
                                                            activo.Serie)
                                                        ? "N/I"
                                                        : activo.Serie);


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        activo.Ubicacion?
                                                            .UbicacionNombre
                                                        ?? "N/I");


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        activo.EstadoActivo?
                                                            .Estado
                                                        ?? "N/I");


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        string.IsNullOrWhiteSpace(
                                                            activo.Observacion)
                                                        ? "Sin observación"
                                                        : activo.Observacion);
                                            }
                                        });
                                });


                            // =============================
                            // PIE
                            // =============================
                            page.Footer()
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.DefaultTextStyle(
                                        x =>
                                            x.FontSize(8)
                                                .FontColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Darken1));


                                    text.Span(
                                        "Inventario Institucional - Página ");

                                    text.CurrentPageNumber();

                                    text.Span(" de ");

                                    text.TotalPages();
                                });
                        });
                    });


            byte[] pdf =
                documento.GeneratePdf();


            string nombreArchivo =
                $"EquiposDanados_" +
                $"{DateTime.Now:yyyyMMdd_HHmm}.pdf";


            return File(
                pdf,
                "application/pdf",
                nombreArchivo);
        }


        // =========================================
        // CONSULTA REUTILIZABLE
        // =========================================
        private IQueryable<Activo>
            ConstruirConsultaEquiposDanados(
                string? codigoFiltro,
                string? descripcionFiltro,
                int? idMarcaFiltro,
                string? modeloFiltro,
                string? serieFiltro,
                int? idUbicacionFiltro,
                int? idModoFiltro)
        {
            var consulta =
                _context.Activos
                    .AsNoTracking()
                    .Include(a => a.Marca)
                    .Include(a => a.Ubicacion)
                    .Include(a => a.EstadoActivo)
                    .Include(a => a.ModoAdquisicion)
                    .AsQueryable();


            // =====================================
            // SOLO EQUIPOS DAÑADOS
            //
            // Se contemplan ambos nombres porque
            // el inventario real utiliza "Malo"
            // y el sistema también ha manejado
            // "Dañado".
            // =====================================
            consulta =
                consulta.Where(a =>
                    a.EstadoActivo.Estado == "Dañado" ||
                    a.EstadoActivo.Estado == "Malo");


            // =====================================
            // CÓDIGO
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                codigoFiltro))
            {
                codigoFiltro =
                    codigoFiltro.Trim();

                consulta =
                    consulta.Where(a =>
                        a.NumeroIdentificacion
                            .Contains(codigoFiltro));
            }


            // =====================================
            // DESCRIPCIÓN
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                descripcionFiltro))
            {
                descripcionFiltro =
                    descripcionFiltro.Trim();

                consulta =
                    consulta.Where(a =>
                        a.Descripcion
                            .Contains(descripcionFiltro));
            }


            // =====================================
            // MARCA
            // =====================================
            if (idMarcaFiltro.HasValue)
            {
                consulta =
                    consulta.Where(a =>
                        a.IdMarca ==
                        idMarcaFiltro.Value);
            }


            // =====================================
            // MODELO
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                modeloFiltro))
            {
                modeloFiltro =
                    modeloFiltro.Trim();

                consulta =
                    consulta.Where(a =>
                        a.Modelo != null &&
                        a.Modelo.Contains(
                            modeloFiltro));
            }


            // =====================================
            // SERIE
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                serieFiltro))
            {
                serieFiltro =
                    serieFiltro.Trim();

                consulta =
                    consulta.Where(a =>
                        a.Serie != null &&
                        a.Serie.Contains(
                            serieFiltro));
            }


            // =====================================
            // UBICACIÓN
            // =====================================
            if (idUbicacionFiltro.HasValue)
            {
                consulta =
                    consulta.Where(a =>
                        a.IdUbicacion ==
                        idUbicacionFiltro.Value);
            }


            // =====================================
            // MODO ADQUISICIÓN
            // =====================================
            if (idModoFiltro.HasValue)
            {
                consulta =
                    consulta.Where(a =>
                        a.IdModo ==
                        idModoFiltro.Value);
            }


            return consulta;
        }


        // =========================================
        // CARGAR LISTAS
        // =========================================
        private async Task
            CargarListasEquiposDanados(
                ReporteEquiposDanadosViewModel model)
        {
            model.Marcas =
                await _context.Marcas
                    .AsNoTracking()
                    .OrderBy(m =>
                        m.MarcaNombre)
                    .ToListAsync();


            model.Ubicaciones =
                await _context.Ubicaciones
                    .AsNoTracking()
                    .OrderBy(u =>
                        u.UbicacionNombre)
                    .ToListAsync();


            model.ModosAdquisicion =
                await _context.ModosAdquisicion
                    .AsNoTracking()
                    .OrderBy(m =>
                        m.NombreModoAdquisicion)
                    .ToListAsync();
        }

        // ============================================================
        // REPORTE: EQUIPOS EN MANTENIMIENTO
        // ============================================================


        // =========================================
        // CONFIGURAR REPORTE
        // =========================================
        [HttpGet]
        public async Task<IActionResult> EquiposMantenimiento(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            string? modeloFiltro,
            string? serieFiltro,
            int? idUbicacionFiltro,
            int? idTipoFiltro,
            string? responsableFiltro)
        {
            var model =
                new ReporteEquiposMantenimientoViewModel
                {
                    CodigoFiltro =
                        codigoFiltro,

                    DescripcionFiltro =
                        descripcionFiltro,

                    IdMarcaFiltro =
                        idMarcaFiltro,

                    ModeloFiltro =
                        modeloFiltro,

                    SerieFiltro =
                        serieFiltro,

                    IdUbicacionFiltro =
                        idUbicacionFiltro,

                    IdTipoFiltro =
                        idTipoFiltro,

                    ResponsableFiltro =
                        responsableFiltro
                };


            await CargarListasEquiposMantenimiento(
                model);


            return View(model);
        }


        // =========================================
        // VISTA PREVIA
        // =========================================
        [HttpGet]
        public async Task<IActionResult> EquiposMantenimientoPreview(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            string? modeloFiltro,
            string? serieFiltro,
            int? idUbicacionFiltro,
            int? idTipoFiltro,
            string? responsableFiltro,
            int pagina = 1)
        {
            const int tamanoPagina = 10;


            // =====================================
            // CONSULTA
            // =====================================
            var consulta =
                ConstruirConsultaEquiposMantenimiento(
                    codigoFiltro,
                    descripcionFiltro,
                    idMarcaFiltro,
                    modeloFiltro,
                    serieFiltro,
                    idUbicacionFiltro,
                    idTipoFiltro,
                    responsableFiltro);


            // =====================================
            // TOTAL DE ACTIVOS
            // =====================================
            int totalResultados =
                await consulta.CountAsync();


            // =====================================
            // TOTAL DE PÁGINAS
            // =====================================
            int totalPaginas =
                (int)Math.Ceiling(
                    totalResultados /
                    (double)tamanoPagina);


            if (pagina < 1)
            {
                pagina = 1;
            }


            if (totalPaginas > 0 &&
                pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }


            // =====================================
            // 10 REGISTROS POR PÁGINA
            // =====================================
            var mantenimientos =
                await consulta
                    .OrderByDescending(m =>
                        m.FechaMantenimiento)
                    .ThenByDescending(m =>
                        m.IdMantenimiento)
                    .Skip(
                        (pagina - 1) *
                        tamanoPagina)
                    .Take(tamanoPagina)
                    .ToListAsync();


            // =====================================
            // UBICACIONES
            // =====================================
            int totalUbicaciones =
                await consulta
                    .Select(m =>
                        m.Activo.IdUbicacion)
                    .Distinct()
                    .CountAsync();


            // =====================================
            // TIPOS
            // =====================================
            int totalTipos =
                await consulta
                    .Select(m =>
                        m.IdTipo)
                    .Distinct()
                    .CountAsync();


            // =====================================
            // RESPONSABLES
            // =====================================
            int totalResponsables =
                await consulta
                    .Where(m =>
                        m.Responsable != null &&
                        m.Responsable != "")
                    .Select(m =>
                        m.Responsable)
                    .Distinct()
                    .CountAsync();


            // =====================================
            // VIEWMODEL
            // =====================================
            var model =
                new ReporteEquiposMantenimientoViewModel
                {
                    CodigoFiltro =
                        codigoFiltro,

                    DescripcionFiltro =
                        descripcionFiltro,

                    IdMarcaFiltro =
                        idMarcaFiltro,

                    ModeloFiltro =
                        modeloFiltro,

                    SerieFiltro =
                        serieFiltro,

                    IdUbicacionFiltro =
                        idUbicacionFiltro,

                    IdTipoFiltro =
                        idTipoFiltro,

                    ResponsableFiltro =
                        responsableFiltro,

                    Mantenimientos =
                        mantenimientos,

                    TotalResultados =
                        totalResultados,

                    TotalUbicaciones =
                        totalUbicaciones,

                    TotalTiposMantenimiento =
                        totalTipos,

                    TotalResponsables =
                        totalResponsables,

                    PaginaActual =
                        pagina,

                    TamanoPagina =
                        tamanoPagina,

                    TotalPaginas =
                        totalPaginas
                };


            await CargarListasEquiposMantenimiento(
                model);


            return View(model);
        }


        // =========================================
        // GENERAR PDF
        // =========================================
        [HttpGet]
        public async Task<IActionResult> GenerarEquiposMantenimientoPdf(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idMarcaFiltro,
            string? modeloFiltro,
            string? serieFiltro,
            int? idUbicacionFiltro,
            int? idTipoFiltro,
            string? responsableFiltro)
        {
            var consulta =
                ConstruirConsultaEquiposMantenimiento(
                    codigoFiltro,
                    descripcionFiltro,
                    idMarcaFiltro,
                    modeloFiltro,
                    serieFiltro,
                    idUbicacionFiltro,
                    idTipoFiltro,
                    responsableFiltro);


            var mantenimientos =
                await consulta
                    .OrderByDescending(m =>
                        m.FechaMantenimiento)
                    .ThenByDescending(m =>
                        m.IdMantenimiento)
                    .ToListAsync();


            if (!mantenimientos.Any())
            {
                TempData["ErrorReporte"] =
                    "No existen equipos con mantenimientos registrados para los filtros seleccionados.";

                return RedirectToAction(
                    nameof(EquiposMantenimiento));
            }


            // =====================================
            // DATOS GENERALES
            // =====================================
            string usuario =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.Name)?
                    .Value
                ?? "Usuario";


            DateTime fechaGeneracion =
                DateTime.Now;


            int totalUbicaciones =
                mantenimientos
                    .Select(m =>
                        m.Activo.IdUbicacion)
                    .Distinct()
                    .Count();


            int totalTipos =
                mantenimientos
                    .Select(m =>
                        m.IdTipo)
                    .Distinct()
                    .Count();


            // =====================================
            // ESTILOS
            // =====================================
            IContainer CeldaEncabezado(
                IContainer container)
            {
                return container
                    .Background(
                        QuestPDF.Helpers
                            .Colors.Blue.Darken3)
                    .Border(0.5f)
                    .BorderColor(
                        QuestPDF.Helpers
                            .Colors.Grey.Medium)
                    .Padding(4)
                    .AlignMiddle();
            }


            IContainer CeldaContenido(
                IContainer container)
            {
                return container
                    .BorderBottom(0.5f)
                    .BorderColor(
                        QuestPDF.Helpers
                            .Colors.Grey.Lighten1)
                    .Padding(4)
                    .AlignMiddle();
            }


            // =====================================
            // DOCUMENTO PDF
            // =====================================
            var documento =
                QuestPDF.Fluent.Document.Create(
                    container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(
                                QuestPDF.Helpers
                                    .PageSizes.A4
                                    .Landscape());


                            page.Margin(25);


                            page.DefaultTextStyle(
                                text =>
                                    text.FontSize(8));


                            // =================================
                            // ENCABEZADO
                            // =================================
                            page.Header()
                                .Column(column =>
                                {
                                    column.Spacing(3);


                                    column.Item()
                                        .Text(
                                            "ESCUELA PROYECTO PACUARE")
                                        .FontSize(16)
                                        .Bold()
                                        .FontColor(
                                            QuestPDF.Helpers
                                                .Colors.Blue
                                                .Darken3);


                                    column.Item()
                                        .Text(
                                            "Sistema de Inventario Institucional")
                                        .FontSize(10)
                                        .FontColor(
                                            QuestPDF.Helpers
                                                .Colors.Grey
                                                .Darken2);


                                    column.Item()
                                        .PaddingTop(7)
                                        .Text(
                                            "REPORTE DE EQUIPOS EN MANTENIMIENTO")
                                        .FontSize(13)
                                        .Bold();


                                    column.Item()
                                        .Text(
                                            $"Fecha de generación: " +
                                            $"{fechaGeneracion:dd/MM/yyyy HH:mm}");


                                    column.Item()
                                        .Text(
                                            $"Generado por: {usuario}");


                                    column.Item()
                                        .PaddingTop(5)
                                        .LineHorizontal(1)
                                        .LineColor(
                                            QuestPDF.Helpers
                                                .Colors.Grey
                                                .Lighten1);
                                });


                            // =================================
                            // CONTENIDO
                            // =================================
                            page.Content()
                                .PaddingVertical(10)
                                .Column(column =>
                                {
                                    column.Spacing(10);


                                    // =========================
                                    // RESUMEN
                                    // =========================
                                    column.Item()
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Border(1)
                                                .BorderColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Lighten1)
                                                .Padding(8)
                                                .Column(c =>
                                                {
                                                    c.Item()
                                                        .Text(
                                                            mantenimientos
                                                                .Count
                                                                .ToString())
                                                        .FontSize(15)
                                                        .Bold();

                                                    c.Item()
                                                        .Text(
                                                            "Equipos con mantenimiento");
                                                });


                                            row.RelativeItem()
                                                .Border(1)
                                                .BorderColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Lighten1)
                                                .Padding(8)
                                                .Column(c =>
                                                {
                                                    c.Item()
                                                        .Text(
                                                            totalUbicaciones
                                                                .ToString())
                                                        .FontSize(15)
                                                        .Bold();

                                                    c.Item()
                                                        .Text(
                                                            "Ubicaciones");
                                                });


                                            row.RelativeItem()
                                                .Border(1)
                                                .BorderColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Lighten1)
                                                .Padding(8)
                                                .Column(c =>
                                                {
                                                    c.Item()
                                                        .Text(
                                                            totalTipos
                                                                .ToString())
                                                        .FontSize(15)
                                                        .Bold();

                                                    c.Item()
                                                        .Text(
                                                            "Tipos de mantenimiento");
                                                });
                                        });


                                    // =========================
                                    // TABLA
                                    // =========================
                                    column.Item()
                                        .Table(table =>
                                        {
                                            table.ColumnsDefinition(
                                                columns =>
                                                {
                                                    columns.RelativeColumn(1.0f);
                                                    columns.RelativeColumn(1.8f);
                                                    columns.RelativeColumn(1.1f);
                                                    columns.RelativeColumn(1.2f);
                                                    columns.RelativeColumn(1.4f);
                                                    columns.RelativeColumn(1.3f);
                                                    columns.RelativeColumn(1.0f);
                                                    columns.RelativeColumn(1.3f);
                                                    columns.RelativeColumn(2.0f);
                                                });


                                            // =================
                                            // CABECERA
                                            // =================
                                            table.Header(header =>
                                            {
                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Código")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Descripción")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Marca")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Modelo")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Ubicación")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Tipo")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Fecha")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Responsable")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Observaciones")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();
                                            });


                                            // =================
                                            // FILAS
                                            // =================
                                            foreach (
                                                var mantenimiento
                                                in mantenimientos)
                                            {
                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        mantenimiento
                                                            .Activo
                                                            .NumeroIdentificacion);


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        mantenimiento
                                                            .Activo
                                                            .Descripcion);


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        mantenimiento
                                                            .Activo
                                                            .Marca?
                                                            .MarcaNombre
                                                        ?? "N/I");


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        string.IsNullOrWhiteSpace(
                                                            mantenimiento
                                                                .Activo
                                                                .Modelo)
                                                        ? "N/I"
                                                        : mantenimiento
                                                            .Activo
                                                            .Modelo);


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        mantenimiento
                                                            .Activo
                                                            .Ubicacion?
                                                            .UbicacionNombre
                                                        ?? "N/I");


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        mantenimiento
                                                            .TipoMantenimiento?
                                                            .Mantenimiento
                                                        ?? "N/I");


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        mantenimiento
                                                            .FechaMantenimiento
                                                            .ToString(
                                                                "dd/MM/yyyy"));


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        string.IsNullOrWhiteSpace(
                                                            mantenimiento
                                                                .Responsable)
                                                        ? "N/I"
                                                        : mantenimiento
                                                            .Responsable);


                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        string.IsNullOrWhiteSpace(
                                                            mantenimiento
                                                                .Descripcion)
                                                        ? "Sin observaciones"
                                                        : mantenimiento
                                                            .Descripcion);
                                            }
                                        });
                                });


                            // =================================
                            // PIE
                            // =================================
                            page.Footer()
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.DefaultTextStyle(
                                        x =>
                                            x.FontSize(8)
                                                .FontColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Darken1));


                                    text.Span(
                                        "Inventario Institucional - Página ");

                                    text.CurrentPageNumber();

                                    text.Span(" de ");

                                    text.TotalPages();
                                });
                        });
                    });


            byte[] pdf =
                documento.GeneratePdf();


            string nombreArchivo =
                $"EquiposMantenimiento_" +
                $"{DateTime.Now:yyyyMMdd_HHmm}.pdf";


            return File(
                pdf,
                "application/pdf",
                nombreArchivo);
        }


        // =========================================
        // CONSULTA REUTILIZABLE
        // =========================================
        private IQueryable<Mantenimiento>
     ConstruirConsultaEquiposMantenimiento(
         string? codigoFiltro,
         string? descripcionFiltro,
         int? idMarcaFiltro,
         string? modeloFiltro,
         string? serieFiltro,
         int? idUbicacionFiltro,
         int? idTipoFiltro,
         string? responsableFiltro)
        {
            // =====================================
            // CONSULTA BASE
            // =====================================
            var consulta =
                _context.Mantenimientos
                    .AsNoTracking()
                    .Include(m => m.Activo)
                        .ThenInclude(a => a.Marca)
                    .Include(m => m.Activo)
                        .ThenInclude(a => a.Ubicacion)
                    .Include(m => m.Activo)
                        .ThenInclude(a => a.EstadoActivo)
                    .Include(m => m.TipoMantenimiento)
                    .AsQueryable();


            // =====================================
            // CÓDIGO
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                codigoFiltro))
            {
                codigoFiltro =
                    codigoFiltro.Trim();

                consulta =
                    consulta.Where(m =>
                        m.Activo
                            .NumeroIdentificacion
                            .Contains(
                                codigoFiltro));
            }


            // =====================================
            // DESCRIPCIÓN DEL ACTIVO
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                descripcionFiltro))
            {
                descripcionFiltro =
                    descripcionFiltro.Trim();

                consulta =
                    consulta.Where(m =>
                        m.Activo
                            .Descripcion
                            .Contains(
                                descripcionFiltro));
            }


            // =====================================
            // MARCA
            // =====================================
            if (idMarcaFiltro.HasValue)
            {
                consulta =
                    consulta.Where(m =>
                        m.Activo.IdMarca ==
                        idMarcaFiltro.Value);
            }


            // =====================================
            // MODELO
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                modeloFiltro))
            {
                modeloFiltro =
                    modeloFiltro.Trim();

                consulta =
                    consulta.Where(m =>
                        m.Activo.Modelo != null &&
                        m.Activo.Modelo.Contains(
                            modeloFiltro));
            }


            // =====================================
            // SERIE
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                serieFiltro))
            {
                serieFiltro =
                    serieFiltro.Trim();

                consulta =
                    consulta.Where(m =>
                        m.Activo.Serie != null &&
                        m.Activo.Serie.Contains(
                            serieFiltro));
            }


            // =====================================
            // UBICACIÓN
            // =====================================
            if (idUbicacionFiltro.HasValue)
            {
                consulta =
                    consulta.Where(m =>
                        m.Activo.IdUbicacion ==
                        idUbicacionFiltro.Value);
            }


            // =====================================
            // TIPO DE MANTENIMIENTO
            // =====================================
            if (idTipoFiltro.HasValue)
            {
                consulta =
                    consulta.Where(m =>
                        m.IdTipo ==
                        idTipoFiltro.Value);
            }


            // =====================================
            // RESPONSABLE
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                responsableFiltro))
            {
                responsableFiltro =
                    responsableFiltro.Trim();

                consulta =
                    consulta.Where(m =>
                        m.Responsable != null &&
                        m.Responsable.Contains(
                            responsableFiltro));
            }


            return consulta;
        }


        // =========================================
        // CARGAR LISTAS
        // =========================================
        private async Task
            CargarListasEquiposMantenimiento(
                ReporteEquiposMantenimientoViewModel model)
        {
            model.Marcas =
                await _context.Marcas
                    .AsNoTracking()
                    .OrderBy(m =>
                        m.MarcaNombre)
                    .ToListAsync();


            model.Ubicaciones =
                await _context.Ubicaciones
                    .AsNoTracking()
                    .OrderBy(u =>
                        u.UbicacionNombre)
                    .ToListAsync();


            model.TiposMantenimiento =
                await _context.TiposMantenimiento
                    .AsNoTracking()
                    .OrderBy(t =>
                        t.Mantenimiento)
                    .ToListAsync();
        }

        // ============================================================
        // REPORTE: HISTORIAL DE MOVIMIENTOS
        // ============================================================


        // =========================================
        // CONFIGURAR REPORTE
        // =========================================
        [HttpGet]
        public async Task<IActionResult> HistorialMovimientos(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idUbicacionOrigenFiltro,
            int? idUbicacionDestinoFiltro,
            string? responsableFiltro,
            string? observacionFiltro,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            var model =
                new ReporteHistorialMovimientosViewModel
                {
                    CodigoFiltro =
                        codigoFiltro,

                    DescripcionFiltro =
                        descripcionFiltro,

                    IdUbicacionOrigenFiltro =
                        idUbicacionOrigenFiltro,

                    IdUbicacionDestinoFiltro =
                        idUbicacionDestinoFiltro,

                    ResponsableFiltro =
                        responsableFiltro,

                    ObservacionFiltro =
                        observacionFiltro,

                    FechaDesde =
                        fechaDesde,

                    FechaHasta =
                        fechaHasta
                };


            await CargarListasHistorialMovimientos(
                model);


            return View(model);
        }


        // =========================================
        // VISTA PREVIA
        // =========================================
        [HttpGet]
        public async Task<IActionResult> HistorialMovimientosPreview(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idUbicacionOrigenFiltro,
            int? idUbicacionDestinoFiltro,
            string? responsableFiltro,
            string? observacionFiltro,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int pagina = 1)
        {
            const int tamanoPagina = 10;


            // =====================================
            // VALIDAR RANGO DE FECHAS
            // =====================================
            if (fechaDesde.HasValue &&
                fechaHasta.HasValue &&
                fechaDesde.Value.Date >
                fechaHasta.Value.Date)
            {
                TempData["ErrorReporte"] =
                    "La fecha inicial no puede ser posterior a la fecha final.";

                return RedirectToAction(
                    nameof(HistorialMovimientos));
            }


            // =====================================
            // CONSULTA
            // =====================================
            var consulta =
                ConstruirConsultaHistorialMovimientos(
                    codigoFiltro,
                    descripcionFiltro,
                    idUbicacionOrigenFiltro,
                    idUbicacionDestinoFiltro,
                    responsableFiltro,
                    observacionFiltro,
                    fechaDesde,
                    fechaHasta);


            // =====================================
            // TOTAL DE MOVIMIENTOS
            // =====================================
            int totalMovimientos =
                await consulta.CountAsync();


            // =====================================
            // PAGINACIÓN
            // =====================================
            int totalPaginas =
                (int)Math.Ceiling(
                    totalMovimientos /
                    (double)tamanoPagina);


            if (pagina < 1)
            {
                pagina = 1;
            }


            if (totalPaginas > 0 &&
                pagina > totalPaginas)
            {
                pagina = totalPaginas;
            }


            // =====================================
            // SOLO 10 POR PÁGINA
            // =====================================
            var movimientos =
                await consulta
                    .OrderByDescending(m =>
                        m.FechaMovimiento)
                    .ThenByDescending(m =>
                        m.IdMovimiento)
                    .Skip(
                        (pagina - 1) *
                        tamanoPagina)
                    .Take(tamanoPagina)
                    .ToListAsync();


            // =====================================
            // ACTIVOS DIFERENTES
            // =====================================
            int totalActivosMovidos =
                await consulta
                    .Select(m => m.IdActivo)
                    .Distinct()
                    .CountAsync();


            // =====================================
            // UBICACIONES ORIGEN
            // =====================================
            int totalUbicacionesOrigen =
                await consulta
                    .Where(m =>
                        m.UbicacionOrigen != null)
                    .Select(m =>
                        m.UbicacionOrigen)
                    .Distinct()
                    .CountAsync();


            // =====================================
            // UBICACIONES DESTINO
            // =====================================
            int totalUbicacionesDestino =
                await consulta
                    .Select(m =>
                        m.UbicacionDestino)
                    .Distinct()
                    .CountAsync();


            // =====================================
            // VIEWMODEL
            // =====================================
            var model =
                new ReporteHistorialMovimientosViewModel
                {
                    CodigoFiltro =
                        codigoFiltro,

                    DescripcionFiltro =
                        descripcionFiltro,

                    IdUbicacionOrigenFiltro =
                        idUbicacionOrigenFiltro,

                    IdUbicacionDestinoFiltro =
                        idUbicacionDestinoFiltro,

                    ResponsableFiltro =
                        responsableFiltro,

                    ObservacionFiltro =
                        observacionFiltro,

                    FechaDesde =
                        fechaDesde,

                    FechaHasta =
                        fechaHasta,

                    Movimientos =
                        movimientos,

                    TotalMovimientos =
                        totalMovimientos,

                    TotalActivosMovidos =
                        totalActivosMovidos,

                    TotalUbicacionesOrigen =
                        totalUbicacionesOrigen,

                    TotalUbicacionesDestino =
                        totalUbicacionesDestino,

                    PaginaActual =
                        pagina,

                    TamanoPagina =
                        tamanoPagina,

                    TotalPaginas =
                        totalPaginas
                };


            await CargarListasHistorialMovimientos(
                model);


            return View(model);
        }


        // =========================================
        // GENERAR PDF
        // =========================================
        [HttpGet]
        public async Task<IActionResult> GenerarHistorialMovimientosPdf(
            string? codigoFiltro,
            string? descripcionFiltro,
            int? idUbicacionOrigenFiltro,
            int? idUbicacionDestinoFiltro,
            string? responsableFiltro,
            string? observacionFiltro,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            // =====================================
            // VALIDAR FECHAS
            // =====================================
            if (fechaDesde.HasValue &&
                fechaHasta.HasValue &&
                fechaDesde.Value.Date >
                fechaHasta.Value.Date)
            {
                TempData["ErrorReporte"] =
                    "La fecha inicial no puede ser posterior a la fecha final.";

                return RedirectToAction(
                    nameof(HistorialMovimientos));
            }


            // =====================================
            // CONSULTA COMPLETA
            // =====================================
            var consulta =
                ConstruirConsultaHistorialMovimientos(
                    codigoFiltro,
                    descripcionFiltro,
                    idUbicacionOrigenFiltro,
                    idUbicacionDestinoFiltro,
                    responsableFiltro,
                    observacionFiltro,
                    fechaDesde,
                    fechaHasta);


            var movimientos =
                await consulta
                    .OrderByDescending(m =>
                        m.FechaMovimiento)
                    .ThenByDescending(m =>
                        m.IdMovimiento)
                    .ToListAsync();


            if (!movimientos.Any())
            {
                TempData["ErrorReporte"] =
                    "No existen movimientos para los filtros seleccionados.";

                return RedirectToAction(
                    nameof(HistorialMovimientos));
            }


            // =====================================
            // RESUMEN
            // =====================================
            int totalActivos =
                movimientos
                    .Select(m => m.IdActivo)
                    .Distinct()
                    .Count();


            int totalOrigenes =
                movimientos
                    .Where(m =>
                        m.UbicacionOrigen.HasValue)
                    .Select(m =>
                        m.UbicacionOrigen)
                    .Distinct()
                    .Count();


            int totalDestinos =
                movimientos
                    .Select(m =>
                        m.UbicacionDestino)
                    .Distinct()
                    .Count();


            // =====================================
            // USUARIO / FECHA
            // =====================================
            string usuario =
                User.FindFirst(
                    System.Security.Claims.ClaimTypes.Name)?
                    .Value
                ?? "Usuario";


            DateTime fechaGeneracion =
                DateTime.Now;


            // =====================================
            // ESTILOS
            // =====================================
            IContainer CeldaEncabezado(
                IContainer container)
            {
                return container
                    .Background(
                        QuestPDF.Helpers
                            .Colors.Blue.Darken3)
                    .Border(0.5f)
                    .BorderColor(
                        QuestPDF.Helpers
                            .Colors.Grey.Medium)
                    .Padding(4)
                    .AlignMiddle();
            }


            IContainer CeldaContenido(
                IContainer container)
            {
                return container
                    .BorderBottom(0.5f)
                    .BorderColor(
                        QuestPDF.Helpers
                            .Colors.Grey.Lighten1)
                    .Padding(4)
                    .AlignMiddle();
            }


            // =====================================
            // DOCUMENTO
            // =====================================
            var documento =
                QuestPDF.Fluent.Document.Create(
                    container =>
                    {
                        container.Page(page =>
                        {
                            page.Size(
                                QuestPDF.Helpers
                                    .PageSizes.A4
                                    .Landscape());


                            page.Margin(25);


                            page.DefaultTextStyle(
                                text =>
                                    text.FontSize(8));


                            // =================================
                            // ENCABEZADO
                            // =================================
                            page.Header()
                                .Column(column =>
                                {
                                    column.Spacing(3);


                                    column.Item()
                                        .Text(
                                            "ESCUELA PROYECTO PACUARE")
                                        .FontSize(16)
                                        .Bold()
                                        .FontColor(
                                            QuestPDF.Helpers
                                                .Colors.Blue
                                                .Darken3);


                                    column.Item()
                                        .Text(
                                            "Sistema de Inventario Institucional")
                                        .FontSize(10)
                                        .FontColor(
                                            QuestPDF.Helpers
                                                .Colors.Grey
                                                .Darken2);


                                    column.Item()
                                        .PaddingTop(7)
                                        .Text(
                                            "REPORTE DE HISTORIAL DE MOVIMIENTOS")
                                        .FontSize(13)
                                        .Bold();


                                    column.Item()
                                        .Text(
                                            $"Fecha de generación: " +
                                            $"{fechaGeneracion:dd/MM/yyyy HH:mm}");


                                    column.Item()
                                        .Text(
                                            $"Generado por: {usuario}");


                                    if (fechaDesde.HasValue ||
                                        fechaHasta.HasValue)
                                    {
                                        string desde =
                                            fechaDesde.HasValue
                                            ? fechaDesde.Value
                                                .ToString("dd/MM/yyyy")
                                            : "Inicio";


                                        string hasta =
                                            fechaHasta.HasValue
                                            ? fechaHasta.Value
                                                .ToString("dd/MM/yyyy")
                                            : "Actualidad";


                                        column.Item()
                                            .Text(
                                                $"Período: {desde} - {hasta}");
                                    }


                                    column.Item()
                                        .PaddingTop(5)
                                        .LineHorizontal(1)
                                        .LineColor(
                                            QuestPDF.Helpers
                                                .Colors.Grey
                                                .Lighten1);
                                });


                            // =================================
                            // CONTENIDO
                            // =================================
                            page.Content()
                                .PaddingVertical(10)
                                .Column(column =>
                                {
                                    column.Spacing(10);


                                    // =========================
                                    // RESUMEN
                                    // =========================
                                    column.Item()
                                        .Row(row =>
                                        {
                                            // MOVIMIENTOS
                                            row.RelativeItem()
                                                .Border(1)
                                                .BorderColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Lighten1)
                                                .Padding(8)
                                                .Column(c =>
                                                {
                                                    c.Item()
                                                        .Text(
                                                            movimientos
                                                                .Count
                                                                .ToString())
                                                        .FontSize(15)
                                                        .Bold();

                                                    c.Item()
                                                        .Text(
                                                            "Movimientos");
                                                });


                                            // ACTIVOS
                                            row.RelativeItem()
                                                .Border(1)
                                                .BorderColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Lighten1)
                                                .Padding(8)
                                                .Column(c =>
                                                {
                                                    c.Item()
                                                        .Text(
                                                            totalActivos
                                                                .ToString())
                                                        .FontSize(15)
                                                        .Bold();

                                                    c.Item()
                                                        .Text(
                                                            "Activos movilizados");
                                                });


                                            // ORÍGENES
                                            row.RelativeItem()
                                                .Border(1)
                                                .BorderColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Lighten1)
                                                .Padding(8)
                                                .Column(c =>
                                                {
                                                    c.Item()
                                                        .Text(
                                                            totalOrigenes
                                                                .ToString())
                                                        .FontSize(15)
                                                        .Bold();

                                                    c.Item()
                                                        .Text(
                                                            "Ubicaciones origen");
                                                });


                                            // DESTINOS
                                            row.RelativeItem()
                                                .Border(1)
                                                .BorderColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Lighten1)
                                                .Padding(8)
                                                .Column(c =>
                                                {
                                                    c.Item()
                                                        .Text(
                                                            totalDestinos
                                                                .ToString())
                                                        .FontSize(15)
                                                        .Bold();

                                                    c.Item()
                                                        .Text(
                                                            "Ubicaciones destino");
                                                });
                                        });


                                    // =========================
                                    // TABLA
                                    // =========================
                                    column.Item()
                                        .Table(table =>
                                        {
                                            table.ColumnsDefinition(
                                                columns =>
                                                {
                                                    columns
                                                        .RelativeColumn(1.2f);

                                                    columns
                                                        .RelativeColumn(1.0f);

                                                    columns
                                                        .RelativeColumn(2.0f);

                                                    columns
                                                        .RelativeColumn(1.5f);

                                                    columns
                                                        .RelativeColumn(1.5f);

                                                    columns
                                                        .RelativeColumn(1.4f);

                                                    columns
                                                        .RelativeColumn(2.0f);
                                                });


                                            // =================
                                            // ENCABEZADOS
                                            // =================
                                            table.Header(header =>
                                            {
                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Fecha")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Código")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Descripción")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Origen")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Destino")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Responsable")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();


                                                header.Cell()
                                                    .Element(
                                                        CeldaEncabezado)
                                                    .Text("Observación")
                                                    .FontColor(
                                                        QuestPDF.Helpers
                                                            .Colors.White)
                                                    .Bold();
                                            });


                                            // =================
                                            // FILAS
                                            // =================
                                            foreach (
                                                var movimiento
                                                in movimientos)
                                            {
                                                // FECHA
                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        movimiento
                                                            .FechaMovimiento
                                                            .ToString(
                                                                "dd/MM/yyyy HH:mm"));


                                                // CÓDIGO
                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        movimiento
                                                            .Activo?
                                                            .NumeroIdentificacion
                                                        ?? "N/I");


                                                // DESCRIPCIÓN
                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        movimiento
                                                            .Activo?
                                                            .Descripcion
                                                        ?? "N/I");


                                                // ORIGEN
                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        movimiento
                                                            .MovimientosOrigen?
                                                            .UbicacionNombre
                                                        ?? "Sin origen");


                                                // DESTINO
                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        movimiento
                                                            .MovimientosDestino?
                                                            .UbicacionNombre
                                                        ?? "N/I");


                                                // RESPONSABLE
                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        string.IsNullOrWhiteSpace(
                                                            movimiento
                                                                .Responsable)
                                                        ? "N/I"
                                                        : movimiento
                                                            .Responsable);


                                                // OBSERVACIÓN
                                                table.Cell()
                                                    .Element(
                                                        CeldaContenido)
                                                    .Text(
                                                        string.IsNullOrWhiteSpace(
                                                            movimiento
                                                                .Observacion)
                                                        ? "Sin observación"
                                                        : movimiento
                                                            .Observacion);
                                            }
                                        });
                                });


                            // =================================
                            // PIE
                            // =================================
                            page.Footer()
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.DefaultTextStyle(
                                        x =>
                                            x.FontSize(8)
                                                .FontColor(
                                                    QuestPDF.Helpers
                                                        .Colors.Grey
                                                        .Darken1));


                                    text.Span(
                                        "Inventario Institucional - Página ");

                                    text.CurrentPageNumber();

                                    text.Span(" de ");

                                    text.TotalPages();
                                });
                        });
                    });


            byte[] pdf =
                documento.GeneratePdf();


            string nombreArchivo =
                $"HistorialMovimientos_" +
                $"{DateTime.Now:yyyyMMdd_HHmm}.pdf";


            return File(
                pdf,
                "application/pdf",
                nombreArchivo);
        }


        // =========================================
        // CONSULTA REUTILIZABLE
        // =========================================
        private IQueryable<MovimientoActivo>
            ConstruirConsultaHistorialMovimientos(
                string? codigoFiltro,
                string? descripcionFiltro,
                int? idUbicacionOrigenFiltro,
                int? idUbicacionDestinoFiltro,
                string? responsableFiltro,
                string? observacionFiltro,
                DateTime? fechaDesde,
                DateTime? fechaHasta)
        {
            // =====================================
            // CONSULTA BASE
            // =====================================
            var consulta =
                _context.MovimientosActivos
                    .AsNoTracking()
                    .Include(m => m.Activo)
                    .Include(m => m.MovimientosOrigen)
                    .Include(m => m.MovimientosDestino)
                    .AsQueryable();


            // =====================================
            // CÓDIGO
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                codigoFiltro))
            {
                codigoFiltro =
                    codigoFiltro.Trim();

                consulta =
                    consulta.Where(m =>
                        m.Activo
                            .NumeroIdentificacion
                            .Contains(
                                codigoFiltro));
            }


            // =====================================
            // DESCRIPCIÓN
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                descripcionFiltro))
            {
                descripcionFiltro =
                    descripcionFiltro.Trim();

                consulta =
                    consulta.Where(m =>
                        m.Activo
                            .Descripcion
                            .Contains(
                                descripcionFiltro));
            }


            // =====================================
            // ORIGEN
            // =====================================
            if (idUbicacionOrigenFiltro.HasValue)
            {
                consulta =
                    consulta.Where(m =>
                        m.UbicacionOrigen ==
                        idUbicacionOrigenFiltro.Value);
            }


            // =====================================
            // DESTINO
            // =====================================
            if (idUbicacionDestinoFiltro.HasValue)
            {
                consulta =
                    consulta.Where(m =>
                        m.UbicacionDestino ==
                        idUbicacionDestinoFiltro.Value);
            }


            // =====================================
            // RESPONSABLE
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                responsableFiltro))
            {
                responsableFiltro =
                    responsableFiltro.Trim();

                consulta =
                    consulta.Where(m =>
                        m.Responsable != null &&
                        m.Responsable.Contains(
                            responsableFiltro));
            }


            // =====================================
            // OBSERVACIÓN
            // =====================================
            if (!string.IsNullOrWhiteSpace(
                observacionFiltro))
            {
                observacionFiltro =
                    observacionFiltro.Trim();

                consulta =
                    consulta.Where(m =>
                        m.Observacion != null &&
                        m.Observacion.Contains(
                            observacionFiltro));
            }


            // =====================================
            // FECHA DESDE
            // =====================================
            if (fechaDesde.HasValue)
            {
                DateTime inicio =
                    fechaDesde.Value.Date;

                consulta =
                    consulta.Where(m =>
                        m.FechaMovimiento >= inicio);
            }


            // =====================================
            // FECHA HASTA
            // Incluye todo el día seleccionado.
            // =====================================
            if (fechaHasta.HasValue)
            {
                DateTime fin =
                    fechaHasta.Value
                        .Date
                        .AddDays(1);

                consulta =
                    consulta.Where(m =>
                        m.FechaMovimiento < fin);
            }


            return consulta;
        }


        // =========================================
        // LISTAS
        // =========================================
        private async Task
            CargarListasHistorialMovimientos(
                ReporteHistorialMovimientosViewModel model)
        {
            model.Ubicaciones =
                await _context.Ubicaciones
                    .AsNoTracking()
                    .OrderBy(u =>
                        u.UbicacionNombre)
                    .ToListAsync();
        }

    }
}

