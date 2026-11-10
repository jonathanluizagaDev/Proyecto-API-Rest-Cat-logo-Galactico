using CatalogoGalacticoWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogoGalacticoWebApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Personaje> Personajes => Set<Personaje>();
    public DbSet<Carta> Cartas => Set<Carta>();
    public DbSet<Planeta> Planetas => Set<Planeta>();
    public DbSet<Evento> Eventos => Set<Evento>();
    public DbSet<Participacion> Participaciones => Set<Participacion>();
    public DbSet<Simulacion> Simulaciones => Set<Simulacion>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Personaje>(e =>
        {
            e.Property(p => p.Nombre).IsRequired().HasMaxLength(120);
            e.Property(p => p.Faccion).IsRequired().HasMaxLength(40);
            e.Property(p => p.Estado).IsRequired().HasMaxLength(30);
        });

        modelBuilder.Entity<Carta>(e =>
        {
            e.HasOne(c => c.Personaje)
                .WithOne(p => p.Carta)
                .HasForeignKey<Carta>(c => c.PersonajeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Planeta>(e =>
        {
            e.Property(p => p.Nombre).IsRequired().HasMaxLength(120);
            e.HasIndex(p => p.Nombre).IsUnique();
        });

        modelBuilder.Entity<Evento>(e =>
        {
            e.Property(ev => ev.Estado).IsRequired().HasMaxLength(30);
            e.HasOne(ev => ev.Planeta)
                .WithMany(p => p.Eventos)
                .HasForeignKey(ev => ev.PlanetaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Participacion>(e =>
        {
            e.HasKey(p => new { p.EventoId, p.PersonajeId });
            e.Property(p => p.Resultado).IsRequired().HasMaxLength(20);

            e.HasOne(p => p.Evento)
                .WithMany(ev => ev.Participaciones)
                .HasForeignKey(p => p.EventoId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(p => p.Personaje)
                .WithMany(personaje => personaje.Participaciones)
                .HasForeignKey(p => p.PersonajeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Simulacion>(e =>
        {
            e.HasOne(s => s.Evento)
                .WithMany(ev => ev.Simulaciones)
                .HasForeignKey(s => s.EventoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.CorreoBusquedaHash).IsUnique();
            e.Property(u => u.CorreoBusquedaHash).IsRequired().HasMaxLength(64);
            e.Property(u => u.CorreoCifrado).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
        });
    }
}
