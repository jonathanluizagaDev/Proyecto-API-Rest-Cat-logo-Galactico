# Catálogo Galáctico API

API REST académica desarrollada con ASP.NET Core Minimal API para consultar y administrar personajes,
cartas coleccionables y eventos de un universo galáctico.

La aplicación usa almacenamiento en memoria. Todos los cambios realizados mediante la API se pierden
cuando el proceso se reinicia y las colecciones vuelven a sus datos iniciales.

## Tecnologías

- .NET 10
- ASP.NET Core Minimal API
- OpenAPI
- Swagger UI
- C#

## Organización

```text
Data/       Listas en memoria y datos iniciales
Dtos/       Datos de entrada y respuestas calculadas
Endpoints/  Rutas agrupadas por recurso
Models/     Personaje, CardPersonaje y Evento
Services/   Validaciones, relaciones, estadísticas y simulación
Program.cs  Registro de servicios, OpenAPI y endpoints
```

## Requisitos

- .NET SDK 10.0 o superior
- Visual Studio Code, Visual Studio u otro editor compatible con C#

## Ejecución

Desde la carpeta del proyecto:

```bash
dotnet restore
dotnet run
```

Con el perfil HTTP incluido, la API queda disponible en:

```text
http://localhost:5048
```

Swagger está disponible en:

```text
http://localhost:5048/swagger
```

También se puede usar `ProyectoCatalogoGalactico.http` para enviar solicitudes desde Visual Studio Code.

## Convención de fechas

La propiedad `Fecha` de un evento es un número entero que permite ordenar cronológicamente:

| Fecha narrativa | Valor |
| --- | ---: |
| 10 BBY | -10 |
| 5 BBY | -5 |
| Batalla de Yavin | 0 |
| 3 ABY | 3 |
| 10 ABY | 10 |

## Endpoints

### Personajes

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/personajes` | Lista personajes y acepta filtros opcionales |
| GET | `/personajes/{id}` | Obtiene un personaje |
| POST | `/personajes` | Crea un personaje |
| PUT | `/personajes/{id}` | Actualiza un personaje |
| DELETE | `/personajes/{id}` | Elimina un personaje sin relaciones |
| GET | `/personajes/{id}/eventos` | Lista los eventos del personaje |
| GET | `/personajes/ranking?por=poder` | Ordena personajes por poder |

Filtros combinables:

```http
GET /personajes?faccion=Imperio&fuerzaSensitivo=true
```

### Cartas

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/cartas` | Lista todas las cartas |
| GET | `/cartas/{id}` | Obtiene una carta |
| POST | `/cartas` | Crea la carta principal de un personaje |
| PUT | `/cartas/{id}` | Actualiza una carta |

### Eventos

| Método | Ruta | Descripción |
| --- | --- | --- |
| GET | `/eventos` | Lista todos los eventos |
| GET | `/eventos/{id}` | Obtiene un evento |
| POST | `/eventos` | Crea un evento |
| PUT | `/eventos/{id}` | Actualiza un evento |
| GET | `/eventos/{id}/mvp` | Obtiene el participante con mayor poder |
| POST | `/eventos/{id}/simular` | Simula una batalla entre facciones |

## Ejemplos

Crear un personaje:

```json
{
  "nombre": "Ahsoka Tano",
  "especie": "Togruta",
  "faccion": "Neutral",
  "afiliacion": "Orden Jedi",
  "estado": "vivo",
  "fuerzaSensitivo": true
}
```

Crear una carta:

```json
{
  "personajeId": 5,
  "poder": 90,
  "habilidadEspecial": "Doble sable",
  "arma": "Sables de luz",
  "nivelPeligrosidad": 9,
  "imagenUrl": "https://upload.wikimedia.org/wikipedia/en/d/d7/Ahsoka_Tano.png"
}
```

Crear un evento:

```json
{
  "nombre": "Encuentro galáctico",
  "fecha": 2,
  "ubicacion": "Borde Exterior",
  "descripcion": "Encuentro entre dos facciones.",
  "participanteIds": [2, 5],
  "fallecidoIds": []
}
```

## Reglas de negocio

- La facción debe ser `Rebelde`, `Imperio` o `Neutral`.
- El estado debe ser `vivo`, `muerto` o `desconocido`.
- Cada personaje puede tener una sola carta principal.
- El poder debe estar entre 0 y 100.
- El nivel de peligrosidad debe estar entre 1 y 10.
- Todo participante y fallecido debe referenciar un personaje existente.
- Un fallecido debe formar parte de los participantes del evento.
- Un personaje no puede participar en eventos posteriores a su muerte registrada.
- Registrar una muerte cambia automáticamente el estado del personaje a `muerto`.
- No se puede eliminar un personaje que tenga una carta o participe en eventos.
- La simulación exige al menos dos participantes, sus cartas y dos facciones diferentes.

## Respuestas HTTP

- `200 OK`: consulta, actualización o simulación correcta.
- `201 Created`: recurso creado correctamente.
- `204 No Content`: eliminación correcta.
- `400 Bad Request`: datos inválidos o incumplimiento de reglas.
- `404 Not Found`: recurso inexistente.

## Imágenes

Los datos iniciales usan URLs de imágenes publicadas en Wikipedia o Wikimedia. Para usos distintos al
académico se debe revisar la licencia individual de cada imagen.
