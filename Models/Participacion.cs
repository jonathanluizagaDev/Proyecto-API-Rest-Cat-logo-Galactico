using System.Text.Json.Serialization;
namespace CatalogoGalacticoWebApi.Models;
public class Participacion
{
    public int EventoId { get; set; }
    [JsonIgnore]
    public Evento Evento { get; set; } = null!;
    public int PersonajeId { get; set; }
    [JsonIgnore]
    public Personaje Personaje { get; set; } = null!;
    public bool Fallecido { get; set; }
}
