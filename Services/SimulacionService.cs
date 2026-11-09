using System.Text.Json;
using CatalogoGalacticoWebApi.Data;
using CatalogoGalacticoWebApi.Dtos;
using CatalogoGalacticoWebApi.Models;
using Microsoft.EntityFrameworkCore;
namespace CatalogoGalacticoWebApi.Services;
public class SimulacionService(AppDbContext db, ICatalogoService catalogo) : ISimulacionService
{
    public async Task<Resultado<ResultadoSimulacion>> Resultado(int eventoId)
    {
        var ev = await db.Eventos.AsNoTracking()
                     .Include(x => x.Participaciones)
                     .Include(x => x.Simulaciones)
                     .FirstOrDefaultAsync(x => x.Id == eventoId);
        if (ev == null)
            return Resultado<ResultadoSimulacion>.Fail("Evento no encontrado", 404);
        var sim = ev.Simulaciones.OrderByDescending(x => x.FechaEjecucionUtc).ThenByDescending(x => x.Id).FirstOrDefault();
        if (sim == null)
            return Resultado<ResultadoSimulacion>.Fail("Evento sin simulación", 404);
        var totales = JsonSerializer.Deserialize<List<TotalFaccionResponse>>(sim.TotalesJson) ?? new();
        return Resultado<ResultadoSimulacion>.Ok(new ResultadoSimulacion(ev.Id,
            sim.Tipo,
            sim.Ganador,
            sim.Criterio,
            totales,
            ev.Participaciones.Where(x => x.Fallecido).Select(x => x.PersonajeId).ToList(),
            ev.Participaciones.Where(x => !x.Fallecido).Select(x => x.PersonajeId).ToList()));
    }
    public async Task<Resultado<List<ResultadoSimulacion>>> Historial(int eventoId)
    {
        var ev = await db.Eventos.AsNoTracking()
                     .Include(x => x.Participaciones)
                     .Include(x => x.Simulaciones)
                     .FirstOrDefaultAsync(x => x.Id == eventoId);
        if (ev == null)
            return Resultado<List<ResultadoSimulacion>>.Fail("Evento no encontrado", 404);
        var fallecidos = ev.Participaciones.Where(x => x.Fallecido).Select(x => x.PersonajeId).ToList();
        var sobrevivientes = ev.Participaciones.Where(x => !x.Fallecido).Select(x => x.PersonajeId).ToList();
        var historial = ev.Simulaciones.OrderByDescending(x => x.FechaEjecucionUtc)
                            .ThenByDescending(x => x.Id)
                            .Select(sim => new ResultadoSimulacion(ev.Id,
                                        sim.Tipo,
                                        sim.Ganador,
                                        sim.Criterio,
                                        JsonSerializer.Deserialize<List<TotalFaccionResponse>>(sim.TotalesJson) ?? new(),
                                        fallecidos,
                                        sobrevivientes))
                            .ToList();
        return Resultado<List<ResultadoSimulacion>>.Ok(historial);
    }
    public async Task<Resultado<ResultadoSimulacion>> Simular(int eventoId, string tipo = "batalla", string? faccionDefensora = null)
    {
        var ev = await db.Eventos.Include(x => x.Planeta)
                     .Include(x => x.Participaciones)
                     .ThenInclude(x => x.Personaje)
                     .ThenInclude(x => x.Carta)
                     .Include(x => x.Simulaciones)
                     .FirstOrDefaultAsync(x => x.Id == eventoId);
        if (ev == null)
            return Resultado<ResultadoSimulacion>.Fail("Evento no encontrado", 404);
        var ps = ev.Participaciones.Select(x => x.Personaje).ToList();
        if (ps.Count < 2 || ps.Any(x => x.Carta == null) || ps.Any(x => x.Estado == "muerto"))
            return Resultado<ResultadoSimulacion>.Fail("Se requieren participantes vivos con carta", 400);
        var grupos = ps.GroupBy(x => x.Faccion).OrderBy(x => x.Key).ToList();
        if (grupos.Count != 2)
            return Resultado<ResultadoSimulacion>.Fail("La simulación requiere exactamente dos facciones", 400);
        if (tipo == "invasion" && !grupos.Any(x => x.Key == faccionDefensora))
            return Resultado<ResultadoSimulacion>.Fail("Facción defensora no participa", 400);
        // Fórmula reproducible: poderBase + bonus defensivo (5% x nivel de peligro, máximo 50%).
        var totales = grupos
                          .Select(g =>
                                  {
                                      int basePoder = g.Sum(p => p.Carta!.Poder);
                                      double factor =
                                          (tipo == "invasion" && g.Key == faccionDefensora) ? 1 + ev.Planeta.NivelPeligro * 0.05 : 1.0;
                                      return new TotalFaccionResponse(g.Key, basePoder, factor, Math.Round(basePoder * factor, 2));
                                  })
                          .OrderByDescending(x => x.PoderFinal)
                          .ThenBy(x => x.Faccion)
                          .ToList();
        var empate = totales[0].PoderFinal == totales[1].PoderFinal;
        var ganador = empate ? "Empate" : totales[0].Faccion;
        // Bajas reproducibles: si hay ganador, la facción perdedora pierde hasta la mitad
        // (redondeo hacia arriba) de sus miembros de menor poder; la ganadora no tiene bajas.
        var fallecidos = new List<int>();
        if (!empate)
        {
            var perdedores = ps.Where(x => x.Faccion != ganador).OrderBy(x => x.Carta!.Poder).ThenBy(x => x.Id).ToList();
            fallecidos = perdedores.Take((perdedores.Count + 1) / 2).Select(x => x.Id).ToList();
        }
        foreach (var p in ev.Participaciones.Where(x => fallecidos.Contains(x.PersonajeId)))
        {
            p.Fallecido = true;
            p.Personaje.Estado = "muerto";
        }
        const string criterio = "Se suma el poder de las cartas. En invasiones el defensor recibe 5% por nivel de peligro. Gana el mayor " +
                                "total; si hay ganador fallece la mitad redondeada hacia arriba de los perdedores con menor poder.";
        ev.Simulaciones.Add(
            new Simulacion { Tipo = tipo, Ganador = ganador, Criterio = criterio, TotalesJson = JsonSerializer.Serialize(totales) });
        // La operación es atómica: resultados y estado se guardan en un solo SaveChanges.
        await db.SaveChangesAsync();
        return Resultado<ResultadoSimulacion>.Ok(new ResultadoSimulacion(
            ev.Id, tipo, ganador, criterio, totales, fallecidos, ps.Where(x => !fallecidos.Contains(x.Id)).Select(x => x.Id).ToList()));
    }
    public async Task<Resultado<ResultadoSimulacion>> Invadir(int planetaId, InvasionInput input)
    {
        if (!await db.Planetas.AnyAsync(x => x.Id == planetaId))
            return Resultado<ResultadoSimulacion>.Fail("Planeta no encontrado", 404);
        if (input.ParticipanteIds == null || input.ParticipanteIds.Count < 2 || input.FaccionAtacante == input.FaccionDefensora)
            return Resultado<ResultadoSimulacion>.Fail("Se necesitan dos facciones distintas y participantes", 400);
        var personajes = await db.Personajes.Where(x => input.ParticipanteIds.Contains(x.Id)).ToListAsync();
        if (personajes.Count != input.ParticipanteIds.Distinct().Count() ||
            personajes.Any(x => x.Faccion != input.FaccionAtacante && x.Faccion != input.FaccionDefensora) ||
            !personajes.Any(x => x.Faccion == input.FaccionAtacante) || !personajes.Any(x => x.Faccion == input.FaccionDefensora))
            return Resultado<ResultadoSimulacion>.Fail("Participantes inválidos para las facciones declaradas", 400);
        var creado =
            await catalogo.CrearEvento(new EventoInput(input.Nombre, input.Fecha, input.Descripcion, planetaId, input.ParticipanteIds));
        if (creado.Error != null)
            return Resultado<ResultadoSimulacion>.Fail(creado.Error, creado.Codigo);
        return await Simular(creado.Valor!.Id, "invasion", input.FaccionDefensora);
    }
    public async Task<Resultado<object>> Estadisticas(int planetaId)
    {
        var planeta = await db.Planetas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == planetaId);
        if (planeta == null)
            return Resultado<object>.Fail("Planeta no encontrado", 404);
        var eventos = await db.Eventos.AsNoTracking()
                          .Include(x => x.Simulaciones)
                          .Include(x => x.Participaciones)
                          .Where(x => x.PlanetaId == planetaId)
                          .ToListAsync();
        var simulaciones = eventos.SelectMany(x => x.Simulaciones).ToList();
        return Resultado<object>.Ok(new { planeta = planeta.Nombre,
            totalEventos = eventos.Count,
            batallasSimuladas = simulaciones.Count,
            fallecidos = eventos.Sum(x => x.Participaciones.Count(p => p.Fallecido)),
            victorias = simulaciones.GroupBy(x => x.Ganador).Select(x => new { faccion = x.Key, total = x.Count() }) });
    }
}
