using CatalogoGalacticoWebApi.Dtos;
using CatalogoGalacticoWebApi.Services;
using Microsoft.AspNetCore.Mvc;
namespace CatalogoGalacticoWebApi.Controllers;
[Route("api/planetas")]
public class PlanetasController(ICatalogoService service, ISimulacionService simulador) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Lista([FromQuery] string? sector) => Ok(await service.Planetas(sector));
    [HttpGet("actividad")]
    public async Task<IActionResult> Actividad() => Ok(await service.ActividadPlanetas());
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Uno(int id)
    {
        var x = await service.Planeta(id);
        return x == null ? NotFound() : Ok(x);
    }
    [HttpPost]
    public async Task<IActionResult> Crear(PlanetaInput dto) => Enviar(await service.CrearPlaneta(dto));
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Editar(int id, PlanetaInput dto) => Enviar(await service.EditarPlaneta(id, dto));
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var r = await service.EliminarPlaneta(id);
        return r.Error != null ? Enviar(r) : NoContent();
    }
    [HttpGet("{id:int}/eventos")]
    public async Task<IActionResult> Eventos(int id) => await service.Planeta(id) == null ? NotFound()
                                                                                          : Ok(await service.EventosPlaneta(id));
    [HttpPost("{id:int}/simular-invasion")]
    public async Task<IActionResult> Invadir(int id, InvasionInput dto) => Enviar(await simulador.Invadir(id, dto));
    [HttpGet("{id:int}/estadisticas")]
    public async Task<IActionResult> Estadisticas(int id) => Enviar(await simulador.Estadisticas(id));
}
