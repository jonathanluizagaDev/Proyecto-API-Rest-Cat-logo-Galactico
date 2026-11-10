using System.Text.Json;
using CatalogoGalacticoWebApi.Data;
using CatalogoGalacticoWebApi.Dtos;
using CatalogoGalacticoWebApi.Models;
using Microsoft.EntityFrameworkCore;
namespace CatalogoGalacticoWebApi.Services;
public class SimulacionService(AppDbContext db, ICatalogoService catalogo) : ISimulacionService
{
    private static List<int> LeerIds(string json) => JsonSerializer.Deserialize<List<int>>(json) ?? new();
    private static ResultadoSimulacion Convertir(int eventoId, Simulacion sim) => new(
        eventoId,
        sim.Tipo,
        sim.Ganador,
        sim.Criterio,
        JsonSerializer.Deserialize<List<TotalFaccionResponse>>(sim.TotalesJson) ?? new(),
        sim.CantidadDeBajas,
        LeerIds(sim.FallecidosJson),
        LeerIds(sim.SobrevivientesJson),
        sim.FechaEjecucionUtc);

    public async Task<Resultado<ResultadoSimulacion>> Resultado(int eventoId)
    {
        var ev = await db.Eventos.AsNoTracking()
                     .Include(x => x.Simulaciones)
                     .FirstOrDefaultAsync(x => x.Id == eventoId);
        if (ev == null)
            return Resultado<ResultadoSimulacion>.Fail("Evento no encontrado", 404);
        var sim = ev.Simulaciones.OrderByDescending(x => x.FechaEjecucionUtc).ThenByDescending(x => x.Id).FirstOrDefault();
        if (sim == null)
            return Resultado<ResultadoSimulacion>.Fail("Evento sin simulación", 404);
        return Resultado<ResultadoSimulacion>.Ok(Convertir(ev.Id, sim));
    }
    public async Task<Resultado<List<ResultadoSimulacion>>> Historial(int eventoId)
    {
        var ev = await db.Eventos.AsNoTracking()
                     .Include(x => x.Simulaciones)
                     .FirstOrDefaultAsync(x => x.Id == eventoId);
        if (ev == null)
            return Resultado<List<ResultadoSimulacion>>.Fail("Evento no encontrado", 404);
        var historial = ev.Simulaciones.OrderByDescending(x => x.FechaEjecucionUtc)
                            .ThenByDescending(x => x.Id)
                            .Select(sim => Convertir(ev.Id, sim))
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
        var disponibles = ev.Participaciones
            .Where(x => x.Resultado != "fallecido" && x.Personaje.Estado.Equals("vivo", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.PersonajeId)
            .ToList();
        var ps = disponibles.Select(x => x.Personaje).ToList();
        if (ps.Count < 2 || ps.Any(x => x.Carta == null))
            return Resultado<ResultadoSimulacion>.Fail("Se requieren participantes vivos con carta", 400);
        var grupos = ps.GroupBy(x => x.Faccion).OrderBy(x => x.Key).ToList();
        if (grupos.Count != 2)
            return Resultado<ResultadoSimulacion>.Fail("La simulación requiere exactamente dos facciones", 400);
        faccionDefensora ??= disponibles[0].Personaje.Faccion;
        if (!grupos.Any(x => x.Key == faccionDefensora))
            return Resultado<ResultadoSimulacion>.Fail("Facción defensora no participa", 400);
        // El planeta siempre influye: el defensor recibe 5% por nivel de peligro (máximo 50%).
        var totales = grupos
                          .Select(g =>
                                  {
                                      int basePoder = g.Sum(p => p.Carta!.Poder);
                                      double factor = g.Key == faccionDefensora ? 1 + ev.Planeta.NivelPeligro * 0.05 : 1.0;
                                      return new TotalFaccionResponse(g.Key, basePoder, factor, Math.Round(basePoder * factor, 2));
                                  })
                          .OrderByDescending(x => x.PoderFinal)
                          .ThenByDescending(x => x.PoderBase)
                          .ThenBy(x => x.Faccion)
                          .ToList();
        var ganador = totales[0].Faccion;
        var perdedores = ps.Where(x => x.Faccion != ganador).OrderBy(x => x.Carta!.Poder).ThenBy(x => x.Id).ToList();
        var poderGanador = totales[0].PoderFinal;
        var poderPerdedor = totales[1].PoderFinal;
        var poderTotal = poderGanador + poderPerdedor;
        var proporcionPoder = poderTotal == 0 ? 0.5 : poderGanador / poderTotal;
        var tasaBajas = Math.Min(1.0, 0.25 + 0.50 * proporcionPoder + 0.025 * ev.Planeta.NivelPeligro);
        var cantidadDeBajas = Math.Max(1, (int)Math.Ceiling(perdedores.Count * tasaBajas));
        var fallecidos = perdedores.Take(cantidadDeBajas).Select(x => x.Id).ToList();
        var sobrevivientes = ps.Where(x => !fallecidos.Contains(x.Id)).Select(x => x.Id).OrderBy(x => x).ToList();
        foreach (var p in disponibles)
        {
            p.Resultado = fallecidos.Contains(p.PersonajeId) ? "fallecido" : "sobreviviente";
            if (p.Resultado == "fallecido")
                p.Personaje.Estado = "muerto";
        }
        var criterio = $"Defensor: {faccionDefensora}. Recibe 5% de poder por cada nivel de peligro. " +
                       "Gana el mayor poder final; los empates se resuelven por poder base y luego alfabéticamente. " +
                       "Bajas = techo(perdedores x [25% + 50% de poder relativo ganador + 2.5% por nivel de peligro]); " +
                       "fallecen primero los personajes perdedores de menor poder.";
        var fecha = DateTime.UtcNow;
        ev.Estado = "simulado";
        ev.Simulaciones.Add(new Simulacion
        {
            Tipo = tipo,
            Ganador = ganador,
            Criterio = criterio,
            TotalesJson = JsonSerializer.Serialize(totales),
            CantidadDeBajas = fallecidos.Count,
            FallecidosJson = JsonSerializer.Serialize(fallecidos),
            SobrevivientesJson = JsonSerializer.Serialize(sobrevivientes),
            FechaEjecucionUtc = fecha
        });
        var transaccionPropia = db.Database.CurrentTransaction == null;
        await using var transaccion = transaccionPropia ? await db.Database.BeginTransactionAsync() : null;
        try
        {
            await db.SaveChangesAsync();
            if (transaccion != null)
                await transaccion.CommitAsync();
        }
        catch
        {
            if (transaccion != null)
                await transaccion.RollbackAsync();
            throw;
        }
        return Resultado<ResultadoSimulacion>.Ok(new ResultadoSimulacion(
            ev.Id, tipo, ganador, criterio, totales, fallecidos.Count, fallecidos, sobrevivientes, fecha));
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
        await using var transaccion = await db.Database.BeginTransactionAsync();
        try
        {
            var creado =
                await catalogo.CrearEvento(new EventoInput(input.Nombre, input.Fecha, input.Descripcion, planetaId, input.ParticipanteIds));
            if (creado.Error != null)
            {
                await transaccion.RollbackAsync();
                return Resultado<ResultadoSimulacion>.Fail(creado.Error, creado.Codigo);
            }
            var resultado = await Simular(creado.Valor!.Id, "invasion", input.FaccionDefensora);
            if (resultado.Error != null)
            {
                await transaccion.RollbackAsync();
                return resultado;
            }
            await transaccion.CommitAsync();
            return resultado;
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
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
            fallecidos = simulaciones.Sum(x => x.CantidadDeBajas),
            victorias = simulaciones.GroupBy(x => x.Ganador).Select(x => new { faccion = x.Key, total = x.Count() }) });
    }
}
