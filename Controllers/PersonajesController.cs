using CatalogoGalacticoWebApi.Dtos;
using CatalogoGalacticoWebApi.Services;
using Microsoft.AspNetCore.Mvc;
namespace CatalogoGalacticoWebApi.Controllers;
[Route("api/personajes")]
public class PersonajesController(ICatalogoService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Lista([FromQuery] string? faccion, [FromQuery] bool? fuerzaSensitivo) => Ok(
        await service.Personajes(faccion, fuerzaSensitivo));
    [HttpGet("ranking")]
    public async Task<IActionResult> Ranking() => Ok(await service.Ranking());
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Uno(int id)
    {
        var x = await service.Personaje(id);
        return x == null ? NotFound() : Ok(x);
    }
    [HttpPost]
    public async Task<IActionResult> Crear(PersonajeInput dto) => Enviar(await service.CrearPersonaje(dto));
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, PersonajeInput dto) => Enviar(await service.EditarPersonaje(id, dto));
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var r = await service.EliminarPersonaje(id);
        return r.Error != null ? Enviar(r) : NoContent();
    }
    [HttpGet("{id:int}/eventos")]
    public async Task<IActionResult> Eventos(int id) => await service.Personaje(id) == null ? NotFound()
                                                                                            : Ok(await service.EventosPersonaje(id));
}
