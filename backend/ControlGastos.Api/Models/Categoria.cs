namespace ControlGastos.Api.Models;

public class Categoria
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoMovimiento Tipo { get; set; }
    public string Color { get; set; } = "#3498db";

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public List<Transaccion> Transacciones { get; set; } = new();
}