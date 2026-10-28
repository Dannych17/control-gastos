# 💰 Control de Gastos

API REST para gestionar finanzas personales: registra ingresos y gastos, organízalos por categorías y consulta un resumen mensual.

## Tecnologías

- C# / ASP.NET Core 9 (Web API)
- Entity Framework Core + SQL Server
- Autenticación con JWT y contraseñas con BCrypt
- Swagger (documentación interactiva)

## Funcionalidades

- Registro e inicio de sesión de usuarios
- Categorías de ingresos y gastos
- Transacciones con filtro por mes
- Resumen mensual: total de ingresos, gastos, balance y gastos por categoría
- Cada usuario solo ve sus propios datos

## Capturas

![Documentación Swagger](docs/swagger.png)

![Resumen mensual](docs/resumen.png)

## Endpoints principales

| Método       | Ruta                       | Descripción                      |
| ------------ | -------------------------- | -------------------------------- |
| POST         | /api/auth/register         | Crear cuenta                     |
| POST         | /api/auth/login            | Iniciar sesión (devuelve token)  |
| GET / POST   | /api/categorias            | Listar y crear categorías        |
| PUT / DELETE | /api/categorias/{id}       | Editar y borrar categoría        |
| GET / POST   | /api/transacciones         | Listar (con ?anio=&mes=) y crear |
| PUT / DELETE | /api/transacciones/{id}    | Editar y borrar transacción      |
| GET          | /api/transacciones/resumen | Resumen mensual                  |

## Cómo ejecutarlo

1. Clona el repositorio y entra a `backend/ControlGastos.Api`.
2. En `appsettings.json`, ajusta la cadena de conexión a tu servidor de SQL Server.
3. Configura la clave del token (se guarda fuera del repo):
   ```
   dotnet user-secrets init
   dotnet user-secrets set "Jwt:Key" "una-clave-aleatoria-de-mas-de-32-caracteres"
   ```
4. Crea la base de datos:
   ```
   dotnet ef database update
   ```
5. Ejecuta con `dotnet run` y abre `https://localhost:7125/swagger`.

## Próximos pasos

- [ ] Frontend con dashboard y gráficos
- [ ] Pruebas unitarias
- [ ] Docker y despliegue en la nube

## Autor

Danny Chilan · [GitHub](https://github.com/Dannych17)
