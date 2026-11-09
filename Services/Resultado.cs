namespace CatalogoGalacticoWebApi.Services;
public record Resultado<T>(T? Valor, int Codigo, string? Error = null)
{
    public static Resultado<T> Ok(T dato, int codigo = 200) => new(dato, codigo);
    public static Resultado<T> Fail(string mensaje, int codigo = 400) => new(default, codigo, mensaje);
}
