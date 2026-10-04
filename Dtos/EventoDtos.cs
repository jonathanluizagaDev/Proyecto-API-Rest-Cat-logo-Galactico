using ProyectoCatalogoGalactico.Models;

namespace ProyectoCatalogoGalactico.Dtos;

public record EventoInput(
    string Nombre,
    int Fecha,
    string Ubicacion,
    string Descripcion,
    List<int> ParticipanteIds,
    List<int> FallecidoIds);

public record MvpResponse(
    Personaje Personaje,
    CardPersonaje Carta);

public record TotalFaccionResponse(
    string Faccion,
    int PoderBase,
    double Factor,
    double PoderFinal);

public record SimulacionResponse(
    int EventoId,
    string Ganador,
    string Criterio,
    List<TotalFaccionResponse> Totales);
