using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inmobiliaria_api.Migrations
{
    /// <inheritdoc />
    public partial class AddThumbnailToImagenPropiedad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "thumbnail_key",
                table: "imagenes_propiedades",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "thumbnail_url",
                table: "imagenes_propiedades",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "thumbnail_key",
                table: "imagenes_propiedades");

            migrationBuilder.DropColumn(
                name: "thumbnail_url",
                table: "imagenes_propiedades");
        }
    }
}
