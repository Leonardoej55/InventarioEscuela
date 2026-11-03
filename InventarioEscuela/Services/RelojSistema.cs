using InventarioEscuela.Services.Reportes;
using Microsoft.Extensions.Options;

namespace InventarioEscuela.Services;

public sealed class RelojSistema
{
    private readonly TimeProvider _fuente;
    private readonly TimeZoneInfo _zonaHoraria;

    public RelojSistema(
        TimeProvider fuente,
        IOptions<OpcionesReportes> opciones)
    {
        _fuente = fuente;
        _zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById(
            opciones.Value.ZonaHoraria);
    }

    public DateTimeOffset Ahora =>
        TimeZoneInfo.ConvertTime(_fuente.GetUtcNow(), _zonaHoraria);

    // Las columnas existentes guardan la fecha y hora local institucional.
    public DateTime FechaHora => Ahora.DateTime;

    public DateTime Hoy => FechaHora.Date;
}