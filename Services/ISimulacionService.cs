using CatalogoGalacticoWebApi.Dtos;
namespace CatalogoGalacticoWebApi.Services;
public interface ISimulacionService
{
    Task<Resultado<ResultadoSimulacion>> Simular(int eventoId, string tipo = "batalla", string? faccionDefensora = null);
    Task<Resultado<ResultadoSimulacion>> Resultado(int eventoId);
    Task<Resultado<List<ResultadoSimulacion>>> Historial(int eventoId);
    Task<Resultado<ResultadoSimulacion>> Invadir(int planetaId, InvasionInput input);
    Task<Resultado<object>> Estadisticas(int planetaId);
}
