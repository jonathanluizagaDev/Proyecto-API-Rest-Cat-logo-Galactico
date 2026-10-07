namespace ProyectoCatalogoGalactico.Dtos;
using ProyectoCatalogoGalactico.Models;

public record PersonajeInput(
    string Nombre,
    string Especie,
    string Faccion,
    string Afiliacion,
    string Estado,
    bool FuerzaSensitivo,
    string foto);

public record PersonajeRankingResponse(
    int Posicion,
    int PersonajeId,
    string Nombre,
    int Poder);


public record PersonajeConCardResponse(
    Personaje Personaje,
    CardPersonaje Card

);