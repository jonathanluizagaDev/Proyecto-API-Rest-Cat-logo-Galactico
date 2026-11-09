using CatalogoGalacticoWebApi.Dtos;
using CatalogoGalacticoWebApi.Services;
using Microsoft.AspNetCore.Mvc;
namespace CatalogoGalacticoWebApi.Controllers;
[Route("api/usuarios")]
public class UsuariosController(IUsuarioService service) : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Crear(UsuarioInput dto) => Enviar(await service.Crear(dto));
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Uno(int id) => Enviar(await service.Obtener(id));
}
