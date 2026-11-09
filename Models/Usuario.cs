namespace CatalogoGalacticoWebApi.Models;
public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string CorreoBusquedaHash { get; set; } = "";
    public string CorreoCifrado { get; set; } = "";
    public string PasswordHash { get; set; } = "";
}
