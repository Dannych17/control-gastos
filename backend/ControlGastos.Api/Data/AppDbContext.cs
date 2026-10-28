using ControlGastos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Transaccion> Transacciones => Set<Transaccion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Transaccion>()
            .Property(t => t.Monto)
            .HasPrecision(18, 2);

        // Evita que SQL Server rechace la migración por "múltiples rutas de eliminación en cascada"
        modelBuilder.Entity<Transaccion>()
            .HasOne(t => t.Categoria)
            .WithMany(c => c.Transacciones)
            .HasForeignKey(t => t.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}