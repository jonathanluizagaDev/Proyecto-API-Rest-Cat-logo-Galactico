using System.Text.Json.Serialization;
namespace CatalogoGalacticoWebApi.Models;
public class Evento
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int Fecha { get; set; } // Año galáctico (compatibilidad con la Minimal API original)
    public string Descripcion { get; set; } = "";
    public string Estado { get; set; } = "pendiente";
    public int PlanetaId { get; set; }
    [JsonIgnore]
    public Planeta Planeta { get; set; } = null!;
    public List<Participacion> Participaciones { get; set; } = new();
    [JsonIgnore]
    public List<Simulacion> Simulaciones { get; set; } = new();
}
