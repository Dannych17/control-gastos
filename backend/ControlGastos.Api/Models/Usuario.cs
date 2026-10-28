namespace ControlGastos.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public List<Categoria> Categorias { get; set; } = new();
    public List<Transaccion> Transacciones { get; set; } = new();
}