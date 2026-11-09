using CatalogoGalacticoWebApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
namespace CatalogoGalacticoWebApi.Migrations;
[DbContext(typeof(AppDbContext))]
public class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder m)
    {
        m.Entity("CatalogoGalacticoWebApi.Models.Personaje",
            b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd();
                b.Property<string>("Nombre").IsRequired().HasMaxLength(120);
                b.Property<string>("Especie").IsRequired();
                b.Property<string>("Faccion").IsRequired().HasMaxLength(40);
                b.Property<string>("Afiliacion").IsRequired();
                b.Property<string>("Estado").IsRequired().HasMaxLength(30);
                b.Property<bool>("FuerzaSensitivo");
                b.HasKey("Id");
                b.ToTable("Personajes");
            });
        m.Entity("CatalogoGalacticoWebApi.Models.Planeta",
            b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd();
                b.Property<string>("Nombre").IsRequired().HasMaxLength(120);
                b.Property<string>("Sector").IsRequired();
                b.Property<string>("Clima").IsRequired();
                b.Property<int>("NivelPeligro");
                b.HasKey("Id");
                b.HasIndex("Nombre").IsUnique();
                b.ToTable("Planetas");
            });
        m.Entity("CatalogoGalacticoWebApi.Models.Usuario",
            b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd();
                b.Property<string>("Nombre").IsRequired();
                b.Property<string>("CorreoBusquedaHash").IsRequired().HasMaxLength(64);
                b.Property<string>("CorreoCifrado").IsRequired();
                b.Property<string>("PasswordHash").IsRequired();
                b.HasKey("Id");
                b.HasIndex("CorreoBusquedaHash").IsUnique();
                b.ToTable("Usuarios");
            });
        m.Entity("CatalogoGalacticoWebApi.Models.Carta",
            b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd();
                b.Property<int>("PersonajeId");
                b.Property<int>("Poder");
                b.Property<string>("HabilidadEspecial").IsRequired();
                b.Property<string>("Arma").IsRequired();
                b.Property<int>("NivelPeligrosidad");
                b.Property<string>("ImagenUrl").IsRequired();
                b.HasKey("Id");
                b.HasIndex("PersonajeId").IsUnique();
                b.ToTable("Cartas");
            });
        m.Entity("CatalogoGalacticoWebApi.Models.Evento",
            b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd();
                b.Property<string>("Nombre").IsRequired();
                b.Property<int>("Fecha");
                b.Property<string>("Descripcion").IsRequired();
                b.Property<int>("PlanetaId");
                b.HasKey("Id");
                b.HasIndex("PlanetaId");
                b.ToTable("Eventos");
            });
        m.Entity("CatalogoGalacticoWebApi.Models.Participacion",
            b =>
            {
                b.Property<int>("EventoId");
                b.Property<int>("PersonajeId");
                b.Property<bool>("Fallecido");
                b.HasKey("EventoId", "PersonajeId");
                b.HasIndex("PersonajeId");
                b.ToTable("Participaciones");
            });
        m.Entity("CatalogoGalacticoWebApi.Models.Simulacion",
            b =>
            {
                b.Property<int>("Id").ValueGeneratedOnAdd();
                b.Property<int>("EventoId");
                b.Property<string>("Tipo").IsRequired();
                b.Property<string>("Ganador").IsRequired();
                b.Property<string>("Criterio").IsRequired();
                b.Property<string>("TotalesJson").IsRequired();
                b.Property<DateTime>("FechaEjecucionUtc");
                b.HasKey("Id");
                b.HasIndex("EventoId");
                b.ToTable("Simulaciones");
            });
        m.Entity("CatalogoGalacticoWebApi.Models.Carta",
            b => b.HasOne("CatalogoGalacticoWebApi.Models.Personaje", "Personaje")
                .WithOne("Carta")
                .HasForeignKey("CatalogoGalacticoWebApi.Models.Carta", "PersonajeId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired());
        m.Entity("CatalogoGalacticoWebApi.Models.Evento",
            b => b.HasOne("CatalogoGalacticoWebApi.Models.Planeta", "Planeta")
                .WithMany("Eventos")
                .HasForeignKey("PlanetaId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired());
        m.Entity("CatalogoGalacticoWebApi.Models.Participacion",
            b =>
            {
                b.HasOne("CatalogoGalacticoWebApi.Models.Evento", "Evento")
                    .WithMany("Participaciones")
                    .HasForeignKey("EventoId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
                b.HasOne("CatalogoGalacticoWebApi.Models.Personaje", "Personaje")
                    .WithMany("Participaciones")
                    .HasForeignKey("PersonajeId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();
            });
        m.Entity("CatalogoGalacticoWebApi.Models.Simulacion",
            b => b.HasOne("CatalogoGalacticoWebApi.Models.Evento", "Evento")
                .WithMany("Simulaciones")
                .HasForeignKey("EventoId")
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired());
    }
}
