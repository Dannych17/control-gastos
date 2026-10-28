using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

using ControlGastos.Api.Data;
using ControlGastos.Api.Dtos;
using ControlGastos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly AppDbContext _context;

    // TEMPORAL: se reemplazará por el usuario del token JWT
    private int UsuarioId => int.Parse(User.FindFirstValue("sub")!);

    public CategoriasController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<CategoriaDto>>> GetAll()
    {
        var categorias = await _context.Categorias
            .Where(c => c.UsuarioId == UsuarioId)
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                Tipo = c.Tipo,
                Color = c.Color
            })
            .ToListAsync();

        return Ok(categorias);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoriaDto>> GetById(int id)
    {
        var c = await _context.Categorias
            .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == UsuarioId);

        if (c is null) return NotFound();

        return Ok(new CategoriaDto { Id = c.Id, Nombre = c.Nombre, Tipo = c.Tipo, Color = c.Color });
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaDto>> Create(CategoriaCreateDto dto)
    {
        var categoria = new Categoria
        {
            Nombre = dto.Nombre,
            Tipo = dto.Tipo,
            Color = dto.Color,
            UsuarioId = UsuarioId
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        var result = new CategoriaDto
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Tipo = categoria.Tipo,
            Color = categoria.Color
        };

        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CategoriaCreateDto dto)
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == UsuarioId);

        if (categoria is null) return NotFound();

        categoria.Nombre = dto.Nombre;
        categoria.Tipo = dto.Tipo;
        categoria.Color = dto.Color;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias
            .FirstOrDefaultAsync(x => x.Id == id && x.UsuarioId == UsuarioId);

        if (categoria is null) return NotFound();

        var tieneTransacciones = await _context.Transacciones.AnyAsync(t => t.CategoriaId == id);
        if (tieneTransacciones)
            return Conflict("No se puede eliminar una categoría que tiene transacciones.");

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}