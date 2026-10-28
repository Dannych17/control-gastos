using System.ComponentModel.DataAnnotations;
using ControlGastos.Api.Models;

namespace ControlGastos.Api.Dtos;

public class TransaccionCreateDto
{
    [Range(0.01, 1000000000, ErrorMessage = "El monto debe ser mayor a 0.")]
    public decimal Monto { get; set; }

    public DateTime Fecha { get; set; }

    [StringLength(200)]
    public string? Descripcion { get; set; }

    public int CategoriaId { get; set; }
}

public class TransaccionDto
{
    public int Id { get; set; }
    public decimal Monto { get; set; }
    public DateTime Fecha { get; set; }
    public string? Descripcion { get; set; }
    public int CategoriaId { get; set; }
    public string CategoriaNombre { get; set; } = string.Empty;
    public TipoMovimiento Tipo { get; set; }
}

public class GastoPorCategoriaDto
{
    public string Categoria { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public decimal Total { get; set; }
}

public class ResumenMensualDto
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public decimal TotalIngresos { get; set; }
    public decimal TotalGastos { get; set; }
    public decimal Balance { get; set; }
    public List<GastoPorCategoriaDto> GastosPorCategoria { get; set; } = new();
}