using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoGalacticoWebApi.Migrations
{
    /// <inheritdoc />
    public partial class ResultadoDeParticipacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Resultado",
                table: "Participaciones",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "pendiente")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql(
                "UPDATE `Participaciones` SET `Resultado` = " +
                "CASE WHEN `Fallecido` = 1 THEN 'fallecido' ELSE 'sobreviviente' END;");

            migrationBuilder.DropColumn(
                name: "Fallecido",
                table: "Participaciones");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Fallecido",
                table: "Participaciones",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                "UPDATE `Participaciones` SET `Fallecido` = " +
                "CASE WHEN `Resultado` = 'fallecido' THEN 1 ELSE 0 END;");

            migrationBuilder.DropColumn(
                name: "Resultado",
                table: "Participaciones");
        }
    }
}
