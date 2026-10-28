using ControlGastos.Api.Controllers;
using ControlGastos.Api.Data;
using ControlGastos.Api.Dtos;
using ControlGastos.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ControlGastos.Tests;

public class AuthControllerTests
{
    private static AuthController CrearControlador(AppDbContext db)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = new string('k', 40),
                ["Jwt:Issuer"] = "pruebas",
                ["Jwt:Audience"] = "pruebas"
            })
            .Build();

        return new AuthController(db, new TokenService(config));
    }

    private static AppDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task Register_CorreoNuevo_DevuelveTokenYGuardaHashDeLaContrasena()
    {
        using var db = CrearContexto();
        var controller = CrearControlador(db);

        var resultado = await controller.Register(new RegisterDto
        {
            Nombre = "Ana",
            Email = "ana@test.com",
            Password = "secreto123"
        });

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var respuesta = Assert.IsType<AuthResponseDto>(ok.Value);
        Assert.False(string.IsNullOrEmpty(respuesta.Token));

        var usuario = await db.Usuarios.SingleAsync();
        Assert.NotEqual("secreto123", usuario.PasswordHash); // nunca en texto plano
    }

    [Fact]
    public async Task Register_CorreoRepetido_DevuelveConflict()
    {
        using var db = CrearContexto();
        var controller = CrearControlador(db);
        await controller.Register(new RegisterDto { Nombre = "Ana", Email = "Ana@Test.com", Password = "secreto123" });

        // mismo correo con otras mayúsculas: debe considerarse el mismo
        var resultado = await controller.Register(new RegisterDto
        {
            Nombre = "Otra Ana",
            Email = "ana@test.com",
            Password = "otraclave1"
        });

        Assert.IsType<ConflictObjectResult>(resultado.Result);
    }

    [Fact]
    public async Task Login_ConDatosCorrectos_DevuelveToken()
    {
        using var db = CrearContexto();
        var controller = CrearControlador(db);
        await controller.Register(new RegisterDto { Nombre = "Ana", Email = "ana@test.com", Password = "secreto123" });

        var resultado = await controller.Login(new LoginDto { Email = "ana@test.com", Password = "secreto123" });

        var ok = Assert.IsType<OkObjectResult>(resultado.Result);
        var respuesta = Assert.IsType<AuthResponseDto>(ok.Value);
        Assert.Equal("Ana", respuesta.Nombre);
    }

    [Fact]
    public async Task Login_ContrasenaIncorrecta_DevuelveUnauthorized()
    {
        using var db = CrearContexto();
        var controller = CrearControlador(db);
        await controller.Register(new RegisterDto { Nombre = "Ana", Email = "ana@test.com", Password = "secreto123" });

        var resultado = await controller.Login(new LoginDto { Email = "ana@test.com", Password = "equivocada" });

        Assert.IsType<UnauthorizedObjectResult>(resultado.Result);
    }
}