namespace ProyectoCatalogoGalactico.Dtos;

public record PersonajeInput(
    string Nombre,
    string Especie,
    string Faccion,
    string Afiliacion,
    string Estado,
    bool FuerzaSensitivo);

public record PersonajeRankingResponse(
    int Posicion,
    int PersonajeId,
    string Nombre,
    int Poder);
