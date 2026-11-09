using CatalogoGalacticoWebApi.Services;
using Microsoft.AspNetCore.Mvc;
namespace CatalogoGalacticoWebApi.Controllers;
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Enviar<T>(Resultado<T> resultado) => resultado.Error is not null
                                                                     ? StatusCode(resultado.Codigo, new { error = resultado.Error })
                                                                     : StatusCode(resultado.Codigo, resultado.Valor);
}
