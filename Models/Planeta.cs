using System.Text.Json.Serialization;
namespace CatalogoGalacticoWebApi.Models;
public class Planeta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Sector { get; set; } = "";
    public string Clima { get; set; } = "";
    public int NivelPeligro { get; set; }
    [JsonIgnore]
    public List<Evento> Eventos { get; set; } = new();
}
