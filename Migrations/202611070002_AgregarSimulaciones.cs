using CatalogoGalacticoWebApi.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
namespace CatalogoGalacticoWebApi.Migrations;
[DbContext(typeof(AppDbContext))]
[Migration("202611070002_AgregarSimulaciones")]
public class AgregarSimulaciones : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.CreateTable("Simulaciones",
            table => new { Id = table.Column<int>("int", nullable: false)
                               .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                EventoId = table.Column<int>("int", nullable: false),
                Tipo = table.Column<string>("longtext", nullable: false),
                Ganador = table.Column<string>("longtext", nullable: false),
                Criterio = table.Column<string>("longtext", nullable: false),
                TotalesJson = table.Column<string>("longtext", nullable: false),
                FechaEjecucionUtc = table.Column<DateTime>("datetime(6)", nullable: false) },
            constraints: table =>
            {
                table.PrimaryKey("PK_Simulaciones", x => x.Id);
                table.ForeignKey(
                    "FK_Simulaciones_Eventos_EventoId", x => x.EventoId, "Eventos", "Id", onDelete: ReferentialAction.Restrict);
            });
        m.CreateIndex("IX_Simulaciones_EventoId", "Simulaciones", "EventoId", unique: true);
    }
    protected override void Down(MigrationBuilder m) => m.DropTable("Simulaciones");
}
