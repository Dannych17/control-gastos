using ControlGastos.Api.Data;
using ControlGastos.Api.Dtos;
using ControlGastos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Security.Claims;

namespace ControlGastos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TransaccionesController : ControllerBase
{
    private readonly AppDbContext _context;

    // TEMPORAL: se reemplazará por el usuario del token JWT
    private int UsuarioId => int.Parse(User.FindFirstValue("sub")!);

    public TransaccionesController(AppDbContext context)
    {
        _context = context;
    }

    private static readonly Expression<Func<Transaccion, TransaccionDto>> ToDto = t => new TransaccionDto
    {
        Id = t.Id,
        Monto = t.Monto,
        Fecha = t.Fecha,
        Descripcion = t.Descripcion,
        CategoriaId = t.CategoriaId,
        CategoriaNombre = t.Categoria!.Nombre,
        Tipo = t.Categoria!.Tipo
    };

    // GET api/transacciones?anio=2026&mes=10
    [HttpGet]
    public async Task<ActionResult<List<TransaccionDto>>> GetAll([FromQuery] int? anio, [FromQuery] int? mes)
    {
        var query = _context.Transacciones.Where(t => t.UsuarioId == UsuarioId);

        if (anio.HasValue && mes.HasValue)
        {
            if (mes < 1 || mes > 12) return BadRequest("El mes debe estar entre 1 y 12.");

            var inicio = new DateTime(anio.Value, mes.Value, 1);
            var fin = inicio.AddMonths(1);
            query = query.Where(t => t.Fecha >= inicio && t.Fecha < fin);
        }

        var lista = await query
            .OrderByDescending(t => t.Fecha)
            .ThenByDescending(t => t.Id)
            .Select(ToDto)
            .ToListAsync();

        return Ok(lista);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TransaccionDto>> GetById(int id)
    {
        var transaccion = await _context.Transacciones
            .Where(t => t.Id == id && t.UsuarioId == UsuarioId)
            .Select(ToDto)
            .FirstOrDefaultAsync();

        if (transaccion is null) return NotFound();
        return Ok(transaccion);
    }

    // GET api/transacciones/resumen?anio=2026&mes=10
    [HttpGet("resumen")]
    public async Task<ActionResult<ResumenMensualDto>> GetResumen([FromQuery] int? anio, [FromQuery] int? mes)
    {
        var hoy = DateTime.Today;
        var a = anio ?? hoy.Year;
        var m = mes ?? hoy.Month;

        if (m < 1 || m > 12) return BadRequest("El mes debe estar entre 1 y 12.");

        var inicio = new DateTime(a, m, 1);
        var fin = inicio.AddMonths(1);

        var delMes = _context.Transacciones
            .Where(t => t.UsuarioId == UsuarioId && t.Fecha >= inicio && t.Fecha < fin);

        var totalIngresos = await delMes
            .Where(t => t.Categoria!.Tipo == TipoMovimiento.Ingreso)
            .SumAsync(t => t.Monto);

        var totalGastos = await delMes
            .Where(t => t.Categoria!.Tipo == TipoMovimiento.Gasto)
            .SumAsync(t => t.Monto);

        var gastosPorCategoria = await delMes
            .Where(t => t.Categoria!.Tipo == TipoMovimiento.Gasto)
            .GroupBy(t => new { t.Categoria!.Nombre, t.Categoria.Color })
            .Select(g => new GastoPorCategoriaDto
            {
                Categoria = g.Key.Nombre,
                Color = g.Key.Color,
                Total = g.Sum(t => t.Monto)
            })
            .OrderByDescending(x => x.Total)
            .ToListAsync();

        return Ok(new ResumenMensualDto
        {
            Anio = a,
            Mes = m,
            TotalIngresos = totalIngresos,
            TotalGastos = totalGastos,
            Balance = totalIngresos - totalGastos,
            GastosPorCategoria = gastosPorCategoria
        });
    }

    [HttpPost]
    public async Task<ActionResult<TransaccionDto>> Create(TransaccionCreateDto dto)
    {
        var categoriaValida = await _context.Categorias
            .AnyAsync(c => c.Id == dto.CategoriaId && c.UsuarioId == UsuarioId);

        if (!categoriaValida) return BadRequest("La categoría no existe.");

        var transaccion = new Transaccion
        {
            Monto = dto.Monto,
            Fecha = dto.Fecha,
            Descripcion = dto.Descripcion,
            CategoriaId = dto.CategoriaId,
            UsuarioId = UsuarioId
        };

        _context.Transacciones.Add(transaccion);
        await _context.SaveChangesAsync();

        var result = await _context.Transacciones
            .Where(t => t.Id == transaccion.Id)
            .Select(ToDto)
            .FirstAsync();

        return CreatedAtAction(nameof(GetById), new { id = transaccion.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TransaccionCreateDto dto)
    {
        var transaccion = await _context.Transacciones
            .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == UsuarioId);

        if (transaccion is null) return NotFound();

        var categoriaValida = await _context.Categorias
            .AnyAsync(c => c.Id == dto.CategoriaId && c.UsuarioId == UsuarioId);

        if (!categoriaValida) return BadRequest("La categoría no existe.");

        transaccion.Monto = dto.Monto;
        transaccion.Fecha = dto.Fecha;
        transaccion.Descripcion = dto.Descripcion;
        transaccion.CategoriaId = dto.CategoriaId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var transaccion = await _context.Transacciones
            .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == UsuarioId);

        if (transaccion is null) return NotFound();

        _context.Transacciones.Remove(transaccion);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}