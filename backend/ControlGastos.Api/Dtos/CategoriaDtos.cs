using System.ComponentModel.DataAnnotations;
using ControlGastos.Api.Models;

namespace ControlGastos.Api.Dtos;

public class CategoriaCreateDto
{
    [Required, StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    public TipoMovimiento Tipo { get; set; }

    [StringLength(7)]
    public string Color { get; set; } = "#3498db";
}

public class CategoriaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoMovimiento Tipo { get; set; }
    public string Color { get; set; } = string.Empty;
}