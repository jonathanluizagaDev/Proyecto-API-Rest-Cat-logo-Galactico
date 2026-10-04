using ProyectoCatalogoGalactico.Dtos;
using ProyectoCatalogoGalactico.Models;
using ProyectoCatalogoGalactico.Services;

namespace ProyectoCatalogoGalactico.Endpoints;

public static class PersonajeEndpoints
{
    public static RouteGroupBuilder MapPersonajeEndpoints(this RouteGroupBuilder api)
    {
        var group = api.MapGroup("/personajes").WithTags("Personajes");

        group.MapGet("/", (CatalogoService service, string? faccion, bool? fuerzaSensitivo) =>
                Results.Ok(service.GetPersonajes(faccion, fuerzaSensitivo)))
            .WithName("ObtenerPersonajes")
            .WithSummary("Lista personajes y permite combinar filtros de facción y Fuerza.")
            .Produces<IEnumerable<Personaje>>(StatusCodes.Status200OK);

        group.MapGet("/ranking", (CatalogoService service, string? por) =>
        {
            if (por is not null && !por.Equals("poder", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest(new { error = "El ranking solo admite el criterio 'poder'." });
            }

            return Results.Ok(service.GetRankingPorPoder());
        })
            .WithName("ObtenerRankingPersonajes")
            .WithSummary("Ordena los personajes por el poder de su carta principal.")
            .Produces<IEnumerable<PersonajeRankingResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id:int}", (int id, CatalogoService service) =>
        {
            var personaje = service.GetPersonaje(id);
            return personaje is null ? Results.NotFound() : Results.Ok(personaje);
        })
            .WithName("ObtenerPersonajePorId")
            .WithSummary("Obtiene un personaje específico por su identificador.")
            .Produces<Personaje>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", (PersonajeInput input, CatalogoService service) =>
        {
            var result = service.CreatePersonaje(input);
            return result.IsSuccess
                ? Results.Created($"/api/personajes/{result.Value!.Id}", result.Value)
                : EndpointResults.BadRequest(result);
        })
            .WithName("CrearPersonaje")
            .WithSummary("Crea un personaje nuevo.")
            .Produces<Personaje>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:int}", (int id, PersonajeInput input, CatalogoService service) =>
        {
            if (service.GetPersonaje(id) is null)
            {
                return Results.NotFound();
            }

            var result = service.UpdatePersonaje(id, input);
            return result.IsSuccess ? Results.Ok(result.Value) : EndpointResults.BadRequest(result);
        })
            .WithName("ActualizarPersonaje")
            .WithSummary("Actualiza un personaje existente.")
            .Produces<Personaje>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:int}", (int id, CatalogoService service) =>
        {
            if (service.GetPersonaje(id) is null)
            {
                return Results.NotFound();
            }

            var result = service.DeletePersonaje(id);
            return result.IsSuccess ? Results.NoContent() : EndpointResults.BadRequest(result);
        })
            .WithName("EliminarPersonaje")
            .WithSummary("Elimina un personaje sin relaciones registradas.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{id:int}/eventos", (int id, CatalogoService service) =>
        {
            if (service.GetPersonaje(id) is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(service.GetEventosDePersonaje(id));
        })
            .WithName("ObtenerEventosDePersonaje")
            .WithSummary("Lista los eventos en los que participa un personaje.")
            .Produces<IEnumerable<Evento>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }
}
