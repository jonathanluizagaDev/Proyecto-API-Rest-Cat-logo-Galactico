using CatalogoGalacticoWebApi.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CatalogoGalacticoWebApi.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("202611080001_ConvertirSimulacionEnHistorial")]
public class ConvertirSimulacionEnHistorial : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Simulaciones_EventoId", table: "Simulaciones");

        migrationBuilder.CreateIndex(name: "IX_Simulaciones_EventoId", table: "Simulaciones", column: "EventoId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Simulaciones_EventoId", table: "Simulaciones");

        migrationBuilder.CreateIndex(name: "IX_Simulaciones_EventoId", table: "Simulaciones", column: "EventoId", unique: true);
    }
}
