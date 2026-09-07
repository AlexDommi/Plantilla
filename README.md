# PlantillaCapas

Plantilla base para construir APIs REST en **.NET 10** con arquitectura en capas.

La solución incluye un CRUD completo de ejemplo sobre la entidad `Ejemplo`. Ese recorrido —entidad, DTOs, interfaz, servicio y controller— es la referencia: cada entidad nueva se arma repitiendo exactamente los mismos pasos.

---

## Requisitos

- **.NET SDK 10.0** o superior
- **SQL Server** (local, Express, LocalDB o contenedor)
- **Herramienta de EF Core**, para generar migraciones:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

---

## Estructura de la solución

La solución (`PlantillaCapas.slnx`) agrupa cuatro proyectos bajo la carpeta `/src/`:

| Proyecto | Responsabilidad | Qué contiene |
|---|---|---|
| **Domain** | El modelo del negocio | Entidades (`Domain/Entities/`) |
| **Application** | El contrato de la aplicación | DTOs (`Application/Dtos/`) e interfaces de servicio (`Application/Interfaces/`) |
| **Infrastructure** | El acceso a datos y la implementación | `AppDbContext`, servicios (`Infrastructure/Services/`), migraciones y seeders |
| **WebAPI** | La entrada HTTP | Controllers, `Program.cs`, configuración |

### Dependencias entre capas

```
WebAPI  ──►  Infrastructure  ──►  Application  ──►  Domain
```

La regla que sostiene la plantilla:

- **Domain no referencia a nadie.** Es el centro y no conoce EF, ASP.NET ni ninguna otra capa.
- **Application sólo referencia a Domain.** Define *qué* hace la aplicación (interfaces y DTOs), no *cómo*.
- **Infrastructure implementa lo que Application declara.** Acá vive todo lo que sabe de base de datos.
- **WebAPI depende de las tres**, pero sus controllers consumen únicamente interfaces de `Application`.

---

## Puesta en marcha

### 1. Configurar la conexión

`WebAPI/appsettings.json` no está versionado: cada uno mantiene el suyo en local. El repo trae `WebAPI/appsettings.Example.json` como molde, así que el primer paso después de clonar es copiarlo:

```bash
cp WebAPI/appsettings.Example.json WebAPI/appsettings.json
```

Y completar ahí la clave `ConnectionStrings:DefaultConnection` con los datos del entorno:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PlantillaCapas;User Id=sa;Password=TU_PASSWORD;TrustServerCertificate=True;"
  }
}
```

Alternativamente, sin poner credenciales en ningún archivo, usando User Secrets:

```bash
dotnet user-secrets init --project WebAPI
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=PlantillaCapas;..." --project WebAPI
```

### 2. Crear la base de datos

Las migraciones se generan en `Infrastructure/Migrations/`. Como el `DbContext` vive en **Infrastructure** y el host es **WebAPI**, los comandos llevan siempre los dos proyectos:

```bash
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project WebAPI
dotnet ef database update --project Infrastructure --startup-project WebAPI
```

### 3. Levantar la API

```bash
dotnet run --project WebAPI
```

| Perfil | URL |
|---|---|
| `http` | http://localhost:5033 |
| `https` | https://localhost:7034 |

En entorno **Development** queda expuesto el documento OpenAPI en `/openapi/v1.json`.

Para probar los endpoints a mano está `WebAPI/WebAPI.http`, que se ejecuta directo desde Visual Studio o VS Code.

---

## El ejemplo de referencia

El CRUD de `Ejemplo` recorre las cuatro capas. Estos son los archivos, en el orden en que conviene leerlos:

| Capa | Archivo |
|---|---|
| Domain | `Domain/Entities/Ejemplo.cs` |
| Application | `Application/Dtos/Ejemplo/EjemploCreateDto.cs` |
| Application | `Application/Dtos/Ejemplo/EjemploReadDto.cs` |
| Application | `Application/Dtos/Ejemplo/EjemploUpdateDto.cs` |
| Application | `Application/Interfaces/IGenericService.cs` |
| Application | `Application/Interfaces/IEjemploService.cs` |
| Infrastructure | `Infrastructure/Data/AppDbContext.cs` |
| Infrastructure | `Infrastructure/Services/EjemploService.cs` |
| WebAPI | `WebAPI/Controllers/EjemploController.cs` |
| WebAPI | `WebAPI/Program.cs` |

### Endpoints que expone

| Método | Ruta | Respuestas |
|---|---|---|
| `GET` | `/api/Ejemplo` | `200` con la lista |
| `GET` | `/api/Ejemplo/{id}` | `200` con el recurso · `404` |
| `POST` | `/api/Ejemplo` | `201` con cabecera `Location` |
| `PUT` | `/api/Ejemplo/{id}` | `204` · `404` |
| `DELETE` | `/api/Ejemplo/{id}` | `204` · `404` |

---

## Cómo agregar una entidad nueva

Los pasos, tomando `Producto` como ejemplo. Se sigue siempre el mismo orden: de adentro hacia afuera.

### 1. La entidad — `Domain/Entities/Producto.cs`

```csharp
namespace Domain.Entities
{
    public class Producto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
    }
}
```

### 2. Los DTOs — `Application/Dtos/Producto/`

Uno por operación. **Create** trae lo que el cliente envía, **Read** lo que se devuelve, **Update** todo opcional para permitir actualizaciones parciales.

```csharp
// ProductoCreateDto.cs
public class ProductoCreateDto
{
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
}

// ProductoReadDto.cs
public class ProductoReadDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
}

// ProductoUpdateDto.cs
public class ProductoUpdateDto
{
    public string? Nombre { get; set; }
    public decimal? Precio { get; set; }
}
```

### 3. La interfaz — `Application/Interfaces/IProductoService.cs`

Hereda de `IGenericService`, que ya define el CRUD estándar. Los métodos propios de la entidad se agregan acá.

```csharp
using Application.Dtos.Producto;

namespace Application.Interfaces
{
    public interface IProductoService
        : IGenericService<ProductoCreateDto, ProductoReadDto, ProductoUpdateDto>
    {
        // Métodos específicos de Producto, por ejemplo:
        // Task<IEnumerable<ProductoReadDto>> GetByRangoPrecioAsync(decimal min, decimal max);
    }
}
```

### 4. El `DbSet` y su configuración — `Infrastructure/Data/AppDbContext.cs`

```csharp
public DbSet<Producto> Productos => Set<Producto>();

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Producto>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
        entity.Property(e => e.Precio).HasPrecision(18, 2);
    });
}
```

### 5. El servicio — `Infrastructure/Services/ProductoService.cs`

Se copia `EjemploService` y se adapta. Recibe el `AppDbContext` por constructor y devuelve siempre DTOs.

```csharp
public class ProductoService : IProductoService
{
    private readonly AppDbContext _context;

    public ProductoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductoReadDto>> GetAllAsync()
    {
        return await _context.Productos
            .AsNoTracking()
            .Select(p => new ProductoReadDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Precio = p.Precio
            })
            .ToListAsync();
    }

    // GetByIdAsync, AddAsync, UpdateAsync y DeleteAsync
    // siguen la misma forma que en EjemploService.
}
```

### 6. El registro en el contenedor — `WebAPI/Program.cs`

```csharp
builder.Services.AddScoped<IProductoService, ProductoService>();
```

### 7. El controller — `WebAPI/Controllers/ProductoController.cs`

Recibe la interfaz, nunca el servicio concreto ni el `DbContext`.

```csharp
[Route("api/[controller]")]
[ApiController]
public class ProductoController : ControllerBase
{
    private readonly IProductoService _productoService;

    public ProductoController(IProductoService productoService)
    {
        _productoService = productoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var productos = await _productoService.GetAllAsync();
        return Ok(productos);
    }

    // El resto de las acciones replica EjemploController.
}
```

### 8. La migración

```bash
dotnet ef migrations add AgregarProducto --project Infrastructure --startup-project WebAPI
dotnet ef database update --project Infrastructure --startup-project WebAPI
```

---

## Convenciones de trabajo

Para que todo el código de la solución se lea igual:

**Nombres**

- Entidad en singular: `Producto`. `DbSet` en plural: `Productos`.
- DTOs: `{Entidad}{Operación}Dto` — `ProductoCreateDto`, `ProductoReadDto`, `ProductoUpdateDto`.
- Servicios: `{Entidad}Service`, con su interfaz `I{Entidad}Service`.
- Los DTOs de cada entidad van en su propia subcarpeta: `Application/Dtos/{Entidad}/`.

**Capas**

- Las entidades de `Domain` no salen de `Infrastructure`. Hacia afuera viajan DTOs.
- Los controllers dependen de interfaces de `Application`, nunca de `Infrastructure` ni del `AppDbContext`.
- El mapeo entre entidad y DTO se hace dentro del servicio.

**Servicios**

- Todos los métodos son `async` y terminan en `Async`.
- Las lecturas usan `AsNoTracking()`.
- Los listados proyectan con `.Select(...)` para traer sólo las columnas necesarias.
- En `UpdateAsync`, una propiedad en `null` significa "no modificar ese campo".

**Contratos de retorno**

Los servicios comunican el resultado por el valor de retorno, y el controller lo traduce a HTTP:

| Método del servicio | Devuelve | El controller responde |
|---|---|---|
| `GetAllAsync` | La colección | `200 Ok` |
| `GetByIdAsync` | El DTO, o `null` si no existe | `200 Ok` · `404 NotFound` |
| `AddAsync` | El `Guid` generado | `201 CreatedAtAction` |
| `UpdateAsync` | `true` si existía | `204 NoContent` · `404 NotFound` |
| `DeleteAsync` | `true` si existía | `204 NoContent` · `404 NotFound` |

**Rutas**

- Patrón `api/[controller]`, con `[ApiController]` en la clase.
- Los identificadores llevan restricción de tipo: `[HttpGet("{id:guid}")]`.

---

## Comandos frecuentes

```bash
# Compilar toda la solución
dotnet build PlantillaCapas.slnx

# Levantar la API
dotnet run --project WebAPI

# Nueva migración
dotnet ef migrations add NombreDeLaMigracion --project Infrastructure --startup-project WebAPI

# Aplicar migraciones pendientes
dotnet ef database update --project Infrastructure --startup-project WebAPI

# Revertir la última migración (si todavía no se aplicó a la base)
dotnet ef migrations remove --project Infrastructure --startup-project WebAPI
```

---

## Paquetes incluidos

| Proyecto | Paquete | Versión |
|---|---|---|
| Infrastructure | `Microsoft.EntityFrameworkCore` | 10.0.11 |
| Infrastructure | `Microsoft.EntityFrameworkCore.SqlServer` | 10.0.11 |
| Infrastructure | `Microsoft.EntityFrameworkCore.Tools` | 10.0.11 |
| WebAPI | `Microsoft.AspNetCore.OpenApi` | 10.0.11 |
| WebAPI | `Microsoft.EntityFrameworkCore.Design` | 10.0.11 |
