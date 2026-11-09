using CatalogoGalacticoWebApi.Dtos;
using CatalogoGalacticoWebApi.Services;
using Microsoft.AspNetCore.Mvc;
namespace CatalogoGalacticoWebApi.Controllers;
[Route("api/eventos")]
public class EventosController(ICatalogoService service, ISimulacionService simulador) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Lista() => Ok(await service.Eventos());
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Uno(int id)
    {
        var e = await service.Evento(id);
        return e == null ? NotFound() : Ok(e);
    }
    [HttpPost]
    public async Task<IActionResult> Crear(EventoInput dto) => Enviar(await service.CrearEvento(dto));
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, EventoInput dto) => Enviar(await service.EditarEvento(id, dto));
    [HttpGet("{id:int}/mvp")]
    public async Task<IActionResult> Mvp(int id) => Enviar(await service.Mvp(id));
    [HttpPost("{id:int}/simular")]
    public async Task<IActionResult> Simular(int id) => Enviar(await simulador.Simular(id));
    [HttpGet("{id:int}/resultado")]
    public async Task<IActionResult> Resultado(int id) => Enviar(await simulador.Resultado(id));
    [HttpGet("{id:int}/historial")]
    public async Task<IActionResult> Historial(int id) => Enviar(await simulador.Historial(id));
}
