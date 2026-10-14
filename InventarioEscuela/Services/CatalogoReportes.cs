using InventarioEscuela.ViewModels;

namespace InventarioEscuela.Services.Reportes;

public static class CatalogoReportes
{
    private static readonly ReporteOrden[] OrdenActivos =
    [new("codigo", "Código"), new("descripcion", "Descripción"), new("ubicacion", "Ubicación"),
     new("registro_desc", "Registro más reciente")];
    private static readonly ReporteOrden[] OrdenHistorial =
    [new("fecha_desc", "Más recientes primero"), new("fecha_asc", "Más antiguos primero"), new("codigo", "Código del activo")];

    // Las columnas adicionales se incluyen en CSV para conservar los datos registrales.
    private static ReporteColumna[] ColumnasActivos(bool observacionVisible = false) =>
    [new("Código", 1), new("Descripción", 2.2f), new("Marca", 1), new("Modelo", 1),
     new("Serie", 1), new("Estado", 1.2f), new("Ubicación", 1.4f), new("Adquisición", 1.2f),
     new("Fecha de registro", 1), new("Tomo", 1, false, false), new("Folio", 1, false, false),
     new("Asiento", 1, false, false), new("Observación", 2, observacionVisible, observacionVisible)];

    public static IReadOnlyList<ReporteDefinicion> Todos { get; } =
    [
        new()
        {
            Tipo = TipoReporte.InventarioGeneral, Titulo = "Inventario general", Icono = "📦", Unidad = "activos",
            Descripcion = "Revise el inventario actual y su distribución por estado.",
            Ayuda = "El estado y la ubicación son los actuales. Las fechas filtran cuándo se registró cada activo; no reconstruyen el inventario de una fecha pasada.",
            EtiquetaFecha = "Fecha de registro del activo", Columnas = ColumnasActivos(), Ordenes = OrdenActivos
        },
        new()
        {
            Tipo = TipoReporte.RecursosPorUbicacion, Titulo = "Activos por ubicación", Icono = "🏫", Unidad = "activos",
            Descripcion = "Compruebe qué activos hay en cada aula, oficina o almacén.",
            Ayuda = "Las cantidades por ubicación incluyen todos los resultados filtrados. Las fechas corresponden al registro del activo; la ubicación es la actual.",
            EtiquetaFecha = "Fecha de registro del activo", Columnas = ColumnasActivos(),
            Ordenes = [OrdenActivos[2], OrdenActivos[0], OrdenActivos[1], OrdenActivos[3]]
        },
        new()
        {
            Tipo = TipoReporte.EquiposDanados, Titulo = "Activos dañados", Icono = "⚠️", Unidad = "activos",
            Descripcion = "Identifique activos en estado Dañado o Malo para revisar sus observaciones.",
            Ayuda = "Se incluyen los estados Dañado y Malo del catálogo. Las fechas son de registro del activo, no la fecha del daño.",
            EtiquetaFecha = "Fecha de registro del activo", Columnas = ColumnasActivos(true), Ordenes = OrdenActivos
        },
        new()
        {
            Tipo = TipoReporte.EquiposMantenimiento, Titulo = "Activos en mantenimiento", Icono = "🛠️", Unidad = "activos",
            Descripcion = "Consulte los activos cuyo estado actual es En mantenimiento.",
            Ayuda = "Cada activo aparece una vez, aunque tenga varias intervenciones o ninguna. Para revisar trabajos realizados, utilice Historial de mantenimientos. Las fechas filtran el registro del activo.",
            EtiquetaFecha = "Fecha de registro del activo", Columnas = ColumnasActivos(true), Ordenes = OrdenActivos
        },
        new()
        {
            Tipo = TipoReporte.HistorialMantenimientos, Titulo = "Historial de mantenimientos", Icono = "📋", Unidad = "mantenimientos",
            Descripcion = "Revise intervenciones por fecha, tipo, activo y responsable.",
            Ayuda = "Cada fila es una intervención. El activo puede aparecer varias veces. La ubicación, marca, modelo y serie del activo son sus datos actuales; no se guardan copias históricas de esos campos.",
            EtiquetaFecha = "Fecha del mantenimiento", Ordenes = OrdenHistorial,
            Columnas = [new("Código", 1), new("Activo", 1.8f), new("Fecha del mantenimiento", 1.1f),
                new("Tipo", 1.1f), new("Trabajo realizado", 2.3f), new("Responsable", 1.4f),
                new("Ubicación actual", 1.3f), new("Marca", 1, false, false),
                new("Modelo", 1, false, false), new("Serie", 1, false, false)]
        },
        new()
        {
            Tipo = TipoReporte.HistorialMovimientos, Titulo = "Historial de movimientos", Icono = "🔄", Unidad = "movimientos",
            Descripcion = "Revise los traslados entre ubicaciones y quién los realizó.",
            Ayuda = "Origen y destino corresponden al traslado registrado. El código y la descripción del activo son los actuales. Las fechas corresponden al movimiento.",
            EtiquetaFecha = "Fecha del movimiento", Ordenes = OrdenHistorial,
            Columnas = [new("Código", 1), new("Activo", 1.8f), new("Fecha del movimiento", 1.3f),
                new("Origen", 1.3f), new("Destino", 1.3f), new("Responsable", 1.4f), new("Observación", 2)]
        }
    ];

    public static ReporteDefinicion Obtener(TipoReporte tipo) =>
        Todos.FirstOrDefault(r => r.Tipo == tipo) ?? throw new ArgumentOutOfRangeException(nameof(tipo));
}
