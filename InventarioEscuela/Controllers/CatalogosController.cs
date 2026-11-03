using InventarioEscuela.Data;
using InventarioEscuela.Models;
using InventarioEscuela.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventarioEscuela.Controllers;

[Authorize(Roles = "Administrador")]
public class CatalogosController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CatalogosController> _logger;

    public CatalogosController(
        ApplicationDbContext context,
        ILogger<CatalogosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar(
        [FromForm] string? tipo,
        [FromForm] string? nombre,
        CancellationToken cancellationToken)
    {
        int longitudMaxima = tipo switch
        {
            "marca" or "ubicacion" or
            "modoAdquisicion" or "tipoMantenimiento" => 100,
            "estado" => 50,
            _ => 0
        };

        if (!ModelState.IsValid || longitudMaxima == 0)
        {
            return BadRequest(new
            {
                mensaje = "El tipo de opción no es válido."
            });
        }

        nombre = nombre?.Trim();

        if (string.IsNullOrWhiteSpace(nombre))
        {
            return BadRequest(new
            {
                mensaje = "Ingrese un nombre."
            });
        }

        if (nombre.Length > longitudMaxima)
        {
            return BadRequest(new
            {
                mensaje =
                    $"El nombre no puede superar los {longitudMaxima} caracteres."
            });
        }

        const string mensajeDuplicado =
            "Ya existe una opción con ese nombre. Selecciónela en la lista.";

        try
        {
            string normalizado = nombre.ToLowerInvariant();

            bool existe = tipo switch
            {
                "marca" => await _context.Marcas.AnyAsync(
                    m => m.MarcaNombre.Trim().ToLower() == normalizado,
                    cancellationToken),

                "estado" => await _context.EstadosActivos.AnyAsync(
                    e => e.Estado.Trim().ToLower() == normalizado,
                    cancellationToken),

                "ubicacion" => await _context.Ubicaciones.AnyAsync(
                    u => u.UbicacionNombre.Trim().ToLower() == normalizado,
                    cancellationToken),

                "modoAdquisicion" => await _context.ModosAdquisicion.AnyAsync(
                    m => m.NombreModoAdquisicion.Trim().ToLower() == normalizado,
                    cancellationToken),

                "tipoMantenimiento" => await _context.TiposMantenimiento.AnyAsync(
                    t => t.Mantenimiento.Trim().ToLower() == normalizado,
                    cancellationToken),

                _ => false
            };

            if (existe)
            {
                return Conflict(new
                {
                    mensaje = mensajeDuplicado
                });
            }

            object opcion = tipo switch
            {
                "marca" => new Marca
                {
                    MarcaNombre = nombre
                },

                "estado" => new EstadoActivo
                {
                    Estado = nombre
                },

                "ubicacion" => new Ubicacion
                {
                    UbicacionNombre = nombre
                },

                "modoAdquisicion" => new ModoAdquisicion
                {
                    NombreModoAdquisicion = nombre
                },

                "tipoMantenimiento" => new TipoMantenimiento
                {
                    Mantenimiento = nombre
                },

                _ => throw new InvalidOperationException(
                    "Tipo de catálogo no válido.")
            };

            _context.Add(opcion);
            await _context.SaveChangesAsync(cancellationToken);

            int id = opcion switch
            {
                Marca marca => marca.IdMarca,
                EstadoActivo estado => estado.IdEstado,
                Ubicacion ubicacion => ubicacion.IdUbicacion,
                ModoAdquisicion modo => modo.IdModoAdquisicion,
                TipoMantenimiento mantenimiento => mantenimiento.IdTipo,

                _ => throw new InvalidOperationException(
                    "Tipo de catálogo no válido.")
            };

            return Ok(new { id, nombre });
        }
        catch (Exception ex)
            when (ErroresBaseDeDatos.EsErrorDeBaseDeDatos(ex))
        {
            if (ErroresBaseDeDatos.EsDuplicado(ex))
            {
                return Conflict(new
                {
                    mensaje = mensajeDuplicado
                });
            }

            _logger.LogError(
                ex,
                "Error al agregar una opción al catálogo {Tipo}.",
                tipo);

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    mensaje = ErroresBaseDeDatos.MensajeGuardado(ex)
                });
        }
    }
}