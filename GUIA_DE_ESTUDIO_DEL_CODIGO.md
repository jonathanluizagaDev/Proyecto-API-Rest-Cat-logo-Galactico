# Guía de estudio del código — Catálogo Galáctico

Esta guía está escrita para leer el proyecto y comprenderlo, no solo para instalarlo. Explica la sintaxis utilizada, la responsabilidad de cada archivo, lo que hace cada método y el recorrido completo desde Swagger hasta MySQL.

La documentación técnica resumida está en [`DOCUMENTACION_TECNICA.md`](DOCUMENTACION_TECNICA.md). Esta guía es la versión didáctica y detallada.

## 1. Orden recomendado de lectura

No conviene empezar por el archivo más largo. El orden más fácil es:

1. [`CatalogoGalacticoWebApi.csproj`](CatalogoGalacticoWebApi.csproj): tecnologías usadas.
2. [`Program.cs`](Program.cs): cómo arranca la aplicación.
3. [`Models/`](Models): qué datos existen.
4. [`Data/AppDbContext.cs`](Data/AppDbContext.cs): cómo se relacionan esos datos.
5. [`Dtos/Inputs.cs`](Dtos/Inputs.cs): qué JSON entra y sale.
6. [`Services/Resultado.cs`](Services/Resultado.cs) e interfaces: contratos internos.
7. [`Services/CatalogoService.cs`](Services/CatalogoService.cs): CRUD y validaciones.
8. [`Services/SimulacionService.cs`](Services/SimulacionService.cs): batallas e invasiones.
9. [`Services/UsuarioService.cs`](Services/UsuarioService.cs) y [`CifradoService.cs`](Services/CifradoService.cs): protección de datos.
10. [`Controllers/`](Controllers): rutas que aparecen en Swagger.
11. [`Migrations/`](Migrations): traducción del modelo a MySQL.

## 2. Mapa mental del sistema

```text
Swagger o cliente HTTP
        |
        | solicitud: ruta + JSON
        v
Controller
        |
        | llama una interfaz
        v
Service
        |
        | valida y aplica reglas
        v
AppDbContext / Entity Framework
        |
        | genera SQL
        v
MySQL
```

La respuesta realiza el camino inverso:

```text
MySQL -> EF Core -> Service -> Resultado<T> -> Controller -> JSON
```

Una idea importante: el controlador no decide las reglas del negocio. Solo traduce HTTP. El servicio contiene la lógica y `AppDbContext` contiene el acceso a datos.

## 3. Sintaxis de C# que debes reconocer

### `using`

```csharp
using Microsoft.EntityFrameworkCore;
```

Permite usar clases de otro espacio de nombres sin escribir su nombre completo. Por ejemplo, se escribe `DbContext` en vez de `Microsoft.EntityFrameworkCore.DbContext`.

### `namespace`

```csharp
namespace CatalogoGalacticoWebApi.Models;
```

Agrupa clases y evita conflictos de nombres. El punto y coma indica la sintaxis de *file-scoped namespace*: todo lo que sigue en el archivo pertenece a ese espacio.

### Clase y propiedades

```csharp
public class Personaje
{
    public int Id { get; set; }
}
```

- `public`: se puede acceder desde otras clases.
- `class`: define un tipo de objeto.
- `int`: número entero.
- `get`: permite leer la propiedad.
- `set`: permite cambiarla.

### Valor inicial

```csharp
public string Estado { get; set; } = "vivo";
```

Un personaje nuevo comienza con `Estado` igual a `vivo`, salvo que luego se asigne otro valor.

### `?` y `!`

```csharp
public Carta? Carta { get; set; }
public Personaje Personaje { get; set; } = null!;
```

- `Carta?` significa que puede ser una carta o `null`.
- `null!` le dice al compilador: «esta propiedad comienza nula, pero Entity Framework la completará». No convierte el valor en no nulo; solamente silencia la advertencia de nulabilidad.

### Lista vacía

```csharp
public List<Participacion> Participaciones { get; set; } = new();
```

`new()` crea la lista usando el tipo que el compilador ya conoce. Evita que la propiedad empiece en `null`.

### `record`

```csharp
public record UsuarioResponse(int Id, string Nombre, string Correo);
```

Un `record` es apropiado para transportar datos. El compilador crea constructor y propiedades. En este ejemplo se instancia con:

```csharp
new UsuarioResponse(1, "Leia", "leia@ejemplo.com")
```

### Constructor primario

```csharp
public class CatalogoService(AppDbContext db) : ICatalogoService
```

`AppDbContext db` es una dependencia que ASP.NET entrega al crear el servicio. `: ICatalogoService` significa que la clase cumple ese contrato.

### Método asíncrono

```csharp
public async Task<Planeta?> Buscar(int id)
```

- `async`: el método puede usar `await`.
- `Task<Planeta?>`: devolverá un planeta o `null` en el futuro.
- `await`: espera una operación sin bloquear el servidor mientras MySQL responde.

### Expresión lambda

```csharp
x => x.Id == id
```

Se lee: «para cada `x`, comprobar si su ID es igual a `id`». EF Core convierte muchas expresiones lambda a SQL.

### Método de expresión

```csharp
public Task<Personaje?> Personaje(int id) =>
    db.Personajes.FirstOrDefaultAsync(x => x.Id == id);
```

`=>` permite escribir en una línea un método que devuelve directamente una expresión.

### Operador ternario

```csharp
condicion ? valorSiEsVerdadero : valorSiEsFalso
```

Es una forma corta de `if/else`. El proyecto lo usa para respuestas y validaciones.

### Operadores de null

```csharp
configuracion["Cifrado:Clave"] ?? throw new InvalidOperationException(...)
```

`??` usa la parte derecha cuando la izquierda es `null`. Aquí la aplicación se detiene con un mensaje claro si falta la clave.

```csharp
s?.Trim()
```

`?.` ejecuta `Trim()` solamente si `s` no es `null`.

### Pattern matching

```csharp
d.NivelPeligro is < 1 or > 10
```

Se lee: «el nivel es menor que 1 o mayor que 10».

### LINQ

El proyecto usa LINQ para consultar y transformar datos:

| Operación | Significado |
|---|---|
| `Where(...)` | filtrar |
| `Select(...)` | transformar o elegir columnas |
| `Any(...)` | comprobar si existe al menos uno |
| `FirstOrDefault(...)` | tomar el primero o `null` |
| `OrderBy(...)` | ordenar ascendente |
| `OrderByDescending(...)` | ordenar descendente |
| `ThenBy(...)` | segundo criterio de orden |
| `GroupBy(...)` | agrupar |
| `Include(...)` | cargar una relación |
| `Distinct()` | quitar repetidos |
| `Count()` | contar |
| `Sum(...)` | sumar |
| `ToListAsync()` | ejecutar la consulta y crear una lista |

### `AsNoTracking()`

Se usa cuando los datos solo se van a leer. EF Core no necesita vigilar cambios, por lo que la consulta consume menos recursos.

### Atributos entre corchetes

```csharp
[Required]
[Range(1, 10)]
[JsonIgnore]
[HttpGet]
```

Son metadatos que modifican el comportamiento de validación, JSON o ASP.NET.

## 4. Archivo del proyecto: `CatalogoGalacticoWebApi.csproj`

Este XML le dice a .NET cómo compilar el proyecto.

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
```

Indica que es una aplicación web ASP.NET Core.

| Elemento | Explicación |
|---|---|
| `TargetFramework net10.0` | compila para .NET 10 |
| `Nullable enable` | activa advertencias para posibles valores `null` |
| `ImplicitUsings enable` | importa automáticamente espacios comunes |
| `UserSecretsId` | identifica el almacén local de secretos |
| `Microsoft.AspNetCore.OpenApi` | genera el documento OpenAPI |
| `EntityFrameworkCore.Design` | habilita comandos de migraciones |
| `Pomelo.EntityFrameworkCore.MySql` | conecta EF Core con MySQL |
| `Swashbuckle.AspNetCore.SwaggerUI` | muestra Swagger en el navegador |

`PrivateAssets=all` en el paquete de diseño significa que se usa para desarrollar, pero no se publica como dependencia de la aplicación.

## 5. Arranque: `Program.cs`

Este archivo se ejecuta primero.

### 5.1 Crear el constructor de la aplicación

```csharp
var builder = WebApplication.CreateBuilder(args);
```

`builder` reúne configuración, registro de servicios y logging.

### 5.2 Configurar los logs

```csharp
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
```

Quita proveedores anteriores y deja los mensajes en la terminal.

### 5.3 Leer la conexión

```csharp
var conexion = builder.Configuration.GetConnectionString("DefaultConnection") ??
               throw new InvalidOperationException(...);
```

Busca `ConnectionStrings:DefaultConnection`. En desarrollo proviene de Secret Manager. Si falta, falla al arrancar en vez de producir un error confuso al consultar MySQL.

### 5.4 Registrar `AppDbContext`

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(conexion, new MySqlServerVersion(new Version(8, 4, 9))));
```

Registra un contexto por solicitud y configura MySQL 8.4.9. Los servicios podrán pedir `AppDbContext` en su constructor.

### 5.5 Registrar controladores y servicios

```csharp
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
```

Se lee: «cuando una clase solicite `ICatalogoService`, entregar un `CatalogoService`». `Scoped` crea una instancia para cada solicitud HTTP.

`CifradoService` es `Singleton`: existe una sola instancia durante la vida de la aplicación porque conserva la clave en memoria y no mantiene estado por usuario.

### 5.6 Construir y mapear

```csharp
var app = builder.Build();
```

Desde aquí ya no se registran dependencias; se configura el flujo HTTP.

OpenAPI y Swagger se habilitan solamente cuando el ambiente es `Development`. `app.MapControllers()` encuentra los atributos `[Route]`, `[HttpGet]`, etc. `app.Run()` inicia el servidor y espera solicitudes.

## 6. Configuración local

### `appsettings.json`

Contiene niveles de log y `AllowedHosts`. No contiene contraseñas.

### `appsettings.Development.json`

Sobrescribe configuración cuando el ambiente es `Development`. Actualmente conserva los mismos niveles de log.

### `Properties/launchSettings.json`

El perfil `http`:

- ejecuta el proyecto;
- abre el navegador;
- abre la ruta `swagger`;
- establece `ASPNETCORE_ENVIRONMENT=Development`;
- escucha en `http://localhost:5080`.

No se usa en producción ni define la estructura de la base de datos.

## 7. Modelos: las entidades de la base de datos

Un modelo representa una tabla. Una propiedad normal suele convertirse en columna; una propiedad de navegación representa una relación.

### 7.1 `Models/Personaje.cs`

Representa la tabla `Personajes`.

| Propiedad | Uso |
|---|---|
| `Id` | clave primaria autoincremental |
| `Nombre` | nombre visible |
| `Especie` | especie del personaje |
| `Faccion` | `Rebelde`, `Imperio` o `Neutral` |
| `Afiliacion` | organización o grupo |
| `Estado` | `vivo`, `muerto` o `desconocido` |
| `FuerzaSensitivo` | verdadero o falso |
| `Carta?` | navegación opcional 1 a 0..1 |
| `Participaciones` | eventos en los que participa |

`Carta` y `Participaciones` tienen `[JsonIgnore]` para que devolver un personaje no produzca un árbol enorme ni un ciclo JSON.

### 7.2 `Models/Carta.cs`

Cada carta pertenece a un personaje mediante `PersonajeId`.

| Propiedad | Uso |
|---|---|
| `Poder` | valor principal para batallas y ranking |
| `HabilidadEspecial` | descripción de la habilidad |
| `Arma` | arma utilizada |
| `NivelPeligrosidad` | valor entre 1 y 10 |
| `ImagenUrl` | URL HTTP/HTTPS |
| `Personaje` | navegación hacia el dueño |

La regla de una sola carta por personaje no depende solamente de C#: la relación uno a uno hace que `PersonajeId` sea único en la base.

### 7.3 `Models/Planeta.cs`

Representa el lugar de los eventos. `NivelPeligro` participa en la fórmula de simulación. `Eventos` es la navegación uno a muchos y no se serializa directamente.

### 7.4 `Models/Evento.cs`

| Propiedad | Uso |
|---|---|
| `Fecha` | año galáctico, por eso es `int` y no `DateTime` |
| `Estado` | empieza `pendiente` y pasa a `simulado` |
| `PlanetaId` | clave foránea |
| `Planeta` | navegación al planeta |
| `Participaciones` | personajes del evento |
| `Simulaciones` | historial de ejecuciones |

`Participaciones` sí puede aparecer en algunas respuestas de entidad. `Simulaciones` y `Planeta` están ignorados para evitar ciclos; el detalle completo se construye con un DTO.

### 7.5 `Models/Participacion.cs`

Es la tabla intermedia de la relación muchos a muchos entre `Evento` y `Personaje`.

Su clave es compuesta: `EventoId + PersonajeId`. Esto impide inscribir dos veces al mismo personaje en el mismo evento.

`Resultado` puede ser:

- `pendiente` antes de simular;
- `sobreviviente` después de sobrevivir;
- `fallecido` si fue una baja.

```csharp
[NotMapped]
public bool Fallecido => Resultado == "fallecido";
```

`Fallecido` es una propiedad calculada para comodidad. `[NotMapped]` significa que EF Core no crea una columna para ella.

### 7.6 `Models/Simulacion.cs`

Guarda una fotografía histórica de cada batalla.

| Propiedad | Uso |
|---|---|
| `EventoId` | evento simulado |
| `Tipo` | `batalla` o `invasion` |
| `Ganador` | nombre de la facción ganadora |
| `Criterio` | explicación textual del cálculo |
| `TotalesJson` | poder de las facciones en ese momento |
| `CantidadDeBajas` | cantidad de fallecidos de esa ejecución |
| `FallecidosJson` | IDs de fallecidos |
| `SobrevivientesJson` | IDs de sobrevivientes |
| `FechaEjecucionUtc` | fecha real de ejecución en UTC |

Los JSON conservan la historia aunque después cambien otros datos del catálogo.

### 7.7 `Models/Usuario.cs`

No guarda el correo ni la contraseña en texto plano.

| Campo | Contenido |
|---|---|
| `CorreoBusquedaHash` | HMAC determinista para buscar duplicados |
| `CorreoCifrado` | correo cifrado recuperable |
| `PasswordHash` | hash no reversible de contraseña |

La entidad existe para demostrar almacenamiento protegido. El proyecto actual no implementa inicio de sesión ni JWT.

## 8. `Data/AppDbContext.cs`

`AppDbContext` es el puente entre objetos C# y tablas MySQL.

### Constructor

```csharp
public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
```

Recibe las opciones configuradas en `Program.cs` y las pasa al `DbContext` base.

### `DbSet`

```csharp
public DbSet<Personaje> Personajes => Set<Personaje>();
```

Representa la tabla y es el punto de inicio para consultas como `db.Personajes.Where(...)`.

### `OnModelCreating`

Este método configura reglas que no se deducen completamente de las clases.

#### Personaje

Nombre, facción y estado son obligatorios y tienen longitudes máximas. Esto limita las columnas y también protege la base.

#### Carta ↔ Personaje

```csharp
e.HasOne(c => c.Personaje)
    .WithOne(p => p.Carta)
    .HasForeignKey<Carta>(c => c.PersonajeId)
    .OnDelete(DeleteBehavior.Restrict);
```

Se lee: una carta tiene un personaje; un personaje tiene una carta; la clave foránea vive en `Carta.PersonajeId`; no se puede borrar el personaje si su carta depende de él.

#### Planeta

El índice único de `Nombre` evita planetas duplicados incluso si una validación de C# fuera omitida.

#### Evento ↔ Planeta

Muchos eventos pueden pertenecer a un planeta. `Restrict` impide borrar un planeta usado.

#### Participación

```csharp
e.HasKey(p => new { p.EventoId, p.PersonajeId });
```

La clave primaria usa dos columnas. Al eliminar un evento, sus participaciones se eliminan en cascada. Al eliminar un personaje se aplica `Restrict` para proteger el historial.

#### Simulación

Un evento tiene muchas simulaciones. `EventoId` es la clave foránea y se restringe la eliminación del evento con historial.

#### Usuario

`CorreoBusquedaHash` tiene índice único. Los tres campos protegidos son obligatorios.

## 9. `Dtos/Inputs.cs`: contratos JSON

DTO significa *Data Transfer Object*. Un DTO define exactamente qué datos acepta o devuelve una operación. Así la API no necesita exponer directamente todas las columnas de una entidad.

### Validaciones automáticas

```csharp
[Required] string Nombre
[Range(1, 10)] int NivelPeligro
[EmailAddress] string Correo
[MinLength(8)] string Contrasena
```

Como los controladores usan `[ApiController]`, ASP.NET revisa estos atributos antes de ejecutar el método. Si el JSON no cumple, devuelve 400 automáticamente.

Los servicios vuelven a comprobar reglas importantes porque los atributos no pueden expresar toda la lógica del negocio y porque un servicio también podría ser llamado fuera de un controlador.

### DTOs de entrada

#### `PersonajeInput`

Se usa para crear y editar personajes. No incluye `Id`: el ID lo genera MySQL al crear y viene en la ruta al editar.

#### `CartaInput`

Incluye `PersonajeId` para indicar el dueño. `[Range]` comprueba poder y peligrosidad. El servicio además valida que el personaje exista, que no tenga otra carta y que la imagen sea una URL.

#### `PlanetaInput`

Contiene los cuatro datos editables del planeta. El nombre único se valida contra la base, no mediante un atributo.

#### `EventoInput`

`ParticipanteIds` recibe una lista de IDs. `Estado` tiene valor predeterminado `pendiente`. Aunque el cliente enviara otro estado, el servicio lo rechaza: el estado cambia por la simulación, no por decisión del cliente.

#### `InvasionInput`

Declara facción atacante, defensora y participantes. El planeta no está en el cuerpo porque su ID viene en la ruta `/api/planetas/{id}/simular-invasion`.

#### `UsuarioInput`

Acepta nombre, correo con formato válido y contraseña mínima de ocho caracteres. La contraseña se usa para generar un hash y nunca se devuelve.

### DTOs de salida

| Record | Propósito |
|---|---|
| `UsuarioResponse` | devuelve usuario sin hashes ni contraseña |
| `RankingResponse` | posición, personaje y poder |
| `TotalFaccionResponse` | poder base, factor y poder final |
| `ResultadoSimulacion` | resultado completo de una ejecución |
| `PlanetaEventoResponse` | planeta anidado en el detalle de evento |
| `ParticipanteEventoResponse` | participante, estado, resultado y poder |
| `EventoDetalleResponse` | evento con planeta y participantes |

Separar respuestas es especialmente importante para `Usuario`: si se devolviera la entidad, se podrían filtrar accidentalmente `PasswordHash` o `CorreoBusquedaHash`.

## 10. `Services/Resultado.cs`

```csharp
public record Resultado<T>(T? Valor, int Codigo, string? Error = null)
```

Es el sobre interno que usan los servicios.

- `T` es el tipo variable: puede ser `Personaje`, `Carta`, `bool`, etc.
- `Valor` contiene el resultado correcto.
- `Codigo` contiene el código HTTP que debería usarse.
- `Error` contiene el mensaje cuando algo falla.

Las fábricas evitan repetir constructores:

```csharp
Resultado<Personaje>.Ok(personaje, 201)
Resultado<Personaje>.Fail("No encontrado", 404)
```

`Ok` usa 200 por defecto y `Fail` usa 400 por defecto. Los servicios especifican 201, 404 o 409 cuando corresponde.

## 11. Interfaces de servicios

Una interfaz dice qué operaciones existen sin indicar cómo se ejecutan.

### `ICatalogoService`

Declara operaciones de personajes, cartas, planetas y eventos. El controlador depende de esta interfaz, mientras `CatalogoService` contiene la implementación.

### `ISimulacionService`

Declara simulación, último resultado, historial, invasión y estadísticas.

### `IUsuarioService`

Declara creación y consulta segura de usuarios.

### `ICifradoService`

Declara cifrar, descifrar y calcular la huella de correo.

La ventaja práctica es desacoplar las capas. En pruebas futuras se podría reemplazar una implementación real por una falsa sin cambiar el controlador.

## 12. `Services/CatalogoService.cs`

Es el servicio más grande porque contiene cuatro grupos de CRUD. Recibe `AppDbContext db` mediante inyección.

### 12.1 Ayudantes de personajes

```csharp
private static readonly string[] Facciones = ["Rebelde", "Imperio", "Neutral"];
```

Son las opciones permitidas. `readonly` evita reemplazar el arreglo después de inicializarlo.

```csharp
private static string? Limpiar(string? s) => s?.Trim();
```

Quita espacios al inicio y al final si el texto existe.

```csharp
private static string? Opcion(string? s, string[] validas)
```

Busca una opción ignorando mayúsculas. Si el cliente envía `rebelde`, devuelve la forma oficial `Rebelde`. Si no coincide, devuelve `null`.

`ValidarPersonaje` devuelve un texto de error o `null`. Los ternarios encadenados significan:

1. comprobar textos obligatorios;
2. comprobar facción;
3. comprobar estado;
4. devolver `null` si todo es válido.

`Actualizar(Personaje p, PersonajeInput d)` copia el DTO limpio a la entidad. Compartir este método evita duplicar asignaciones en crear y editar.

### 12.2 Consultar personajes

```csharp
var q = db.Personajes.AsNoTracking().AsQueryable();
```

La consulta todavía no se ejecuta. Se van agregando filtros opcionales. `ToListAsync()` al final envía una sola consulta SQL con los filtros disponibles.

`Personaje(int id)` usa `FirstOrDefaultAsync`: devuelve el encontrado o `null`.

### 12.3 Crear personaje

Flujo de `CrearPersonaje`:

1. validar DTO;
2. crear `new Personaje()`;
3. copiar campos con `Actualizar`;
4. marcar la entidad para inserción con `Add`;
5. ejecutar `SaveChangesAsync`;
6. devolver 201 con el ID que MySQL asignó.

### 12.4 Editar personaje

`FindAsync(id)` busca por clave primaria y deja la entidad rastreada. Si se cambian sus propiedades, EF detecta los cambios al guardar.

La regla:

```csharp
if (p.Estado == "muerto" && Opcion(d.Estado, Estados) != "muerto")
```

impide revivir personajes. Devuelve 409 porque la solicitud entra en conflicto con el estado actual.

### 12.5 Eliminar personaje

Antes de `Remove`, consulta si existe una carta o participación relacionada. Si existe, devuelve 409. Esta validación produce un mensaje entendible y además la base tiene restricciones como segunda defensa.

### 12.6 Ranking y eventos del personaje

`Ranking` carga la navegación `Personaje`, ordena las cartas por poder y usa:

```csharp
datos.Select((x, i) => new RankingResponse(i + 1, ...))
```

El segundo parámetro `i` es el índice que empieza en cero; por eso la posición es `i + 1`.

`EventosPersonaje` empieza en la tabla intermedia, filtra por personaje y proyecta `x.Evento`.

### 12.7 Cartas

`ValidarCarta` es asíncrono porque consulta MySQL:

1. verifica que exista el personaje;
2. verifica que no tenga otra carta;
3. valida rangos;
4. valida textos;
5. usa `Uri.TryCreate` para exigir URL HTTP/HTTPS.

Al editar, `x.Id != id` excluye la propia carta de la comprobación de duplicado.

Antes de modificar o eliminar una carta, esta consulta busca uso histórico:

```csharp
db.Participaciones.AnyAsync(x =>
    x.PersonajeId == c.PersonajeId && x.Evento.Simulaciones.Any())
```

Si alguna participación del personaje pertenece a un evento simulado, la carta queda protegida para no alterar el contexto del historial.

### 12.8 Planetas

`Planetas(string? sector)` aplica el filtro solo cuando se envió. Ordena alfabéticamente.

`ValidarPlaneta` comprueba textos y rango 1–10. Crear y editar también comprueban nombre duplicado. En editar, `x.Id != id` evita que el planeta se considere duplicado de sí mismo.

`EliminarPlaneta` rechaza el borrado cuando hay eventos.

`EventosPlaneta` devuelve eventos ordenados por año galáctico.

`ActividadPlanetas` usa una proyección anónima:

```csharp
new { p.Id, p.Nombre, TotalEventos = p.Eventos.Count }
```

EF Core traduce el conteo a SQL. Luego ordena los planetas desde el más activo.

### 12.9 Consultar eventos

`Eventos()` y `Evento(id)` usan `Include(x => x.Participaciones)` para cargar la colección relacionada.

`EventoDetalle` es una proyección directa a DTO. Aunque no escribe `Include`, EF Core ve las propiedades utilizadas dentro de `Select` y genera las uniones necesarias. Devuelve:

- datos del evento;
- planeta;
- participantes ordenados;
- estado y resultado de cada uno;
- poder, o `null` si no tiene carta.

### 12.10 Validar evento

`ValidarEvento` comprueba:

1. nombre y descripción;
2. planeta existente;
3. estado `pendiente`;
4. al menos dos IDs sin repetidos;
5. existencia de todos los personajes;
6. ausencia de personajes muertos;
7. presencia de al menos dos facciones.

El parámetro opcional `eventoId` está reservado en la firma, pero la validación actual no necesita utilizarlo.

### 12.11 Crear evento

El inicializador construye el evento y, mediante `Select`, crea una participación pendiente por cada ID. EF Core inserta el evento y las participaciones relacionadas al guardar.

Después vuelve a consultarlo con `Evento(ev.Id)` para devolver también la colección.

### 12.12 Editar evento

Primero carga participaciones y simulaciones. Si existe al menos una simulación, devuelve 409 para proteger la historia.

Para sincronizar participantes:

- calcula los antiguos que ya no vienen en el DTO y los elimina;
- busca IDs nuevos que todavía no están en la colección y los agrega;
- conserva los que aparecen en ambas listas.

### 12.13 MVP

1. busca el evento;
2. obtiene los IDs participantes;
3. elige la carta de mayor poder;
4. consulta su personaje;
5. devuelve un objeto con ambos.

Si ningún participante tiene carta, devuelve 400.

## 13. `Services/SimulacionService.cs`

Este servicio contiene la lógica central del proyecto.

### 13.1 Convertir JSON histórico

```csharp
private static List<int> LeerIds(string json) =>
    JsonSerializer.Deserialize<List<int>>(json) ?? new();
```

Convierte el texto JSON guardado en MySQL nuevamente en una lista. Si la deserialización produce `null`, devuelve una lista vacía.

`Convertir` transforma una entidad `Simulacion` en `ResultadoSimulacion`. Así `Resultado` e `Historial` no duplican la conversión.

### 13.2 Último resultado

`Resultado(eventoId)`:

1. busca evento e incluye simulaciones;
2. devuelve 404 si no existe;
3. ordena por fecha descendente y luego por ID descendente;
4. toma la más reciente;
5. devuelve 404 si nunca fue simulado;
6. convierte la entidad a DTO.

El ID sirve como desempate si dos ejecuciones tienen la misma marca de tiempo.

### 13.3 Historial

Usa el mismo orden descendente, pero convierte todas las simulaciones. Un historial vacío es una respuesta correcta para un evento existente.

### 13.4 Carga necesaria para simular

```csharp
db.Eventos
    .Include(x => x.Planeta)
    .Include(x => x.Participaciones)
    .ThenInclude(x => x.Personaje)
    .ThenInclude(x => x.Carta)
    .Include(x => x.Simulaciones)
```

Carga el planeta, las participaciones, cada personaje, su carta y el historial. `ThenInclude` continúa desde la relación anterior.

### 13.5 Participantes disponibles

Solo entran quienes:

- no tienen resultado `fallecido`;
- conservan estado `vivo`.

Se ordenan por ID. Este orden permite elegir de forma reproducible la facción defensora de una batalla normal.

La simulación exige al menos dos personajes, carta para todos y exactamente dos facciones disponibles.

### 13.6 Defensor

```csharp
faccionDefensora ??= disponibles[0].Personaje.Faccion;
```

`??=` asigna solo si el valor era `null`. En batalla normal toma la facción del personaje disponible con menor ID. En invasión recibe la facción indicada por el cliente.

Luego comprueba que esa facción realmente participe.

### 13.7 Totales de poder

Para cada grupo:

```text
poderBase = suma de Carta.Poder
factorDefensor = 1 + NivelPeligro × 0.05
poderFinal = poderBase × factor
```

El atacante usa factor 1. El resultado se redondea a dos decimales.

Se ordena por:

1. poder final descendente;
2. poder base descendente;
3. nombre de facción ascendente.

La primera facción es la ganadora. Los criterios secundarios hacen determinista un empate.

### 13.8 Selección de bajas

Todos los miembros de la facción no ganadora forman `perdedores`. Se ordenan por menor poder y luego por menor ID.

```text
proporcionPoder = poderGanador / poderTotal
tasaBajas = min(1, 0.25 + 0.50 × proporcionPoder + 0.025 × peligro)
cantidad = max(1, techo(perdedores × tasaBajas))
```

Si el poder total es cero, la proporción es 0.5 para evitar división entre cero.

`Take(cantidadDeBajas)` escoge a los primeros perdedores, que son los de menor poder. `Contains` permite separar las listas de fallecidos y sobrevivientes.

### 13.9 Cambiar el estado en memoria

Para cada participación disponible:

- guarda `fallecido` o `sobreviviente`;
- si fallece, cambia `Personaje.Estado` a `muerto`.

Después crea el texto de criterio, marca el evento `simulado` y agrega una nueva entidad `Simulacion` con su snapshot JSON.

### 13.10 Transacción

```csharp
var transaccionPropia = db.Database.CurrentTransaction == null;
```

Si nadie abrió una transacción, `Simular` crea una. Si `Invadir` ya abrió una, reutiliza la existente.

Dentro de `try` guarda y confirma. Dentro de `catch` revierte la transacción propia y vuelve a lanzar el error. Esto evita guardar solo una parte de la batalla.

### 13.11 Invadir

`Invadir(planetaId, input)` valida:

- planeta existente;
- al menos dos participantes;
- atacante diferente de defensor;
- todos los IDs existentes y sin repeticiones lógicas;
- cada personaje perteneciente a una de las dos facciones;
- presencia de ambas facciones.

Luego abre una transacción exterior:

1. crea el evento mediante `CatalogoService`;
2. simula el evento como `invasion`;
3. confirma si ambas acciones funcionan;
4. revierte todo si falla cualquiera.

Por eso no queda un evento huérfano cuando la simulación no puede completarse.

### 13.12 Estadísticas

Busca todos los eventos del planeta con simulaciones y participaciones. Devuelve un objeto anónimo con:

- nombre del planeta;
- cantidad de eventos;
- cantidad de simulaciones;
- suma histórica de fallecidos;
- victorias agrupadas por facción.

`SelectMany` aplana las listas de simulaciones de varios eventos en una sola lista.

## 14. `Services/CifradoService.cs`

Esta clase protege el correo de los usuarios. Recibe `IConfiguration` para leer la clave desde Secret Manager.

### 14.1 Cargar la clave

```csharp
var secreto = configuracion["Cifrado:Clave"] ?? throw ...;
clave = Convert.FromBase64String(secreto);
```

La clave está guardada como texto Base64, pero al usarla se convierte a bytes. AES-256 requiere exactamente 32 bytes; por eso el constructor verifica la longitud.

### 14.2 Cifrar

Se crean cuatro arreglos:

| Arreglo | Tamaño/función |
|---|---|
| `nonce` | 12 bytes aleatorios; hace diferente cada cifrado |
| `datos` | correo convertido a UTF-8 |
| `cifrado` | salida del mismo largo que los datos |
| `tag` | 16 bytes para comprobar autenticidad |

```csharp
using (var aes = new AesGcm(clave, 16))
    aes.Encrypt(nonce, datos, cifrado, tag);
```

`using` libera el objeto criptográfico. El texto almacenado es:

```text
Base64(nonce + tag + textoCifrado)
```

El correo puede cifrarse dos veces y producir textos distintos porque cada `nonce` es aleatorio.

### 14.3 Descifrar

Primero convierte Base64 a bytes. Debe haber al menos 28 bytes: 12 de nonce y 16 de tag.

Los rangos significan:

| Expresión | Parte |
|---|---|
| `datos[..12]` | posiciones 0 a 11: nonce |
| `datos[12..28]` | posiciones 12 a 27: tag |
| `datos[28..]` | desde 28: contenido cifrado |

AES-GCM verifica el tag mientras descifra. Si la clave o los bytes no coinciden, lanza una excepción criptográfica en vez de devolver datos alterados.

### 14.4 Huella del correo

```csharp
HMACSHA256.HashData(clave, Encoding.UTF8.GetBytes(correoNormalizado))
```

HMAC produce siempre la misma huella para el mismo correo y clave. Permite consultar duplicados sin guardar el correo en claro. No sustituye el cifrado: la huella sirve para buscar y el cifrado sirve para recuperar el correo.

## 15. `Services/UsuarioService.cs`

El constructor recibe tres dependencias:

- `AppDbContext`: base de datos;
- `IPasswordHasher<Usuario>`: hash seguro de contraseña;
- `ICifradoService`: protección del correo.

### Crear usuario

1. valida nombre, correo y longitud de contraseña;
2. normaliza el correo con `Trim().ToLowerInvariant()`;
3. calcula la huella HMAC;
4. consulta si esa huella ya existe;
5. cifra el correo;
6. crea el hash de la contraseña;
7. guarda la entidad;
8. devuelve `UsuarioResponse`, nunca la entidad sensible.

```csharp
user.PasswordHash = hasher.HashPassword(user, input.Contrasena);
```

El hash de contraseña no se descifra. Para un futuro inicio de sesión se usaría `VerifyHashedPassword`.

### Obtener usuario

Consulta sin tracking. Si existe, descifra solamente el correo y construye `UsuarioResponse`. El hash de contraseña y la huella nunca salen en el JSON.

## 16. Controladores

Los controladores son la puerta HTTP. Reciben datos de Swagger, llaman servicios y devuelven respuestas. No hablan directamente con `AppDbContext`.

### 16.1 `Controllers/ApiControllerBase.cs`

```csharp
[ApiController]
public abstract class ApiControllerBase : ControllerBase
```

- `[ApiController]` habilita validación automática y enlace de JSON.
- `abstract` significa que no se instancia directamente.
- `ControllerBase` aporta `Ok`, `NotFound`, `BadRequest`, `StatusCode`, etc.

`Enviar<T>` traduce `Resultado<T>`:

- si `Error` no es nulo, responde el código con `{ "error": "..." }`;
- si no hay error, responde el código con `Valor`.

### 16.2 Rutas y parámetros

```csharp
[Route("api/personajes")]
[HttpGet("{id:int}")]
```

Juntos forman `GET /api/personajes/{id}`. `:int` impide que una ruta no numérica coincida.

Los parámetros pueden venir de:

| Origen | Ejemplo |
|---|---|
| ruta | `int id` en `{id:int}` |
| query string | `[FromQuery] string? sector` |
| cuerpo JSON | `PlanetaInput dto` |

### 16.3 `PersonajesController.cs`

| Método C# | HTTP | Servicio llamado |
|---|---|---|
| `Lista` | GET `/api/personajes` | `Personajes(faccion, fuerzaSensitivo)` |
| `Ranking` | GET `/api/personajes/ranking` | `Ranking()` |
| `Uno` | GET `/api/personajes/{id}` | `Personaje(id)` |
| `Crear` | POST | `CrearPersonaje(dto)` |
| `Editar` | PUT `/{id}` | `EditarPersonaje(id, dto)` |
| `Eliminar` | DELETE `/{id}` | `EliminarPersonaje(id)` |
| `Eventos` | GET `/{id}/eventos` | comprueba personaje y lista eventos |

`Ranking` valida en el controlador que `por` sea `poder`, porque esa query selecciona el tipo de ranking expuesto por esa ruta.

En eliminar, el servicio devuelve `Resultado<bool>`, pero una eliminación correcta se transforma en `NoContent()` para producir 204 sin cuerpo.

### 16.4 `CartasController.cs`

Expone CRUD de cartas. `Uno` devuelve 404 si el servicio de consulta devuelve `null`. Crear y editar usan `Enviar`; eliminar usa 204 cuando tiene éxito.

### 16.5 `PlanetasController.cs`

Además del CRUD, expone:

- `GET /actividad`: ranking por cantidad de eventos;
- `GET /{id}/eventos`: eventos del planeta;
- `POST /{id}/simular-invasion`: creación y simulación atómica;
- `GET /{id}/estadisticas`: resumen histórico.

Observa que `Actividad` está declarado antes que `/{id:int}` y además la restricción `:int` evita confundir la palabra `actividad` con un ID.

### 16.6 `EventosController.cs`

Recibe dos servicios: catálogo para CRUD/consultas y simulador para resultados.

| Ruta | Acción |
|---|---|
| GET `/api/eventos` | lista |
| GET `/api/eventos/{id}` | detalle proyectado |
| POST `/api/eventos` | crea |
| PUT `/api/eventos/{id}` | edita antes de simular |
| GET `/{id}/mvp` | carta más poderosa |
| POST `/{id}/simular` | ejecuta batalla |
| GET `/{id}/resultado` | último resultado |
| GET `/{id}/historial` | todas las ejecuciones |

### 16.7 `UsuariosController.cs`

Solo expone crear y obtener. Al depender de `IUsuarioService`, nunca ve la clave AES ni decide cómo se protege el correo.

## 17. Cómo ASP.NET convierte una solicitud

Ejemplo:

```http
POST /api/planetas
Content-Type: application/json

{
  "nombre": "Hoth",
  "sector": "Anoat",
  "clima": "Helado",
  "nivelPeligro": 8
}
```

El recorrido es:

1. `MapControllers` encuentra `PlanetasController.Crear`.
2. El serializador convierte el JSON en `PlanetaInput`.
3. `[ApiController]` ejecuta `[Required]` y `[Range]`.
4. El controlador llama `service.CrearPlaneta(dto)`.
5. `CatalogoService` ejecuta validaciones del negocio.
6. `db.Planetas.Add(p)` marca una inserción.
7. `SaveChangesAsync()` genera un `INSERT` para MySQL.
8. MySQL asigna el ID.
9. El servicio crea `Resultado<Planeta>` con código 201.
10. `Enviar` produce la respuesta JSON.

## 18. Cómo leer una consulta LINQ como SQL

Ejemplo simplificado:

```csharp
db.Planetas
  .AsNoTracking()
  .Where(x => x.Sector == sector)
  .OrderBy(x => x.Nombre)
  .ToListAsync();
```

Mentalmente se parece a:

```sql
SELECT *
FROM Planetas
WHERE Sector = @sector
ORDER BY Nombre;
```

No es SQL escrito a mano: EF Core examina la expresión y la traduce. La consulta normalmente se ejecuta al llegar a `ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`, etc.

## 19. Migraciones

Las migraciones son el historial del esquema. No contienen la lógica de las batallas; contienen instrucciones para cambiar tablas.

### `202611070001_Inicial.cs`

Crea las tablas iniciales: personajes, cartas, planetas, eventos, participaciones y usuarios, además de sus claves e índices.

### `202611070002_AgregarSimulaciones.cs`

Agrega la tabla para guardar resultados de simulación. La primera versión trataba el resultado como uno por evento.

### `202611080001_ConvertirSimulacionEnHistorial.cs`

Modifica el índice para permitir varias simulaciones por evento.

### `20261109154053_SincronizarModelo.cs`

Sincroniza metadata de columnas autoincrementales con el snapshot de EF Core.

### `20261109232835_CompletarPropuesta.cs`

Agrega estado al evento y datos históricos de bajas y sobrevivientes a simulación.

### `20261109233541_ResultadoDeParticipacion.cs`

Agrega `Resultado`, migra el booleano anterior a valores descriptivos y elimina la columna antigua. Su `Down` realiza el camino inverso.

### Archivos `.Designer.cs`

Son metadata generada por EF para cada migración. No se estudian como reglas de negocio ni se editan normalmente.

### `AppDbContextModelSnapshot.cs`

Es la fotografía del modelo que EF considera actual. Cuando se pide una migración nueva, EF compara las clases/configuración contra este snapshot.

### `Up` y `Down`

- `Up`: aplica el cambio al avanzar.
- `Down`: intenta revertirlo al retroceder.

`dotnet ef database update` revisa la tabla interna `__EFMigrationsHistory` y aplica solamente las migraciones que faltan.

## 20. Otros archivos del repositorio

| Archivo | Para qué sirve |
|---|---|
| `.editorconfig` | establece estilo, sangría y formato del código |
| `.gitignore` | evita subir `bin`, `obj`, secretos y configuración local |
| `README.md` | instalación y uso rápido |
| `DOCUMENTACION_TECNICA.md` | referencia técnica general |
| `PRUEBAS.http` | peticiones que puedes ejecutar desde VS Code |
| `KANBAN.md` | seguimiento de tareas |
| `USO_DE_IA.md` | declaración académica de asistencia de IA |

`bin/` contiene archivos compilados y `obj/` contiene archivos temporales de compilación. No debes estudiarlos ni subirlos.

## 21. Recorrido completo de una simulación

Cuando Swagger llama `POST /api/eventos/7/simular`:

1. `EventosController.Simular(7)` recibe el ID de la ruta.
2. Llama `ISimulacionService.Simular(7)`.
3. `SimulacionService` carga evento, planeta, participantes, personajes y cartas.
4. Descarta fallecidos.
5. Verifica cartas y exactamente dos facciones.
6. Elige defensor.
7. Agrupa personajes por facción.
8. Suma poder base.
9. Aplica bonificación de peligro al defensor.
10. Ordena y elige ganador.
11. Calcula la tasa de bajas.
12. Selecciona fallecidos de menor poder.
13. actualiza participaciones y personajes;
14. marca el evento simulado;
15. agrega una fila histórica de simulación;
16. guarda todo dentro de una transacción;
17. devuelve `ResultadoSimulacion`;
18. `Enviar` lo convierte en HTTP 200 y JSON.

## 22. Diferencias que debes poder explicar

### Entidad vs DTO

- Entidad: representa una tabla y EF Core la rastrea.
- DTO: representa el contrato que entra o sale por HTTP.

### Hash vs cifrado

- Hash de contraseña: no reversible; se usa para verificar.
- Cifrado de correo: reversible con la clave; se usa para recuperar.
- HMAC de correo: determinista; se usa para buscar duplicados.

### `Include` vs `Select`

- `Include` carga entidades relacionadas completas.
- `Select` puede pedir solo columnas necesarias y construir un DTO.

### 400 vs 404 vs 409

- 400: datos o regla inválida.
- 404: recurso inexistente.
- 409: existe un conflicto con datos o estado actuales.

### `Add` vs `SaveChangesAsync`

- `Add` solo marca una entidad como nueva en el contexto.
- `SaveChangesAsync` envía realmente el `INSERT` a MySQL.

### Estado rastreado vs `AsNoTracking`

- Rastreado: EF observa cambios; se usa para editar.
- Sin tracking: lectura más liviana; se usa para consultas.

### Regla de servicio vs restricción de base

La regla de servicio devuelve un error entendible. La restricción de base protege la integridad incluso ante concurrencia o un error de programación. En reglas importantes conviene tener ambas.

## 23. Preguntas para comprobar que entendiste

1. ¿Por qué `Personaje.Carta` lleva `?`?
2. ¿Por qué una participación usa dos columnas como clave?
3. ¿Qué diferencia hay entre `Evento.Estado` y `Participacion.Resultado`?
4. ¿En qué momento una consulta LINQ llega realmente a MySQL?
5. ¿Por qué editar usa entidades rastreadas y listar usa `AsNoTracking`?
6. ¿Qué ocurre si se intenta borrar un planeta con eventos?
7. ¿Por qué las cartas usadas en una simulación quedan protegidas?
8. ¿Cómo afecta el peligro del planeta al resultado?
9. ¿Por qué `Invadir` abre una transacción antes de crear el evento?
10. ¿Por qué no se busca un correo comparando `CorreoCifrado`?
11. ¿Por qué el `UsuarioResponse` no devuelve la entidad completa?
12. ¿Qué traduce `ApiControllerBase.Enviar`?

Si puedes responder estas preguntas siguiendo el código, ya comprendes la estructura principal del proyecto.

## 24. Ejercicio de lectura en VS Code

Para estudiar una ruta, usa este procedimiento:

1. abre un método del controlador;
2. usa `Ctrl+clic` sobre el método del servicio;
3. revisa primero la firma en la interfaz;
4. entra en la implementación;
5. identifica validaciones y consultas LINQ;
6. usa `Ctrl+clic` sobre el DTO o modelo;
7. revisa la relación correspondiente en `AppDbContext`;
8. ejecuta la solicitud equivalente en Swagger o `PRUEBAS.http`;
9. coloca un breakpoint y avanza con `F10` para observar los valores.

Empieza con `POST /api/planetas`, continúa con `POST /api/personajes` y deja `POST /api/eventos/{id}/simular` para el final. Así aumentas la dificultad gradualmente.
