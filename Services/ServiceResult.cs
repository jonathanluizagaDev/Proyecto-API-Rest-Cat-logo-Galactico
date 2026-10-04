namespace ProyectoCatalogoGalactico.Services;

public record ServiceResult<T>(T? Value, string? Error)
{
    public bool IsSuccess => Error is null;

    public static ServiceResult<T> Ok(T value) => new(value, null);

    public static ServiceResult<T> Fail(string error) => new(default, error);
}
