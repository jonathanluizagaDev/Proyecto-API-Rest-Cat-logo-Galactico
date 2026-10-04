using ProyectoCatalogoGalactico.Services;

namespace ProyectoCatalogoGalactico.Endpoints;

internal static class EndpointResults
{
    public static IResult BadRequest<T>(ServiceResult<T> result) =>
        Results.BadRequest(new { error = result.Error });
}
