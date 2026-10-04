using ProyectoCatalogoGalactico.Data;
using ProyectoCatalogoGalactico.Dtos;
using ProyectoCatalogoGalactico.Models;

namespace ProyectoCatalogoGalactico.Services;

public class CatalogoService
{
    private static readonly string[] Facciones = ["Rebelde", "Imperio", "Neutral"];
    private static readonly string[] Estados = ["vivo", "muerto", "desconocido"];

    public IEnumerable<Personaje> GetPersonajes(string? faccion, bool? fuerzaSensitivo)
    {
        IEnumerable<Personaje> personajes = GalacticData.Personajes;

        if (!string.IsNullOrWhiteSpace(faccion))
        {
            personajes = personajes.Where(personaje =>
                personaje.Faccion.Equals(faccion.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (fuerzaSensitivo.HasValue)
        {
            personajes = personajes.Where(personaje =>
                personaje.FuerzaSensitivo == fuerzaSensitivo.Value);
        }

        return personajes;
    }

    public Personaje? GetPersonaje(int id) =>
        GalacticData.Personajes.FirstOrDefault(personaje => personaje.Id == id);

    public ServiceResult<Personaje> CreatePersonaje(PersonajeInput input)
    {
        var error = ValidarPersonaje(input);
        if (error is not null)
        {
            return ServiceResult<Personaje>.Fail(error);
        }

        var personaje = new Personaje(
            GalacticData.NextId(GalacticData.Personajes, item => item.Id),
            input.Nombre.Trim(),
            input.Especie.Trim(),
            NormalizarValor(input.Faccion, Facciones),
            input.Afiliacion.Trim(),
            NormalizarValor(input.Estado, Estados),
            input.FuerzaSensitivo);

        GalacticData.Personajes.Add(personaje);
        return ServiceResult<Personaje>.Ok(personaje);
    }

    public ServiceResult<Personaje> UpdatePersonaje(int id, PersonajeInput input)
    {
        var index = GalacticData.Personajes.FindIndex(personaje => personaje.Id == id);
        if (index < 0)
        {
            return ServiceResult<Personaje>.Fail("El personaje no existe.");
        }

        var error = ValidarPersonaje(input);
        if (error is not null)
        {
            return ServiceResult<Personaje>.Fail(error);
        }

        var estado = NormalizarValor(input.Estado, Estados);
        var tieneMuerteRegistrada = GalacticData.Eventos.Any(evento => evento.FallecidoIds.Contains(id));

        if (tieneMuerteRegistrada && estado != "muerto")
        {
            return ServiceResult<Personaje>.Fail(
                "El personaje tiene una muerte registrada y debe conservar el estado muerto.");
        }

        var personajeActualizado = new Personaje(
            id,
            input.Nombre.Trim(),
            input.Especie.Trim(),
            NormalizarValor(input.Faccion, Facciones),
            input.Afiliacion.Trim(),
            estado,
            input.FuerzaSensitivo);

        GalacticData.Personajes[index] = personajeActualizado;
        return ServiceResult<Personaje>.Ok(personajeActualizado);
    }

    public ServiceResult<bool> DeletePersonaje(int id)
    {
        var personaje = GetPersonaje(id);
        if (personaje is null)
        {
            return ServiceResult<bool>.Fail("El personaje no existe.");
        }

        var tieneCarta = GalacticData.Cartas.Any(carta => carta.PersonajeId == id);
        var participaEnEventos = GalacticData.Eventos.Any(evento => evento.ParticipanteIds.Contains(id));

        if (tieneCarta || participaEnEventos)
        {
            return ServiceResult<bool>.Fail(
                "No se puede eliminar el personaje porque tiene cartas o eventos relacionados.");
        }

        GalacticData.Personajes.Remove(personaje);
        return ServiceResult<bool>.Ok(true);
    }

    public IEnumerable<Evento> GetEventosDePersonaje(int personajeId) =>
        GalacticData.Eventos.Where(evento => evento.ParticipanteIds.Contains(personajeId));

    public IEnumerable<PersonajeRankingResponse> GetRankingPorPoder() =>
        (from personaje in GalacticData.Personajes
         join carta in GalacticData.Cartas on personaje.Id equals carta.PersonajeId
         orderby carta.Poder descending, personaje.Nombre
         select new { personaje, carta })
        .Select((item, posicion) => new PersonajeRankingResponse(
            posicion + 1,
            item.personaje.Id,
            item.personaje.Nombre,
            item.carta.Poder));

    public IEnumerable<CardPersonaje> GetCartas() => GalacticData.Cartas;

    public CardPersonaje? GetCarta(int id) =>
        GalacticData.Cartas.FirstOrDefault(carta => carta.Id == id);

    public ServiceResult<CardPersonaje> CreateCarta(CardPersonajeInput input)
    {
        var error = ValidarCarta(input);
        if (error is not null)
        {
            return ServiceResult<CardPersonaje>.Fail(error);
        }

        var carta = new CardPersonaje(
            GalacticData.NextId(GalacticData.Cartas, item => item.Id),
            input.PersonajeId,
            input.Poder,
            input.HabilidadEspecial.Trim(),
            input.Arma.Trim(),
            input.NivelPeligrosidad,
            input.ImagenUrl.Trim());

        GalacticData.Cartas.Add(carta);
        return ServiceResult<CardPersonaje>.Ok(carta);
    }

    public ServiceResult<CardPersonaje> UpdateCarta(int id, CardPersonajeInput input)
    {
        var index = GalacticData.Cartas.FindIndex(carta => carta.Id == id);
        if (index < 0)
        {
            return ServiceResult<CardPersonaje>.Fail("La carta no existe.");
        }

        var error = ValidarCarta(input, id);
        if (error is not null)
        {
            return ServiceResult<CardPersonaje>.Fail(error);
        }

        var cartaActualizada = new CardPersonaje(
            id,
            input.PersonajeId,
            input.Poder,
            input.HabilidadEspecial.Trim(),
            input.Arma.Trim(),
            input.NivelPeligrosidad,
            input.ImagenUrl.Trim());

        GalacticData.Cartas[index] = cartaActualizada;
        return ServiceResult<CardPersonaje>.Ok(cartaActualizada);
    }

    public IEnumerable<Evento> GetEventos() => GalacticData.Eventos;

    public Evento? GetEvento(int id) =>
        GalacticData.Eventos.FirstOrDefault(evento => evento.Id == id);

    public ServiceResult<Evento> CreateEvento(EventoInput input)
    {
        var error = ValidarEvento(input);
        if (error is not null)
        {
            return ServiceResult<Evento>.Fail(error);
        }

        var evento = new Evento(
            GalacticData.NextId(GalacticData.Eventos, item => item.Id),
            input.Nombre.Trim(),
            input.Fecha,
            input.Ubicacion.Trim(),
            input.Descripcion.Trim(),
            [.. input.ParticipanteIds.Distinct()],
            [.. input.FallecidoIds.Distinct()],
            null,
            null);

        GalacticData.Eventos.Add(evento);
        AplicarFallecimientos(evento.FallecidoIds);

        return ServiceResult<Evento>.Ok(evento);
    }

    public ServiceResult<Evento> UpdateEvento(int id, EventoInput input)
    {
        var index = GalacticData.Eventos.FindIndex(evento => evento.Id == id);
        if (index < 0)
        {
            return ServiceResult<Evento>.Fail("El evento no existe.");
        }

        var error = ValidarEvento(input, id);
        if (error is not null)
        {
            return ServiceResult<Evento>.Fail(error);
        }

        var eventoActualizado = new Evento(
            id,
            input.Nombre.Trim(),
            input.Fecha,
            input.Ubicacion.Trim(),
            input.Descripcion.Trim(),
            [.. input.ParticipanteIds.Distinct()],
            [.. input.FallecidoIds.Distinct()],
            null,
            null);

        GalacticData.Eventos[index] = eventoActualizado;
        AplicarFallecimientos(eventoActualizado.FallecidoIds);

        return ServiceResult<Evento>.Ok(eventoActualizado);
    }

    public ServiceResult<MvpResponse> GetMvp(int eventoId)
    {
        var evento = GetEvento(eventoId);
        if (evento is null)
        {
            return ServiceResult<MvpResponse>.Fail("El evento no existe.");
        }

        var carta = GalacticData.Cartas
            .Where(item => evento.ParticipanteIds.Contains(item.PersonajeId))
            .MaxBy(item => item.Poder);

        if (carta is null)
        {
            return ServiceResult<MvpResponse>.Fail("Ningún participante tiene una carta.");
        }

        var personaje = GetPersonaje(carta.PersonajeId)!;
        return ServiceResult<MvpResponse>.Ok(new MvpResponse(personaje, carta));
    }

    public ServiceResult<SimulacionResponse> SimularBatalla(int eventoId)
    {
        var evento = GetEvento(eventoId);
        if (evento is null)
        {
            return ServiceResult<SimulacionResponse>.Fail("El evento no existe.");
        }

        if (evento.ParticipanteIds.Count < 2)
        {
            return ServiceResult<SimulacionResponse>.Fail(
                "El evento necesita al menos dos participantes para simular una batalla.");
        }

        var participantes = GalacticData.Personajes
            .Where(personaje => evento.ParticipanteIds.Contains(personaje.Id))
            .ToList();

        if (participantes.Count != evento.ParticipanteIds.Count)
        {
            return ServiceResult<SimulacionResponse>.Fail(
                "Uno o más participantes del evento ya no existen.");
        }

        var cartas = GalacticData.Cartas
            .Where(carta => evento.ParticipanteIds.Contains(carta.PersonajeId))
            .ToList();

        if (cartas.Count != participantes.Count)
        {
            return ServiceResult<SimulacionResponse>.Fail(
                "Todos los participantes deben tener una carta para simular la batalla.");
        }

        var cartasPorPersonaje = cartas.ToDictionary(carta => carta.PersonajeId);
        var random = new Random(unchecked(evento.Id * 397 ^ evento.Fecha));

        var totales = participantes
            .GroupBy(personaje => personaje.Faccion)
            .Select(grupo =>
            {
                var poderBase = grupo.Sum(personaje => cartasPorPersonaje[personaje.Id].Poder);
                var factor = Math.Round(0.90 + random.NextDouble() * 0.20, 2);
                var poderFinal = Math.Round(poderBase * factor, 2);

                return new TotalFaccionResponse(grupo.Key, poderBase, factor, poderFinal);
            })
            .OrderByDescending(total => total.PoderFinal)
            .ToList();

        if (totales.Count < 2)
        {
            return ServiceResult<SimulacionResponse>.Fail(
                "La batalla requiere participantes de al menos dos facciones.");
        }

        var hayEmpate = Math.Abs(totales[0].PoderFinal - totales[1].PoderFinal) < 0.001;
        var ganador = hayEmpate ? "Empate" : totales[0].Faccion;
        const string criterio =
            "Suma del poder de las cartas por facción, ajustada por un factor entre 0.90 y 1.10.";

        var index = GalacticData.Eventos.FindIndex(item => item.Id == eventoId);
        GalacticData.Eventos[index] = evento with
        {
            Resultado = hayEmpate ? "La batalla terminó en empate." : $"Victoria de la facción {ganador}.",
            Ganador = ganador
        };

        return ServiceResult<SimulacionResponse>.Ok(
            new SimulacionResponse(eventoId, ganador, criterio, totales));
    }

    private static string? ValidarPersonaje(PersonajeInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Nombre))
        {
            return "El nombre es obligatorio.";
        }

        if (string.IsNullOrWhiteSpace(input.Especie))
        {
            return "La especie es obligatoria.";
        }

        if (string.IsNullOrWhiteSpace(input.Afiliacion))
        {
            return "La afiliación es obligatoria.";
        }

        if (!ContieneValor(Facciones, input.Faccion))
        {
            return "La facción debe ser Rebelde, Imperio o Neutral.";
        }

        if (!ContieneValor(Estados, input.Estado))
        {
            return "El estado debe ser vivo, muerto o desconocido.";
        }

        return null;
    }

    private static string? ValidarCarta(CardPersonajeInput input, int? cartaId = null)
    {
        if (GalacticData.Personajes.All(personaje => personaje.Id != input.PersonajeId))
        {
            return "El personaje indicado no existe.";
        }

        if (GalacticData.Cartas.Any(carta =>
                carta.PersonajeId == input.PersonajeId && carta.Id != cartaId))
        {
            return "El personaje ya tiene una carta principal.";
        }

        if (input.Poder is < 0 or > 100)
        {
            return "El poder debe estar entre 0 y 100.";
        }

        if (input.NivelPeligrosidad is < 1 or > 10)
        {
            return "El nivel de peligrosidad debe estar entre 1 y 10.";
        }

        if (string.IsNullOrWhiteSpace(input.HabilidadEspecial) || string.IsNullOrWhiteSpace(input.Arma))
        {
            return "La habilidad especial y el arma son obligatorias.";
        }

        if (!Uri.TryCreate(input.ImagenUrl, UriKind.Absolute, out var imagenUri) ||
            (imagenUri.Scheme != Uri.UriSchemeHttp && imagenUri.Scheme != Uri.UriSchemeHttps))
        {
            return "La URL de la imagen debe ser una dirección HTTP o HTTPS válida.";
        }

        return null;
    }

    private static string? ValidarEvento(EventoInput input, int? eventoId = null)
    {
        if (string.IsNullOrWhiteSpace(input.Nombre) ||
            string.IsNullOrWhiteSpace(input.Ubicacion) ||
            string.IsNullOrWhiteSpace(input.Descripcion))
        {
            return "El nombre, la ubicación y la descripción son obligatorios.";
        }

        if (input.ParticipanteIds is null || input.FallecidoIds is null)
        {
            return "Las listas de participantes y fallecidos son obligatorias.";
        }

        if (input.ParticipanteIds.Count != input.ParticipanteIds.Distinct().Count())
        {
            return "La lista de participantes contiene identificadores repetidos.";
        }

        if (input.FallecidoIds.Count != input.FallecidoIds.Distinct().Count())
        {
            return "La lista de fallecidos contiene identificadores repetidos.";
        }

        if (input.ParticipanteIds.Any(id => GalacticData.Personajes.All(personaje => personaje.Id != id)))
        {
            return "Uno o más participantes no existen.";
        }

        if (input.FallecidoIds.Any(id => !input.ParticipanteIds.Contains(id)))
        {
            return "Todo personaje fallecido debe ser participante del evento.";
        }

        foreach (var personajeId in input.ParticipanteIds)
        {
            var fechaMuerteAnterior = GalacticData.Eventos
                .Where(evento => evento.Id != eventoId && evento.FallecidoIds.Contains(personajeId))
                .Select(evento => (int?)evento.Fecha)
                .Min();

            if (fechaMuerteAnterior.HasValue && input.Fecha > fechaMuerteAnterior.Value)
            {
                return $"El personaje {personajeId} no puede participar después de su muerte " +
                       $"registrada en el año {fechaMuerteAnterior.Value}.";
            }
        }

        foreach (var personajeId in input.FallecidoIds)
        {
            var muerteYaRegistrada = GalacticData.Eventos.Any(evento =>
                evento.Id != eventoId && evento.FallecidoIds.Contains(personajeId));

            if (muerteYaRegistrada)
            {
                return $"El personaje {personajeId} ya tiene una muerte registrada en otro evento.";
            }
        }

        return null;
    }

    private static void AplicarFallecimientos(IEnumerable<int> fallecidoIds)
    {
        foreach (var personajeId in fallecidoIds)
        {
            var index = GalacticData.Personajes.FindIndex(personaje => personaje.Id == personajeId);
            if (index >= 0)
            {
                GalacticData.Personajes[index] = GalacticData.Personajes[index] with
                {
                    Estado = "muerto"
                };
            }
        }
    }

    private static bool ContieneValor(IEnumerable<string> valores, string valor) =>
        !string.IsNullOrWhiteSpace(valor) &&
        valores.Any(item => item.Equals(valor.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string NormalizarValor(string valor, IEnumerable<string> valores) =>
        valores.First(item => item.Equals(valor.Trim(), StringComparison.OrdinalIgnoreCase));
}
