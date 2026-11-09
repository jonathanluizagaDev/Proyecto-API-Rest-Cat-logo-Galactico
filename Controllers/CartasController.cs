using CatalogoGalacticoWebApi.Dtos;
using CatalogoGalacticoWebApi.Services;
using Microsoft.AspNetCore.Mvc;
namespace CatalogoGalacticoWebApi.Controllers;
[Route("api/cartas")]
public class CartasController(ICatalogoService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Lista() => Ok(await service.Cartas());
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Uno(int id)
    {
        var x = await service.Carta(id);
        return x == null ? NotFound() : Ok(x);
    }
    [HttpPost]
    public async Task<IActionResult> Crear(CartaInput dto) => Enviar(await service.CrearCarta(dto));
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, CartaInput dto) => Enviar(await service.EditarCarta(id, dto));
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var r = await service.EliminarCarta(id);
        return r.Error != null ? Enviar(r) : NoContent();
    }
}
