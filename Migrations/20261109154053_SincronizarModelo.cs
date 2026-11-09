using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogoGalacticoWebApi.Migrations
{
    /// <inheritdoc />
    public partial class SincronizarModelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La base ya fue creada con AUTO_INCREMENT. Esta migración sincroniza
            // únicamente el snapshot que antes no incluía esa metadata de MySQL.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // No hay cambios físicos que revertir.
        }
    }
}
