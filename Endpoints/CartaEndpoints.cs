using ProyectoCatalogoGalactico.Dtos;
using ProyectoCatalogoGalactico.Models;
using ProyectoCatalogoGalactico.Services;

namespace ProyectoCatalogoGalactico.Endpoints;

public static class CartaEndpoints
{
    public static RouteGroupBuilder MapCartaEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/cartas").WithTags("Cartas");

        group.MapGet("/", (CatalogoService service) => Results.Ok(service.GetCartas()))
            .WithName("ObtenerCartas")
            .WithSummary("Lista todas las cartas de personajes.")
            .Produces<IEnumerable<CardPersonaje>>(StatusCodes.Status200OK);

        group.MapGet("/{id:int}", (int id, CatalogoService service) =>
        {
            var carta = service.GetCarta(id);
            return carta is null ? Results.NotFound() : Results.Ok(carta);
        })
            .WithName("ObtenerCartaPorId")
            .WithSummary("Obtiene una carta específica por su identificador.")
            .Produces<CardPersonaje>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", (CardPersonajeInput input, CatalogoService service) =>
        {
            var result = service.CreateCarta(input);
            return result.IsSuccess
                ? Results.Created($"/api/cartas/{result.Value!.Id}", result.Value)
                : EndpointResults.BadRequest(result);
        })
            .WithName("CrearCarta")
            .WithSummary("Crea la carta principal de un personaje.")
            .Produces<CardPersonaje>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", (int id, CardPersonajeInput input, CatalogoService service) =>
        {
            if (service.GetCarta(id) is null)
            {
                return Results.NotFound();
            }

            var result = service.UpdateCarta(id, input);
            return result.IsSuccess ? Results.Ok(result.Value) : EndpointResults.BadRequest(result);
        })
            .WithName("ActualizarCarta")
            .WithSummary("Actualiza una carta existente.")
            .Produces<CardPersonaje>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
