using System.Globalization;
using System.Text;
using InventarioEscuela.Models;
using InventarioEscuela.ViewModels;

namespace InventarioEscuela.Services.Reportes;

/// <summary>
/// Exporta los resultados ya consultados sin acceder a la base de datos ni al contexto HTTP.
/// Conserva los identificadores originales; al importarlos en Excel deben elegirse columnas de tipo texto.
/// </summary>
public sealed class ReporteCsvService : IReporteCsvService
{
    private const string NoInformado = "No informado";
    private const string TipoContenido = "text/csv; charset=utf-8";

    private static readonly string[] EncabezadosActivo =
    [
        "Código", "Descripción", "Marca", "Modelo", "Serie", "Tomo", "Folio", "Asiento",
        "Estado actual", "Ubicación actual", "Modo de adquisición", "Fecha de registro", "Observación"
    ];

    public ArchivoReporte GenerarInventarioGeneral(ReporteInventarioGeneralViewModel reporte, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reporte);
        return GenerarActivos(reporte.Activos, "inventario-general.csv", cancellationToken);
    }

    public ArchivoReporte GenerarRecursosPorUbicacion(ReporteRecursosPorUbicacionViewModel reporte, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reporte);
        return GenerarActivos(reporte.Activos, "recursos-por-ubicacion.csv", cancellationToken);
    }

    public ArchivoReporte GenerarEquiposDanados(ReporteEquiposDanadosViewModel reporte, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reporte);
        return GenerarActivos(reporte.Activos, "equipos-danados.csv", cancellationToken);
    }

    public ArchivoReporte GenerarEquiposMantenimiento(ReporteEquiposMantenimientoViewModel reporte, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reporte);
        cancellationToken.ThrowIfCancellationRequested();

        var csv = new StringBuilder();
        EscribirFila(csv,
        [
            "Código", "Descripción del activo", "Marca", "Modelo", "Serie", "Estado actual del activo",
            "Ubicación actual del activo", "Fecha de mantenimiento", "Tipo de mantenimiento", "Responsable",
            "Descripción del mantenimiento"
        ]);

        foreach (var mantenimiento in reporte.Mantenimientos)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var activo = mantenimiento.Activo;
            EscribirFila(csv,
            [
                Identificador(activo?.NumeroIdentificacion), Texto(activo?.Descripcion), Texto(activo?.Marca?.MarcaNombre),
                Texto(activo?.Modelo), Identificador(activo?.Serie), Texto(activo?.EstadoActivo?.Estado),
                Texto(activo?.Ubicacion?.UbicacionNombre), Fecha(mantenimiento.FechaMantenimiento),
                Texto(mantenimiento.TipoMantenimiento?.Mantenimiento), Texto(mantenimiento.Responsable),
                Texto(mantenimiento.Descripcion)
            ]);
        }

        return CrearArchivo(csv, "historial-mantenimientos.csv", cancellationToken);
    }

    public ArchivoReporte GenerarHistorialMovimientos(ReporteHistorialMovimientosViewModel reporte, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reporte);
        cancellationToken.ThrowIfCancellationRequested();

        var csv = new StringBuilder();
        EscribirFila(csv,
        [
            "Código", "Descripción del activo", "Marca", "Modelo", "Serie", "Fecha y hora del movimiento",
            "Ubicación de origen", "Ubicación de destino", "Responsable", "Observación"
        ]);

        foreach (var movimiento in reporte.Movimientos)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var activo = movimiento.Activo;
            EscribirFila(csv,
            [
                Identificador(activo?.NumeroIdentificacion), Texto(activo?.Descripcion), Texto(activo?.Marca?.MarcaNombre),
                Texto(activo?.Modelo), Identificador(activo?.Serie),
                movimiento.FechaMovimiento.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                movimiento.UbicacionOrigen.HasValue ? Texto(movimiento.MovimientosOrigen?.UbicacionNombre) : "Sin ubicación anterior",
                Texto(movimiento.MovimientosDestino?.UbicacionNombre), Texto(movimiento.Responsable), Texto(movimiento.Observacion)
            ]);
        }

        return CrearArchivo(csv, "historial-movimientos.csv", cancellationToken);
    }

    private static ArchivoReporte GenerarActivos(IEnumerable<Activo> activos, string nombreArchivo, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var csv = new StringBuilder();
        EscribirFila(csv, EncabezadosActivo);

        foreach (var activo in activos)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EscribirFila(csv,
            [
                Identificador(activo.NumeroIdentificacion), Texto(activo.Descripcion), Texto(activo.Marca?.MarcaNombre),
                Texto(activo.Modelo), Identificador(activo.Serie), Identificador(activo.Tomo), Identificador(activo.Folio),
                Identificador(activo.Asiento), Texto(activo.EstadoActivo?.Estado), Texto(activo.Ubicacion?.UbicacionNombre),
                Texto(activo.ModoAdquisicion?.NombreModoAdquisicion), Fecha(activo.FechaRegistro), Texto(activo.Observacion)
            ]);
        }

        return CrearArchivo(csv, nombreArchivo, cancellationToken);
    }

    private static string Fecha(DateTime fecha) => fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string Texto(string? valor) => string.IsNullOrWhiteSpace(valor) ? NoInformado : valor;

    private static string Identificador(string? valor)
    {
        // Las comillas CSV conservan el dato original. La hoja de cálculo debe importar
        // estas columnas como texto para no convertir ceros iniciales ni números largos.
        return Texto(valor);
    }

    private static void EscribirFila(StringBuilder csv, IEnumerable<string> valores)
    {
        var primeraColumna = true;
        foreach (var valor in valores)
        {
            if (!primeraColumna)
                csv.Append(';');
            primeraColumna = false;

            // También se ignoran controles antes del prefijo: Excel puede interpretar
            // como fórmula una celda con tabulaciones o espacios delante de =, +, - o @.
            var inicio = 0;
            while (inicio < valor.Length && (char.IsWhiteSpace(valor[inicio]) || char.IsControl(valor[inicio])))
                inicio++;

            csv.Append('"');
            if (inicio < valor.Length && valor[inicio] is '=' or '+' or '-' or '@')
                csv.Append('\'');
            csv.Append(valor.Replace("\"", "\"\"", StringComparison.Ordinal));
            csv.Append('"');
        }
        csv.Append("\r\n");
    }

    private static ArchivoReporte CrearArchivo(StringBuilder csv, string nombreArchivo, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var contenido = Encoding.UTF8.GetBytes(csv.ToString());
        var bom = Encoding.UTF8.GetPreamble();
        var contenidoConBom = new byte[bom.Length + contenido.Length];
        bom.CopyTo(contenidoConBom, 0);
        contenido.CopyTo(contenidoConBom, bom.Length);
        return new ArchivoReporte(contenidoConBom, nombreArchivo, TipoContenido);
    }
}
