using System.Text.Json.Serialization;
namespace CatalogoGalacticoWebApi.Models;
public class Simulacion
{
    public int Id { get; set; }
    public int EventoId { get; set; }
    [JsonIgnore]
    public Evento Evento { get; set; } = null!;
    public string Tipo { get; set; } = "batalla";
    public string Ganador { get; set; } = "";
    public string Criterio { get; set; } = "";
    public string TotalesJson { get; set; } = "[]";
    public int CantidadDeBajas { get; set; }
    public string FallecidosJson { get; set; } = "[]";
    public string SobrevivientesJson { get; set; } = "[]";
    public DateTime FechaEjecucionUtc { get; set; } = DateTime.UtcNow;
}
