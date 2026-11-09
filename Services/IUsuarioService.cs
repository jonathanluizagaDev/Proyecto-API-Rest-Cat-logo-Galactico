using CatalogoGalacticoWebApi.Dtos;
namespace CatalogoGalacticoWebApi.Services;
public interface IUsuarioService
{
    Task<Resultado<UsuarioResponse>> Crear(UsuarioInput input);
    Task<Resultado<UsuarioResponse>> Obtener(int id);
}
