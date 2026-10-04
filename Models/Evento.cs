namespace ProyectoCatalogoGalactico.Models;

public record Evento(
    int Id,
    string Nombre,
    int Fecha,
    string Ubicacion,
    string Descripcion,
    List<int> ParticipanteIds,
    List<int> FallecidoIds,
    string? Resultado,
    string? Ganador);
