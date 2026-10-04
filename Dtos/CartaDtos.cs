namespace ProyectoCatalogoGalactico.Dtos;

public record CardPersonajeInput(
    int PersonajeId,
    int Poder,
    string HabilidadEspecial,
    string Arma,
    int NivelPeligrosidad,
    string ImagenUrl);
