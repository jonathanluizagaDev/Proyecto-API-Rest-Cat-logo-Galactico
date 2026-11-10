# Catálogo Galáctico — Trabajo final de Tecnologías Web I

Adaptación del proyecto Minimal API original a **ASP.NET Core Web API .NET 10 con controladores**, MySQL y EF Core, siguiendo la arquitectura del ejemplo BackofficeApi del docente. El código original se conserva como referencia conceptual; esta implementación no es idéntica a él.

Documentación detallada del código: [DOCUMENTACION_TECNICA.md](DOCUMENTACION_TECNICA.md).

Guía didáctica archivo por archivo: [GUIA_DE_ESTUDIO_DEL_CODIGO.md](GUIA_DE_ESTUDIO_DEL_CODIGO.md).

## Requisitos

- SDK .NET 10, VS Code con C# Dev Kit (opcional)
- Servidor MySQL en ejecución y una base `catalogo_galactico` creada
- `dotnet-ef` instalado (`dotnet tool install --global dotnet-ef --version 9.0.0`, o `dotnet tool update --global dotnet-ef --version 9.0.0`)

## Configuración segura (NO subir secretos al repositorio)

```bash
# Ejecutar en la carpeta donde está el .csproj
dotnet restore
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;port=3306;database=catalogo_galactico;user=USUARIO_FICTICIO;password=CLAVE_FICTICIA;SslMode=None"
```

Sustituye los datos ficticios por tu usuario y contraseña reales, **solo en tu equipo**.

Para generar y guardar una clave AES de 256 bits desde PowerShell:

```powershell
$bytes = New-Object byte[] 32
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
dotnet user-secrets set "Cifrado:Clave" ([Convert]::ToBase64String($bytes))
$rng.Dispose()
Remove-Variable bytes,rng
```

## Ejecutar

```bash
dotnet ef migrations list
dotnet ef database update
dotnet run --launch-profile http
```

Si recibes un problema de conexión, verifica que MySQL esté iniciado y la base exista. No borres migraciones aplicadas ni compartas claves en capturas.

Swagger: `http://localhost:5080/swagger` al ejecutar con `dotnet run --launch-profile http`.

## Endpoints

- `GET, POST /api/personajes`; `GET, PUT, DELETE /api/personajes/{id}`; `GET /api/personajes/ranking?por=poder`; `GET /api/personajes/{id}/eventos`
- `GET, POST /api/cartas`; `GET, PUT, DELETE /api/cartas/{id}`
- `GET, POST /api/planetas`; `GET, PUT, DELETE /api/planetas/{id}`; `GET /api/planetas/actividad`; `GET /api/planetas/{id}/eventos`
- `POST /api/planetas/{id}/simular-invasion`; `GET /api/planetas/{id}/estadisticas`
- `GET, POST /api/eventos`; `GET, PUT /api/eventos/{id}`; `GET /api/eventos/{id}/mvp`; `POST /api/eventos/{id}/simular`; `GET /api/eventos/{id}/resultado`; `GET /api/eventos/{id}/historial`
- `POST /api/usuarios`; `GET /api/usuarios/{id}` (sin login ni JWT)

## Relaciones y simulación

Planeta 1:N Evento; Personaje 1:0..1 Carta; Personaje N:M Evento mediante Participacion; Evento 1:N Simulacion.

Solo se permite simular eventos con exactamente dos facciones disponibles y participantes vivos con carta. En una batalla normal se considera defensora la facción del participante vivo con ID menor; en una invasión se usa la facción defensora indicada en el cuerpo. El defensor recibe **5% de poder por nivel de peligro** del planeta.

Gana el mayor poder final. Un empate se resuelve primero por poder base y, si continúa, por el nombre de facción en orden alfabético. La cantidad de bajas usa una fórmula determinista que combina poder relativo y peligro: `techo(perdedores × [25% + 50% del poder relativo ganador + 2.5% por nivel de peligro])`. Fallecen primero los perdedores de menor poder. Cada simulación guarda sus propios totales, bajas, sobrevivientes y fecha; una nueva ejecución no sobrescribe el historial anterior y excluye a los personajes ya fallecidos. Evento, participaciones y simulación se guardan dentro de una transacción.

## Migraciones

El proyecto incluye seis migraciones (`Inicial`, `AgregarSimulaciones`, `ConvertirSimulacionEnHistorial`, `SincronizarModelo`, `CompletarPropuesta` y `ResultadoDeParticipacion`) y un snapshot sincronizado. En caso de modificar entidades, usa `dotnet ef migrations add NombreDeCambio` para generar una migración adicional. Verifica en MySQL el estado de `__EFMigrationsHistory`.

## Demostración de protección

1. Crear un usuario mediante `POST /api/usuarios`.
2. Consultar en MySQL: `SELECT Id, CorreoBusquedaHash, CorreoCifrado, PasswordHash FROM Usuarios;`. Ninguno de esos campos debe mostrar correo o contraseña en texto plano.
3. Consultar `GET /api/usuarios/{id}` y comprobar que entrega el correo legible y **nunca** la contraseña ni el hash.
4. `CorreoBusquedaHash` es una huella HMAC para prevenir duplicados sin guardar correo claro; `CorreoCifrado` usa AES-GCM.

## Git y entrega

Realiza commits pequeños verificables, agrega `USO_DE_IA.md` con tus cambios reales y al entregar usa `git tag entrega-final` y `git push origin entrega-final`. Comprueba que no haya secretos en commits pasados.

## Estado verificado localmente

Comprobado el 9 de noviembre de 2026 con .NET SDK 10.0.403 y MySQL 8.4.9:

- compilación con 0 errores;
- seis migraciones aplicadas y modelo sin cambios pendientes;
- CRUD de planetas, personajes, cartas y eventos;
- respuestas 409 para planeta/carta duplicados y para intentar revivir un personaje fallecido;
- dos simulaciones consecutivas de un evento con bajas históricas independientes, estado del evento, detalle de planeta y participantes;
- simulación de batalla, bajas reproducibles, MVP e invasión con bono defensivo;
- alta y consulta segura de usuarios, detección de correo duplicado y ausencia de correo/contraseña en texto plano;
- OpenAPI, Swagger y endpoints principales con respuesta HTTP 200.

La advertencia `NU1900` puede aparecer cuando NuGet no logra consultar temporalmente su servicio de vulnerabilidades; no impide compilar ni ejecutar la API. Los datos creados durante las pruebas permanecen únicamente en la base MySQL local.
