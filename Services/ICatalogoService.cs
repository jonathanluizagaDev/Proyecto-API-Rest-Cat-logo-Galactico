using CatalogoGalacticoWebApi.Dtos;
using CatalogoGalacticoWebApi.Models;
namespace CatalogoGalacticoWebApi.Services;
public interface ICatalogoService
{
    Task<List<Personaje>> Personajes(string? faccion, bool? fuerzaSensitivo);
    Task<Personaje?> Personaje(int id);
    Task<Resultado<Personaje>> CrearPersonaje(PersonajeInput input);
    Task<Resultado<Personaje>> EditarPersonaje(int id, PersonajeInput input);
    Task<Resultado<bool>> EliminarPersonaje(int id);
    Task<List<RankingResponse>> Ranking();
    Task<List<Evento>> EventosPersonaje(int id);
    Task<List<Carta>> Cartas();
    Task<Carta?> Carta(int id);
    Task<Resultado<Carta>> CrearCarta(CartaInput input);
    Task<Resultado<Carta>> EditarCarta(int id, CartaInput input);
    Task<Resultado<bool>> EliminarCarta(int id);
    Task<List<Planeta>> Planetas(string? sector);
    Task<Planeta?> Planeta(int id);
    Task<Resultado<Planeta>> CrearPlaneta(PlanetaInput input);
    Task<Resultado<Planeta>> EditarPlaneta(int id, PlanetaInput input);
    Task<Resultado<bool>> EliminarPlaneta(int id);
    Task<List<Evento>> EventosPlaneta(int id);
    Task<List<object>> ActividadPlanetas();
    Task<List<Evento>> Eventos();
    Task<Evento?> Evento(int id);
    Task<Resultado<Evento>> CrearEvento(EventoInput input);
    Task<Resultado<Evento>> EditarEvento(int id, EventoInput input);
    Task<Resultado<object>> Mvp(int id);
}
