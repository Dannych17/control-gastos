using System.Security.Claims;
using ControlGastos.Api.Controllers;
using ControlGastos.Api.Data;
using ControlGastos.Api.Dtos;
using ControlGastos.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlGastos.Tests;

public class TransaccionesControllerTests
{
    private static AppDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AppDbContext(options);
        Sembrar(db);
        return db;
    }

    // Datos de prueba: el usuario 1 tiene movimientos en octubre y septiembre;
    // el usuario 2 tiene un gasto grande que el usuario 1 nunca debe ver.
    private static void Sembrar(AppDbContext db)
    {
        db.Usuarios.AddRange(
            new Usuario { Id = 1, Nombre = "Ana", Email = "ana@test.com", PasswordHash = "x" },
            new Usuario { Id = 2, Nombre = "Luis", Email = "luis@test.com", PasswordHash = "x" });

        db.Categorias.AddRange(
            new Categoria { Id = 1, Nombre = "Sueldo", Tipo = TipoMovimiento.Ingreso, Color = "#2ecc71", UsuarioId = 1 },
            new Categoria { Id = 2, Nombre = "Comida", Tipo = TipoMovimiento.Gasto, Color = "#e67e22", UsuarioId = 1 },
            new Categoria { Id = 3, Nombre = "Transporte", Tipo = TipoMovimiento.Gasto, Color = "#3498db", UsuarioId = 1 },
            new Categoria { Id = 4, Nombre = "Otros", Tipo = TipoMovimiento.Gasto, Color = "#000000", UsuarioId = 2 });

        db.Transacciones.AddRange(
            new Transaccion { Monto = 1200m, Fecha = new DateTime(2026, 10, 1), CategoriaId = 1, UsuarioId = 1 },
            new Transaccion { Monto = 85.50m, Fecha = new DateTime(2026, 10, 5), CategoriaId = 2, UsuarioId = 1 },
            new Transaccion { Monto = 25m, Fecha = new DateTime(2026, 10, 10), CategoriaId = 3, UsuarioId = 1 },
            new Transaccion { Monto = 40m, Fecha = new DateTime(2026, 9, 15), CategoriaId = 2, UsuarioId = 1 },
            new Transaccion { Monto = 999m, Fecha = new DateTime(2026, 10, 3), CategoriaId = 4, UsuarioId = 2 });

        db.SaveChanges();
    }

    // Crea el controlador simulando que el usuario "usuarioId" inició sesión.
    private static TransaccionesController CrearControlador(AppDbContext db, int usuarioId)
    {
        var identity = new ClaimsIdentity(new[] { new Claim("sub", usuarioId.ToString()) }, "test");

        return new TransaccionesController(db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            }
        };
    }

    [Fact]
    public async Task Resumen_CalculaIngresosGastosYBalanceDelMes()
    {
        using var db = CrearContexto();
        var controller = CrearControlador(db, usuarioId: 1);

        var resultado = await controller.GetResumen(2026, 10);

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var resumen = Assert.IsType<ResumenMensualDto>(ok.Value);
        Assert.Equal(1200m, resumen.TotalIngresos);
        Assert.Equal(110.50m, resumen.TotalGastos);
        Assert.Equal(1089.50m, resumen.Balance);
    }

    [Fact]
    public async Task Resumen_AgrupaGastosPorCategoriaDeMayorAMenor()
    {
        using var db = CrearContexto();
        var controller = CrearControlador(db, usuarioId: 1);

        var resultado = await controller.GetResumen(2026, 10);

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var resumen = Assert.IsType<ResumenMensualDto>(ok.Value);
        Assert.Equal(2, resumen.GastosPorCategoria.Count);
        Assert.Equal("Comida", resumen.GastosPorCategoria[0].Categoria);
        Assert.Equal(85.50m, resumen.GastosPorCategoria[0].Total);
        Assert.Equal("Transporte", resumen.GastosPorCategoria[1].Categoria);
    }

    [Fact]
    public async Task Resumen_NoIncluyeDatosDeOtroUsuario()
    {
        using var db = CrearContexto();
        var controller = CrearControlador(db, usuarioId: 2);

        var resultado = await controller.GetResumen(2026, 10);

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var resumen = Assert.IsType<ResumenMensualDto>(ok.Value);
        Assert.Equal(0m, resumen.TotalIngresos);
        Assert.Equal(999m, resumen.TotalGastos);
    }

    [Fact]
    public async Task Resumen_MesInvalido_DevuelveBadRequest()
    {
        using var db = CrearContexto();
        var controller = CrearControlador(db, usuarioId: 1);

        var resultado = await controller.GetResumen(2026, 13);

        Assert.IsType<BadRequestObjectResult>(resultado.Result);
    }

    [Fact]
    public async Task GetAll_FiltraPorMes()
    {
        using var db = CrearContexto();
        var controller = CrearControlador(db, usuarioId: 1);

        var octubre = await controller.GetAll(2026, 10);
        var septiembre = await controller.GetAll(2026, 9);

        var listaOctubre = Assert.IsType<List<TransaccionDto>>(
            Assert.IsType<OkObjectResult>(octubre.Result).Value);
        var listaSeptiembre = Assert.IsType<List<TransaccionDto>>(
            Assert.IsType<OkObjectResult>(septiembre.Result).Value);

        Assert.Equal(3, listaOctubre.Count);
        Assert.Single(listaSeptiembre);
    }

    [Fact]
    public async Task Create_ConCategoriaDeOtroUsuario_DevuelveBadRequest()
    {
        using var db = CrearContexto();
        var controller = CrearControlador(db, usuarioId: 1);

        var dto = new TransaccionCreateDto
        {
            Monto = 10m,
            Fecha = new DateTime(2026, 10, 20),
            CategoriaId = 4 // pertenece al usuario 2
        };

        var resultado = await controller.Create(dto);

        Assert.IsType<BadRequestObjectResult>(resultado.Result);
        Assert.Equal(5, await db.Transacciones.CountAsync()); // no se guardó nada
    }
}