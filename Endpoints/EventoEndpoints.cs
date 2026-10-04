using ProyectoCatalogoGalactico.Dtos;
using ProyectoCatalogoGalactico.Models;
using ProyectoCatalogoGalactico.Services;

namespace ProyectoCatalogoGalactico.Endpoints;

public static class EventoEndpoints
{
    public static RouteGroupBuilder MapEventoEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/eventos").WithTags("Eventos");

        group.MapGet("/", (CatalogoService service) => Results.Ok(service.GetEventos()))
            .WithName("ObtenerEventos")
            .WithSummary("Lista todos los eventos galácticos.")
            .Produces<IEnumerable<Evento>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", (int id, CatalogoService service) =>
        {
            var evento = service.GetEvento(id);
            return evento is null ? Results.NotFound() : Results.Ok(evento);
        })
            .WithName("ObtenerEventoPorId")
            .WithSummary("Obtiene un evento específico por su identificador.")
            .Produces<Evento>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", (EventoInput input, CatalogoService service) =>
        {
            var result = service.CreateEvento(input);
            return result.IsSuccess
                ? Results.Created($"/api/eventos/{result.Value!.Id}", result.Value)
                : EndpointResults.BadRequest(result);
        })
            .WithName("CrearEvento")
            .WithSummary("Crea un evento y actualiza el estado de los personajes fallecidos.")
            .Produces<Evento>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", (int id, EventoInput input, CatalogoService service) =>
        {
            if (service.GetEvento(id) is null)
            {
                return Results.NotFound();
            }

            var result = service.UpdateEvento(id, input);
            return result.IsSuccess ? Results.Ok(result.Value) : EndpointResults.BadRequest(result);
        })
            .WithName("ActualizarEvento")
            .WithSummary("Actualiza un evento y vuelve a validar sus relaciones temporales.")
            .Produces<Evento>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/mvp", (int id, CatalogoService service) =>
        {
            if (service.GetEvento(id) is null)
            {
                return Results.NotFound();
            }

            var result = service.GetMvp(id);
            return result.IsSuccess ? Results.Ok(result.Value) : EndpointResults.BadRequest(result);
        })
            .WithName("ObtenerMvpDeEvento")
            .WithSummary("Obtiene el participante con la carta de mayor poder.")
            .Produces<MvpResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{id:int}/simular", (int id, CatalogoService service) =>
        {
            if (service.GetEvento(id) is null)
            {
                return Results.NotFound();
            }

            var result = service.SimularBatalla(id);
            return result.IsSuccess ? Results.Ok(result.Value) : EndpointResults.BadRequest(result);
        })
            .WithName("SimularBatalla")
            .WithSummary("Simula una batalla usando el poder total de cada facción.")
            .Produces<SimulacionResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
