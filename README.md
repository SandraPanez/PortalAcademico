# Portal Académico

Sistema web para gestión de cursos, estudiantes y matrículas universitarias.

## Stack
- ASP.NET Core MVC (.NET 9)
- Entity Framework Core + SQLite
- ASP.NET Identity
- Redis (sesiones y cache)
- Render.com (deploy)

## Pasos para correr localmente

### 1. Clonar el repositorio
```bash
git clone https://github.com/SandraPanez/PortalAcademico.git
cd PortalAcademico
```

### 2. Configurar variables de entorno
Crea un archivo `appsettings.Development.json` con:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

### 3. Aplicar migraciones
```bash
dotnet ef database update
```

### 4. Correr el proyecto
```bash
dotnet run
```

## Variables de entorno en Render
| Variable | Valor |
|----------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_URLS` | `http://0.0.0.0:${PORT}` |
| `ConnectionStrings__DefaultConnection` | URL de tu base de datos |
| `Redis__ConnectionString` | URL de Redis |

## Usuario Coordinador por defecto
- **Email:** coordinador@portal.com
- **Password:** Coordinador123!

## URL en Render
(agregar URL cuando esté desplegado)