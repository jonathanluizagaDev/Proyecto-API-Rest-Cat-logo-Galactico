# Catálogo Galáctico — Trabajo final de Tecnologías Web I

Adaptación del proyecto Minimal API original a **ASP.NET Core Web API .NET 10 con controladores**, MySQL y EF Core, siguiendo la arquitectura del ejemplo BackofficeApi del docente. El código original se conserva como referencia conceptual; esta implementación no es idéntica a él.

## Requisitos

- SDK .NET 10, VS Code con C# Dev Kit (opcional)
- Servidor MySQL en ejecución y una base `catalogo_galactico` creada
- `dotnet-ef` instalado (`dotnet tool install --global dotnet-ef --version 9.0.0`, o `dotnet tool update --global dotnet-ef --version 9.0.0`)

## Configuración segura (NO subir secretos al repositorio)

```bash
# Ejecutar en la carpeta donde está el .csproj
dotnet restore
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;port=3306;database=catalogo_galactico;user=USUARIO_FICTICIO;password=CLAVE_FICTICIA"
```

Sustituye los datos ficticios por tu usuario y contraseña reales, **solo en tu equipo**.

Para generar la clave AES de 256 bits puedes ejecutar el comando siguiente en una terminal con Python instalado y COPIAR el valor generado:

```bash
python -c "import secrets,base64;print(base64.b64encode(secrets.token_bytes(32)).decode())"
```

Después configura el valor que recibiste:

```bash
dotnet user-secrets set "Cifrado:Clave" "PEGA_AQUI_BASE64_GENERADO"
```

## Ejecutar

```bash
dotnet ef migrations list
dotnet ef database update
dotnet run
```

Si recibes un problema de conexión, verifica que MySQL esté iniciado y la base exista. No borres migraciones aplicadas ni compartas claves en capturas.

Swagger: `http://localhost:5080/swagger` al ejecutar con `dotnet run --launch-profile http`.

## Endpoints

- `GET, POST /api/personajes`; `GET, PUT, DELETE /api/personajes/{id}`; `GET /api/personajes/ranking`; `GET /api/personajes/{id}/eventos`
- `GET, POST /api/cartas`; `GET, PUT, DELETE /api/cartas/{id}`
- `GET, POST /api/planetas`; `GET, PUT, DELETE /api/planetas/{id}`; `GET /api/planetas/actividad`; `GET /api/planetas/{id}/eventos`
- `POST /api/planetas/{id}/simular-invasion`; `GET /api/planetas/{id}/estadisticas`
- `GET, POST /api/eventos`; `GET, PUT /api/eventos/{id}`; `GET /api/eventos/{id}/mvp`; `POST /api/eventos/{id}/simular`; `GET /api/eventos/{id}/resultado`; `GET /api/eventos/{id}/historial`
- `POST /api/usuarios`; `GET /api/usuarios/{id}` (sin login ni JWT)

## Relaciones y simulación

Planeta 1:N Evento; Personaje 1:0..1 Carta; Personaje N:M Evento mediante Participacion; Evento 1:N Simulacion.

Solo se permite simular eventos con exactamente dos facciones, participantes vivos y cartas. Los ataques defensivos de una invasión obtienen bonificación de **5% por nivel de peligro** del planeta. Las bajas son deterministas: la facción derrotada pierde la mitad de sus miembros (redondeada hacia arriba), seleccionados de menor poder; los empates no producen bajas. Cada ejecución se agrega al historial y no sobrescribe simulaciones anteriores. Los cambios quedan en una única operación `SaveChangesAsync`.

## Migraciones

El proyecto incluye tres migraciones (`Inicial`, `AgregarSimulaciones` y `ConvertirSimulacionEnHistorial`) y snapshot. En caso de modificar entidades, usa `dotnet ef migrations add NombreDeCambio` para generar una migración adicional. Verifica en MySQL el estado de `__EFMigrationsHistory`.

## Demostración de protección

1. Crear un usuario mediante `POST /api/usuarios`.
2. Consultar en MySQL: `SELECT Id, CorreoBusquedaHash, CorreoCifrado, PasswordHash FROM Usuarios;`. Ninguno de esos campos debe mostrar correo o contraseña en texto plano.
3. Consultar `GET /api/usuarios/{id}` y comprobar que entrega el correo legible y **nunca** la contraseña ni el hash.
4. `CorreoBusquedaHash` es una huella HMAC para prevenir duplicados sin guardar correo claro; `CorreoCifrado` usa AES-GCM.

## Git y entrega

Realiza commits pequeños verificables, agrega `USO_DE_IA.md` con tus cambios reales y al entregar usa `git tag entrega-final` y `git push origin entrega-final`. Comprueba que no haya secretos en commits pasados.

**Estado:** paquete preparado como base de implementación; debe verificarse compilación, migraciones, endpoints y MySQL en el equipo del estudiante antes de entregar. No sustituye la defensa individual.
