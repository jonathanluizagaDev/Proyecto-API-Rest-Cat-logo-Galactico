using ProyectoCatalogoGalactico.Models;

namespace ProyectoCatalogoGalactico.Data;

public static class GalacticData
{
    public static List<Personaje> Personajes { get; } =
    [
        new(
            1,
            "Luke Skywalker",
            "Humano",
            "Rebelde",
            "Alianza Rebelde",
            "vivo",
            true,"https://upload.wikimedia.org/wikipedia/commons/6/67/Luke_Skywalker_-_Welcome_Banner_%28Cropped%29.jpg"),
        new(
            2,
            "Leia Organa",
            "Humano",
            "Rebelde",
            "Alianza Rebelde",
            "vivo",
            true,"https://upload.wikimedia.org/wikipedia/en/1/1b/Princess_Leia%27s_characteristic_hairstyle.jpg"),
        new(
            3,
            "Darth Vader",
            "Humano",
            "Imperio",
            "Imperio Galáctico",
            "muerto",
            true,
            "https://upload.wikimedia.org/wikipedia/en/0/0b/Darth_Vader_in_The_Empire_Strikes_Back.jpg"),
        new(
            4,
            "Han Solo",
            "Humano",
            "Neutral",
            "Contrabandistas",
            "vivo",
            false,"https://upload.wikimedia.org/wikipedia/en/c/c9/Han_Solo_with_Blaster.jpg")
    ];

    public static List<CardPersonaje> Cartas { get; } =
    [
        new(
            1,
            1,
            92,
            "Dominio de la Fuerza",
            "Sable de luz",
            9,
            "https://upload.wikimedia.org/wikipedia/commons/6/67/Luke_Skywalker_-_Welcome_Banner_%28Cropped%29.jpg"),
        new(
            2,
            2,
            78,
            "Liderazgo",
            "Bláster",
            7,
            "https://upload.wikimedia.org/wikipedia/en/1/1b/Princess_Leia%27s_characteristic_hairstyle.jpg"),
        new(
            3,
            3,
            98,
            "Estrangulamiento de la Fuerza",
            "Sable de luz",
            10,
            "https://upload.wikimedia.org/wikipedia/en/0/0b/Darth_Vader_in_The_Empire_Strikes_Back.jpg"),
        new(
            4,
            4,
            82,
            "Disparo rápido",
            "Bláster DL-44",
            8,
            "https://upload.wikimedia.org/wikipedia/en/c/c9/Han_Solo_with_Blaster.jpg")
    ];

    public static List<Evento> Eventos { get; } =
    [
        new(
            1,
            "Batalla de Yavin",
            0,
            "Yavin IV",
            "Destrucción de la primera Estrella de la Muerte.",
            [1, 2, 3, 4],
            [],
            "Victoria de la Alianza Rebelde",
            "Rebelde"),
        new(
            2,
            "Duelo en la segunda Estrella de la Muerte",
            4,
            "Segunda Estrella de la Muerte",
            "Enfrentamiento final entre Luke Skywalker y Darth Vader.",
            [1, 3],
            [3],
            "Victoria de la Alianza Rebelde",
            "Rebelde")
    ];

    public static int NextId<T>(IEnumerable<T> items, Func<T, int> idSelector) =>
        items.Select(idSelector).DefaultIfEmpty(0).Max() + 1;
}
