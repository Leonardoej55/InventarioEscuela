using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace InventarioEscuela.Services
{
    public static class ErroresBaseDeDatos
    {
        private static IEnumerable<Exception> Causas(Exception error)
        {
            for (Exception? actual = error; actual != null; actual = actual.InnerException)
            {
                yield return actual;
            }
        }

        public static bool EsErrorDeBaseDeDatos(Exception error) =>
            Causas(error).Any(e => e is DbException or DbUpdateException or TimeoutException);

        public static bool EsDuplicado(Exception error) => TieneCodigo(error, 2601, 2627);

        private static bool TieneCodigo(Exception error, params int[] codigos) =>
            Causas(error).OfType<SqlException>().Any(sql =>
                sql.Errors.Cast<SqlError>().Any(e => codigos.Contains(e.Number)));

        public static string MensajeGuardado(Exception error)
        {
            if (Causas(error).Any(e => e is DbUpdateConcurrencyException))
            {
                return "El registro cambió o fue eliminado por otra persona. " +
                    "Actualice el formulario antes de volver a guardar.";
            }

            if (TieneCodigo(error, 547))
            {
                return "Una de las opciones seleccionadas o uno de los datos ya no es válido. " +
                    "Revise el formulario y vuelva a intentarlo.";
            }

            if (TieneCodigo(error, 2628, 8152))
            {
                return "Uno de los datos supera el tamaño permitido en la base de datos. " +
                    "Revise los campos o consulte al administrador.";
            }

            return "No se pudo confirmar el guardado. Compruebe si los datos ya se registraron " +
                "antes de intentarlo nuevamente.";
        }
    }
}
