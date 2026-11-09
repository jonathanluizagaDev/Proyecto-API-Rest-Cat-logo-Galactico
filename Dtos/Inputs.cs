using System.ComponentModel.DataAnnotations;
namespace CatalogoGalacticoWebApi.Dtos;
public record PersonajeInput([Required] string Nombre,
    [Required] string Especie,
    [Required] string Faccion,
    [Required] string Afiliacion,
    [Required] string Estado,
    bool FuerzaSensitivo);
public record CartaInput(int PersonajeId,
    [Range(0, 100)] int Poder,
    [Required] string HabilidadEspecial,
    [Required] string Arma,
    [Range(1, 10)] int NivelPeligrosidad,
    [Required] string ImagenUrl);
public record PlanetaInput([Required] string Nombre, [Required] string Sector, [Required] string Clima, [Range(1, 10)] int NivelPeligro);
public record EventoInput(
    [Required] string Nombre, int Fecha, [Required] string Descripcion, int PlanetaId, [Required] List<int> ParticipanteIds);
public record InvasionInput([Required] string Nombre,
    int Fecha,
    [Required] string Descripcion,
    [Required] string FaccionAtacante,
    [Required] string FaccionDefensora,
    [Required] List<int> ParticipanteIds);
public record UsuarioInput([Required] string Nombre, [Required, EmailAddress] string Correo, [Required, MinLength(8)] string Contrasena);
public record UsuarioResponse(int Id, string Nombre, string Correo);
public record RankingResponse(int Posicion, int PersonajeId, string Nombre, int Poder);
public record TotalFaccionResponse(string Faccion, int PoderBase, double Factor, double PoderFinal);
public record ResultadoSimulacion(int EventoId,
    string Tipo,
    string Ganador,
    string Criterio,
    List<TotalFaccionResponse> Totales,
    List<int> FallecidoIds,
    List<int> SobrevivienteIds);
