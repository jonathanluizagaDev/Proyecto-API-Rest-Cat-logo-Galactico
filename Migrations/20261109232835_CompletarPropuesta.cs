using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoGalacticoWebApi.Migrations
{
    /// <inheritdoc />
    public partial class CompletarPropuesta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantidadDeBajas",
                table: "Simulaciones",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "FallecidosJson",
                table: "Simulaciones",
                type: "longtext",
                nullable: false,
                defaultValue: "[]")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SobrevivientesJson",
                table: "Simulaciones",
                type: "longtext",
                nullable: false,
                defaultValue: "[]")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Eventos",
                type: "varchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "pendiente")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(
                "UPDATE `Eventos` SET `Estado` = 'simulado' " +
                "WHERE EXISTS (SELECT 1 FROM `Simulaciones` WHERE `Simulaciones`.`EventoId` = `Eventos`.`Id`);");

            migrationBuilder.Sql(
                "UPDATE `Simulaciones` AS s SET " +
                "s.`FallecidosJson` = COALESCE((SELECT JSON_ARRAYAGG(p.`PersonajeId`) FROM `Participaciones` AS p " +
                "WHERE p.`EventoId` = s.`EventoId` AND p.`Fallecido` = 1), JSON_ARRAY()), " +
                "s.`SobrevivientesJson` = COALESCE((SELECT JSON_ARRAYAGG(p.`PersonajeId`) FROM `Participaciones` AS p " +
                "WHERE p.`EventoId` = s.`EventoId` AND p.`Fallecido` = 0), JSON_ARRAY()), " +
                "s.`CantidadDeBajas` = (SELECT COUNT(*) FROM `Participaciones` AS p " +
                "WHERE p.`EventoId` = s.`EventoId` AND p.`Fallecido` = 1);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadDeBajas",
                table: "Simulaciones");

            migrationBuilder.DropColumn(
                name: "FallecidosJson",
                table: "Simulaciones");

            migrationBuilder.DropColumn(
                name: "SobrevivientesJson",
                table: "Simulaciones");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Eventos");
        }
    }
}
