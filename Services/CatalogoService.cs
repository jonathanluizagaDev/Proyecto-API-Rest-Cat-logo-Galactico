using CatalogoGalacticoWebApi.Data;
using CatalogoGalacticoWebApi.Dtos;
using CatalogoGalacticoWebApi.Models;
using Microsoft.EntityFrameworkCore;
namespace CatalogoGalacticoWebApi.Services;
public class CatalogoService(AppDbContext db) : ICatalogoService
{
    private static readonly string[] Facciones = ["Rebelde", "Imperio", "Neutral"];
    private static readonly string[] Estados = ["vivo", "muerto", "desconocido"];
    private static string? Limpiar(string? s) => s?.Trim();
    private static string? Opcion(string? s, string[] validas) => validas.FirstOrDefault(
        x => x.Equals(Limpiar(s), StringComparison.OrdinalIgnoreCase));
    private static string? ValidarPersonaje(PersonajeInput d) =>
        string.IsNullOrWhiteSpace(d.Nombre) || string.IsNullOrWhiteSpace(d.Especie) || string.IsNullOrWhiteSpace(d.Afiliacion)
            ? "Nombre, especie y afiliación son obligatorios."
        : Opcion(d.Faccion, Facciones) is null ? "Facción: Rebelde, Imperio o Neutral."
        : Opcion(d.Estado, Estados) is null    ? "Estado: vivo, muerto o desconocido."
                                               : null;
    private static void Actualizar(Personaje p, PersonajeInput d)
    {
        p.Nombre = d.Nombre.Trim();
        p.Especie = d.Especie.Trim();
        p.Faccion = Opcion(d.Faccion, Facciones)!;
        p.Afiliacion = d.Afiliacion.Trim();
        p.Estado = Opcion(d.Estado, Estados)!;
        p.FuerzaSensitivo = d.FuerzaSensitivo;
    }
    public Task<List<Personaje>> Personajes(string? faccion, bool? fuerzaSensitivo)
    {
        var q = db.Personajes.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(faccion))
            q = q.Where(x => x.Faccion == faccion.Trim());
        if (fuerzaSensitivo.HasValue)
            q = q.Where(x => x.FuerzaSensitivo == fuerzaSensitivo.Value);
        return q.OrderBy(x => x.Id).ToListAsync();
    }
    public Task<Personaje?> Personaje(int id) => db.Personajes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    public async Task<Resultado<Personaje>> CrearPersonaje(PersonajeInput d)
    {
        var e = ValidarPersonaje(d);
        if (e != null)
            return Resultado<Personaje>.Fail(e);
        var p = new Personaje();
        Actualizar(p, d);
        db.Personajes.Add(p);
        await db.SaveChangesAsync();
        return Resultado<Personaje>.Ok(p, 201);
    }
    public async Task<Resultado<Personaje>> EditarPersonaje(int id, PersonajeInput d)
    {
        var p = await db.Personajes.FindAsync(id);
        if (p == null)
            return Resultado<Personaje>.Fail("Personaje no encontrado", 404);
        var e = ValidarPersonaje(d);
        if (e != null)
            return Resultado<Personaje>.Fail(e);
        if (p.Estado == "muerto" && Opcion(d.Estado, Estados) != "muerto")
            return Resultado<Personaje>.Fail("Un personaje fallecido no puede volver a la vida.", 409);
        Actualizar(p, d);
        await db.SaveChangesAsync();
        return Resultado<Personaje>.Ok(p);
    }
    public async Task<Resultado<bool>> EliminarPersonaje(int id)
    {
        var p = await db.Personajes.FindAsync(id);
        if (p == null)
            return Resultado<bool>.Fail("No existe", 404);
        if (await db.Cartas.AnyAsync(x => x.PersonajeId == id) || await db.Participaciones.AnyAsync(x => x.PersonajeId == id))
            return Resultado<bool>.Fail("Tiene relaciones asociadas", 409);
        db.Personajes.Remove(p);
        await db.SaveChangesAsync();
        return Resultado<bool>.Ok(true, 204);
    }
    public async Task<List<RankingResponse>> Ranking()
    {
        var datos = await db.Cartas.Include(x => x.Personaje)
                        .AsNoTracking()
                        .OrderByDescending(x => x.Poder)
                        .ThenBy(x => x.Personaje.Nombre)
                        .ToListAsync();
        return datos.Select((x, i) => new RankingResponse(i + 1, x.PersonajeId, x.Personaje.Nombre, x.Poder)).ToList();
    }
    public Task<List<Evento>> EventosPersonaje(
        int id) => db.Participaciones.AsNoTracking().Where(x => x.PersonajeId == id).Select(x => x.Evento).ToListAsync();
    public Task<List<Carta>> Cartas() => db.Cartas.AsNoTracking().ToListAsync();
    public Task<Carta?> Carta(int id) => db.Cartas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    private async Task<string?> ValidarCarta(CartaInput d, int? id = null)
    {
        if (!await db.Personajes.AnyAsync(x => x.Id == d.PersonajeId))
            return "Personaje no encontrado";
        if (await db.Cartas.AnyAsync(x => x.PersonajeId == d.PersonajeId && x.Id != id))
            return "Personaje ya tiene carta";
        if (d.Poder < 0 || d.Poder > 100 || d.NivelPeligrosidad < 1 || d.NivelPeligrosidad > 10)
            return "Poder 0-100 y peligrosidad 1-10";
        if (string.IsNullOrWhiteSpace(d.Arma) || string.IsNullOrWhiteSpace(d.HabilidadEspecial))
            return "Arma y habilidad obligatorias";
        if (!Uri.TryCreate(d.ImagenUrl, UriKind.Absolute, out var u) || (u.Scheme != "http" && u.Scheme != "https"))
            return "ImagenUrl debe ser HTTP(S)";
        return null;
    }
    private static void Actualizar(Carta c, CartaInput d)
    {
        c.PersonajeId = d.PersonajeId;
        c.Poder = d.Poder;
        c.HabilidadEspecial = d.HabilidadEspecial.Trim();
        c.Arma = d.Arma.Trim();
        c.NivelPeligrosidad = d.NivelPeligrosidad;
        c.ImagenUrl = d.ImagenUrl.Trim();
    }
    public async Task<Resultado<Carta>> CrearCarta(CartaInput d)
    {
        var e = await ValidarCarta(d);
        if (e != null)
            return Resultado<Carta>.Fail(e, e.Contains("ya tiene") ? 409 : 400);
        var c = new Carta();
        Actualizar(c, d);
        db.Cartas.Add(c);
        await db.SaveChangesAsync();
        return Resultado<Carta>.Ok(c, 201);
    }
    public async Task<Resultado<Carta>> EditarCarta(int id, CartaInput d)
    {
        var c = await db.Cartas.FindAsync(id);
        if (c == null)
            return Resultado<Carta>.Fail("Carta no encontrada", 404);
        if (await db.Participaciones.AnyAsync(x => x.PersonajeId == c.PersonajeId && x.Evento.Simulaciones.Any()))
            return Resultado<Carta>.Fail("Carta usada en un evento simulado", 409);
        var e = await ValidarCarta(d, id);
        if (e != null)
            return Resultado<Carta>.Fail(e, e.Contains("ya tiene") ? 409 : 400);
        Actualizar(c, d);
        await db.SaveChangesAsync();
        return Resultado<Carta>.Ok(c);
    }
    public async Task<Resultado<bool>> EliminarCarta(int id)
    {
        var c = await db.Cartas.FindAsync(id);
        if (c == null)
            return Resultado<bool>.Fail("Carta no encontrada", 404);
        if (await db.Participaciones.AnyAsync(x => x.PersonajeId == c.PersonajeId && x.Evento.Simulaciones.Any()))
            return Resultado<bool>.Fail("Carta usada en simulación", 409);
        db.Cartas.Remove(c);
        await db.SaveChangesAsync();
        return Resultado<bool>.Ok(true, 204);
    }
    public Task<List<Planeta>> Planetas(string? sector)
    {
        var q = db.Planetas.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(sector))
            q = q.Where(x => x.Sector == sector.Trim());
        return q.OrderBy(x => x.Nombre).ToListAsync();
    }
    public Task<Planeta?> Planeta(int id) => db.Planetas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    private static string? ValidarPlaneta(
        PlanetaInput d) => string.IsNullOrWhiteSpace(d.Nombre) || string.IsNullOrWhiteSpace(d.Sector) || string.IsNullOrWhiteSpace(d.Clima)
                               ? "Nombre, sector y clima obligatorios"
                           : d.NivelPeligro is < 1 or > 10 ? "Nivel de peligro de 1 a 10"
                                                           : null;
    private static void Actualizar(Planeta p, PlanetaInput d)
    {
        p.Nombre = d.Nombre.Trim();
        p.Sector = d.Sector.Trim();
        p.Clima = d.Clima.Trim();
        p.NivelPeligro = d.NivelPeligro;
    }
    public async Task<Resultado<Planeta>> CrearPlaneta(PlanetaInput d)
    {
        var e = ValidarPlaneta(d);
        if (e != null)
            return Resultado<Planeta>.Fail(e);
        if (await db.Planetas.AnyAsync(x => x.Nombre == d.Nombre.Trim()))
            return Resultado<Planeta>.Fail("Planeta duplicado", 409);
        var p = new Planeta();
        Actualizar(p, d);
        db.Planetas.Add(p);
        await db.SaveChangesAsync();
        return Resultado<Planeta>.Ok(p, 201);
    }
    public async Task<Resultado<Planeta>> EditarPlaneta(int id, PlanetaInput d)
    {
        var p = await db.Planetas.FindAsync(id);
        if (p == null)
            return Resultado<Planeta>.Fail("Planeta no encontrado", 404);
        var e = ValidarPlaneta(d);
        if (e != null)
            return Resultado<Planeta>.Fail(e);
        if (await db.Planetas.AnyAsync(x => x.Nombre == d.Nombre.Trim() && x.Id != id))
            return Resultado<Planeta>.Fail("Planeta duplicado", 409);
        Actualizar(p, d);
        await db.SaveChangesAsync();
        return Resultado<Planeta>.Ok(p);
    }
    public async Task<Resultado<bool>> EliminarPlaneta(int id)
    {
        var p = await db.Planetas.FindAsync(id);
        if (p == null)
            return Resultado<bool>.Fail("Planeta no encontrado", 404);
        if (await db.Eventos.AnyAsync(x => x.PlanetaId == id))
            return Resultado<bool>.Fail("Planeta con eventos", 409);
        db.Planetas.Remove(p);
        await db.SaveChangesAsync();
        return Resultado<bool>.Ok(true, 204);
    }
    public Task<List<Evento>> EventosPlaneta(
        int id) => db.Eventos.AsNoTracking().Where(x => x.PlanetaId == id).OrderBy(x => x.Fecha).ToListAsync();
    public async Task<List<object>> ActividadPlanetas()
    {
        var datos = await db.Planetas.AsNoTracking()
                        .Select(p => new { p.Id, p.Nombre, TotalEventos = p.Eventos.Count })
                        .OrderByDescending(p => p.TotalEventos)
                        .ToListAsync();
        return datos.Select(x => (object)x).ToList();
    }
    public Task<List<Evento>> Eventos() => db.Eventos.AsNoTracking().Include(x => x.Participaciones).OrderBy(x => x.Id).ToListAsync();
    public Task<Evento?> Evento(int id) => db.Eventos.AsNoTracking().Include(x => x.Participaciones).FirstOrDefaultAsync(x => x.Id == id);
    public Task<EventoDetalleResponse?> EventoDetalle(int id) => db.Eventos.AsNoTracking()
        .Where(x => x.Id == id)
        .Select(x => new EventoDetalleResponse(
            x.Id,
            x.Nombre,
            x.Fecha,
            x.Descripcion,
            x.Estado,
            new PlanetaEventoResponse(x.Planeta.Id, x.Planeta.Nombre, x.Planeta.Sector, x.Planeta.Clima, x.Planeta.NivelPeligro),
            x.Participaciones.OrderBy(p => p.PersonajeId)
                .Select(p => new ParticipanteEventoResponse(
                    p.PersonajeId,
                    p.Personaje.Nombre,
                    p.Personaje.Faccion,
                    p.Personaje.Estado,
                    p.Resultado,
                    p.Personaje.Carta == null ? null : p.Personaje.Carta.Poder))
                .ToList()))
        .FirstOrDefaultAsync();
    private async Task<string?> ValidarEvento(EventoInput d, int? eventoId = null)
    {
        if (string.IsNullOrWhiteSpace(d.Nombre) || string.IsNullOrWhiteSpace(d.Descripcion))
            return "Nombre y descripción obligatorios";
        if (!await db.Planetas.AnyAsync(x => x.Id == d.PlanetaId))
            return "Planeta no existe";
        if (!string.Equals(d.Estado?.Trim(), "pendiente", StringComparison.OrdinalIgnoreCase))
            return "Un evento nuevo o editable debe tener estado pendiente";
        if (d.ParticipanteIds == null || d.ParticipanteIds.Count < 2 || d.ParticipanteIds.Count != d.ParticipanteIds.Distinct().Count())
            return "Se requieren al menos 2 participantes únicos";
        var p = await db.Personajes.Where(x => d.ParticipanteIds.Contains(x.Id)).ToListAsync();
        if (p.Count != d.ParticipanteIds.Count)
            return "Hay personajes inexistentes";
        if (p.Any(x => x.Estado == "muerto"))
            return "No pueden participar personajes muertos";
        if (p.Select(x => x.Faccion).Distinct().Count() < 2)
            return "Se requieren al menos dos facciones";
        return null;
    }
    public async Task<Resultado<Evento>> CrearEvento(EventoInput d)
    {
        var e = await ValidarEvento(d);
        if (e != null)
            return Resultado<Evento>.Fail(e);
        var ev = new Evento { Nombre = d.Nombre.Trim(),
            Fecha = d.Fecha,
            Descripcion = d.Descripcion.Trim(),
            Estado = "pendiente",
            PlanetaId = d.PlanetaId,
            Participaciones = d.ParticipanteIds.Select(id => new Participacion { PersonajeId = id, Resultado = "pendiente" }).ToList() };
        db.Eventos.Add(ev);
        await db.SaveChangesAsync();
        return Resultado<Evento>.Ok(await Evento(ev.Id) ?? ev, 201);
    }
    public async Task<Resultado<Evento>> EditarEvento(int id, EventoInput d)
    {
        var ev = await db.Eventos.Include(x => x.Participaciones).Include(x => x.Simulaciones).FirstOrDefaultAsync(x => x.Id == id);
        if (ev == null)
            return Resultado<Evento>.Fail("Evento no existe", 404);
        if (ev.Simulaciones.Count > 0)
            return Resultado<Evento>.Fail("Evento ya simulado", 409);
        var e = await ValidarEvento(d, id);
        if (e != null)
            return Resultado<Evento>.Fail(e);
        ev.Nombre = d.Nombre.Trim();
        ev.Fecha = d.Fecha;
        ev.Descripcion = d.Descripcion.Trim();
        ev.Estado = "pendiente";
        ev.PlanetaId = d.PlanetaId;
        var viejos = ev.Participaciones.Where(x => !d.ParticipanteIds.Contains(x.PersonajeId)).ToList();
        foreach (var anterior in viejos)
        {
            ev.Participaciones.Remove(anterior);
            db.Participaciones.Remove(anterior);
        }
        foreach (var pid in d.ParticipanteIds.Where(pid => ev.Participaciones.All(x => x.PersonajeId != pid)))
        {
            ev.Participaciones.Add(new Participacion { EventoId = id, PersonajeId = pid, Resultado = "pendiente" });
        }
        await db.SaveChangesAsync();
        return Resultado<Evento>.Ok(await Evento(id) ?? ev);
    }
    public async Task<Resultado<object>> Mvp(int id)
    {
        var ev = await db.Eventos.AsNoTracking().Include(x => x.Participaciones).FirstOrDefaultAsync(x => x.Id == id);
        if (ev == null)
            return Resultado<object>.Fail("Evento no existe", 404);
        var ids = ev.Participaciones.Select(x => x.PersonajeId).ToList();
        var carta =
            await db.Cartas.AsNoTracking().Where(x => ids.Contains(x.PersonajeId)).OrderByDescending(x => x.Poder).FirstOrDefaultAsync();
        if (carta == null)
            return Resultado<object>.Fail("Participantes sin cartas", 400);
        var personaje = await db.Personajes.AsNoTracking().FirstAsync(x => x.Id == carta.PersonajeId);
        return Resultado<object>.Ok(new { personaje, carta });
    }
}
