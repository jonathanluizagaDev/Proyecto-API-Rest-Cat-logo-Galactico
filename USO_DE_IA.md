# Declaración de uso de IA

Herramienta utilizada: ChatGPT/Codex como apoyo durante el desarrollo y la verificación del proyecto.

## Pedidos principales

- Organizar el trabajo mediante un Kanban.
- Comparar el proyecto Minimal API original con el ejemplo BackofficeApi del docente.
- Adaptar el proyecto a controladores, servicios, DTOs, Entity Framework Core y MySQL.
- Mantener el estilo de escritura y espaciado utilizado en el proyecto de referencia.
- Revisar errores de compilación, migraciones, configuración local y flujo de Git/GitHub.
- Preparar y ejecutar casos de prueba CRUD, simulaciones y protección de datos.

## Cambios revisados y aplicados

- Se reemplazó la estructura de endpoints mínimos por controladores y servicios.
- Se configuraron relaciones entre planetas, eventos, personajes, cartas, participaciones y simulaciones.
- Se cambió la relación Evento-Simulación a un historial 1:N.
- Se corrigió la migración de MySQL que debía retirar temporalmente una clave foránea antes de cambiar su índice.
- Se sincronizó el snapshot de Entity Framework con la metadata `AUTO_INCREMENT` de MySQL.
- Se incorporaron hash de contraseñas, HMAC para búsqueda de correos y cifrado AES-GCM.
- Se actualizaron README, solicitudes HTTP y Kanban con resultados comprobados.

## Verificaciones realizadas

El 9 de noviembre de 2026 se comprobó localmente:

- .NET SDK 10.0.403 y MySQL 8.4.9;
- compilación con 0 errores;
- cuatro migraciones aplicadas y modelo sin cambios pendientes;
- creación, consulta, actualización y eliminación mediante la API;
- validación de duplicados y restricciones de relaciones;
- batalla con ganador, bajas, resultado, historial y MVP;
- invasión con bono defensivo de 5% por nivel de peligro;
- almacenamiento de correo cifrado, huella HMAC y contraseña hasheada;
- respuesta pública de usuario sin contraseña ni hashes;
- Swagger y OpenAPI disponibles en `http://localhost:5080`.

## Responsabilidad individual

La IA se utilizó como asistente. El estudiante debe comprender el código, revisar los cambios antes de fusionarlos y poder explicar la arquitectura, las relaciones, la simulación, las migraciones y la protección de datos durante la defensa.
