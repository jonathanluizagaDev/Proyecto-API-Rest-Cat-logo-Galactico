using CatalogoGalacticoWebApi.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
namespace CatalogoGalacticoWebApi.Migrations;
[DbContext(typeof(AppDbContext))]
[Migration("202611070001_Inicial")]
public class Inicial : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.CreateTable("Personajes",
            table => new { Id = table.Column<int>("int", nullable: false)
                               .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Nombre = table.Column<string>("varchar(120)", maxLength: 120, nullable: false),
                Especie = table.Column<string>("longtext", nullable: false),
                Faccion = table.Column<string>("varchar(40)", maxLength: 40, nullable: false),
                Afiliacion = table.Column<string>("longtext", nullable: false),
                Estado = table.Column<string>("varchar(30)", maxLength: 30, nullable: false),
                FuerzaSensitivo = table.Column<bool>("tinyint(1)", nullable: false) },
            constraints: table => table.PrimaryKey("PK_Personajes", x => x.Id));
        m.CreateTable("Planetas",
            table => new { Id = table.Column<int>("int", nullable: false)
                               .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Nombre = table.Column<string>("varchar(120)", maxLength: 120, nullable: false),
                Sector = table.Column<string>("longtext", nullable: false),
                Clima = table.Column<string>("longtext", nullable: false),
                NivelPeligro = table.Column<int>("int", nullable: false) },
            constraints: table => table.PrimaryKey("PK_Planetas", x => x.Id));
        m.CreateTable("Usuarios",
            table => new { Id = table.Column<int>("int", nullable: false)
                               .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Nombre = table.Column<string>("longtext", nullable: false),
                CorreoBusquedaHash = table.Column<string>("varchar(64)", maxLength: 64, nullable: false),
                CorreoCifrado = table.Column<string>("longtext", nullable: false),
                PasswordHash = table.Column<string>("longtext", nullable: false) },
            constraints: table => table.PrimaryKey("PK_Usuarios", x => x.Id));
        m.CreateTable("Cartas",
            table => new { Id = table.Column<int>("int", nullable: false)
                               .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                PersonajeId = table.Column<int>("int", nullable: false),
                Poder = table.Column<int>("int", nullable: false),
                HabilidadEspecial = table.Column<string>("longtext", nullable: false),
                Arma = table.Column<string>("longtext", nullable: false),
                NivelPeligrosidad = table.Column<int>("int", nullable: false),
                ImagenUrl = table.Column<string>("longtext", nullable: false) },
            constraints: table =>
            {
                table.PrimaryKey("PK_Cartas", x => x.Id);
                table.ForeignKey(
                    "FK_Cartas_Personajes_PersonajeId", x => x.PersonajeId, "Personajes", "Id", onDelete: ReferentialAction.Restrict);
            });
        m.CreateTable("Eventos",
            table => new { Id = table.Column<int>("int", nullable: false)
                               .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                Nombre = table.Column<string>("longtext", nullable: false),
                Fecha = table.Column<int>("int", nullable: false),
                Descripcion = table.Column<string>("longtext", nullable: false),
                PlanetaId = table.Column<int>("int", nullable: false) },
            constraints: table =>
            {
                table.PrimaryKey("PK_Eventos", x => x.Id);
                table.ForeignKey("FK_Eventos_Planetas_PlanetaId", x => x.PlanetaId, "Planetas", "Id", onDelete: ReferentialAction.Restrict);
            });
        m.CreateTable("Participaciones",
            table => new { EventoId = table.Column<int>("int", nullable: false),
                PersonajeId = table.Column<int>("int", nullable: false),
                Fallecido = table.Column<bool>("tinyint(1)", nullable: false) },
            constraints: table =>
            {
                table.PrimaryKey("PK_Participaciones", x => new { x.EventoId, x.PersonajeId });
                table.ForeignKey(
                    "FK_Participaciones_Eventos_EventoId", x => x.EventoId, "Eventos", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_Participaciones_Personajes_PersonajeId",
                    x => x.PersonajeId,
                    "Personajes",
                    "Id",
                    onDelete: ReferentialAction.Restrict);
            });
        m.CreateIndex("IX_Planetas_Nombre", "Planetas", "Nombre", unique: true);
        m.CreateIndex("IX_Usuarios_CorreoBusquedaHash", "Usuarios", "CorreoBusquedaHash", unique: true);
        m.CreateIndex("IX_Cartas_PersonajeId", "Cartas", "PersonajeId", unique: true);
        m.CreateIndex("IX_Eventos_PlanetaId", "Eventos", "PlanetaId");
        m.CreateIndex("IX_Participaciones_PersonajeId", "Participaciones", "PersonajeId");
    }
    protected override void Down(MigrationBuilder m)
    {
        m.DropTable("Cartas");
        m.DropTable("Participaciones");
        m.DropTable("Usuarios");
        m.DropTable("Eventos");
        m.DropTable("Personajes");
        m.DropTable("Planetas");
    }
}
