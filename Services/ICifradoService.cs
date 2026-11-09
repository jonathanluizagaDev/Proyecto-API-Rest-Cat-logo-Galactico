namespace CatalogoGalacticoWebApi.Services;
public interface ICifradoService
{
    string Cifrar(string texto);
    string Descifrar(string texto);
    string HuellaCorreo(string correo);
}
