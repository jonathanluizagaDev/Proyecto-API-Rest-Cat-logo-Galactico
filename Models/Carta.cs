using System.Text.Json.Serialization;
namespace CatalogoGalacticoWebApi.Models;
public class Carta
{
    public int Id { get; set; }
    public int PersonajeId { get; set; }
    public int Poder { get; set; }
    public string HabilidadEspecial { get; set; } = "";
    public string Arma { get; set; } = "";
    public int NivelPeligrosidad { get; set; }
    public string ImagenUrl { get; set; } = "";
    [JsonIgnore]
    public Personaje Personaje { get; set; } = null!;
}
