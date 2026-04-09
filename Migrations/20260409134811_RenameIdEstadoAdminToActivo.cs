using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inmobiliaria_api.Migrations
{
    /// <inheritdoc />
    public partial class RenameIdEstadoAdminToActivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_propiedades_estados_propiedad_actidad_id_estado_admin",
                table: "propiedades");

            migrationBuilder.DropTable(
                name: "estados_propiedad_actidad");

            migrationBuilder.DropIndex(
                name: "IX_propiedades_id_estado_admin",
                table: "propiedades");

            migrationBuilder.DropColumn(
                name: "id_estado_admin",
                table: "propiedades");

            migrationBuilder.AddColumn<bool>(
                name: "activo",
                table: "propiedades",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "activo",
                table: "propiedades");

            migrationBuilder.AddColumn<int>(
                name: "id_estado_admin",
                table: "propiedades",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "estados_propiedad_actidad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Visible = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_propiedad_actidad", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_propiedades_id_estado_admin",
                table: "propiedades",
                column: "id_estado_admin");

            migrationBuilder.AddForeignKey(
                name: "FK_propiedades_estados_propiedad_actidad_id_estado_admin",
                table: "propiedades",
                column: "id_estado_admin",
                principalTable: "estados_propiedad_actidad",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
