using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
namespace CatalogoGalacticoWebApi.Models;
public class Participacion
{
    public int EventoId { get; set; }
    [JsonIgnore]
    public Evento Evento { get; set; } = null!;
    public int PersonajeId { get; set; }
    [JsonIgnore]
    public Personaje Personaje { get; set; } = null!;
    public string Resultado { get; set; } = "pendiente";
    [JsonIgnore]
    [NotMapped]
    public bool Fallecido => Resultado == "fallecido";
}
