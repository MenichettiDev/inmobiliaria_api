using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inmobiliaria_api.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuariosWebAndFavoritos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Crear tabla usuarios_web
            migrationBuilder.CreateTable(
                name: "usuarios_web",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hash_contrasena = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    google_id = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    proveedor_auth = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false, defaultValue: "local")
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    email_verificado = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_web", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Crear tabla propiedades_favoritas
            migrationBuilder.CreateTable(
                name: "propiedades_favoritas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_usuario_web = table.Column<int>(type: "int", nullable: false),
                    id_propiedad = table.Column<int>(type: "int", nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_propiedades_favoritas", x => x.id);
                    table.ForeignKey(
                        name: "FK_propiedades_favoritas_usuarios_web_id_usuario_web",
                        column: x => x.id_usuario_web,
                        principalTable: "usuarios_web",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_propiedades_favoritas_propiedades_id_propiedad",
                        column: x => x.id_propiedad,
                        principalTable: "propiedades",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Agregar columna id_usuario_web a la tabla leads
            migrationBuilder.AddColumn<int>(
                name: "id_usuario_web",
                table: "leads",
                type: "int",
                nullable: true);

            // Crear índice único en usuarios_web.email
            migrationBuilder.CreateIndex(
                name: "IX_usuarios_web_email",
                table: "usuarios_web",
                column: "email",
                unique: true);

            // Crear índice único en usuarios_web.google_id
            migrationBuilder.CreateIndex(
                name: "IX_usuarios_web_google_id",
                table: "usuarios_web",
                column: "google_id",
                unique: true);

            // Crear índice único en propiedades_favoritas
            migrationBuilder.CreateIndex(
                name: "IX_propiedades_favoritas_id_usuario_web_id_propiedad",
                table: "propiedades_favoritas",
                columns: new[] { "id_usuario_web", "id_propiedad" },
                unique: true);

            // Crear índice en propiedades_favoritas por id_propiedad
            migrationBuilder.CreateIndex(
                name: "IX_propiedades_favoritas_id_propiedad",
                table: "propiedades_favoritas",
                column: "id_propiedad");

            // Crear FK en leads hacia usuarios_web
            migrationBuilder.CreateIndex(
                name: "IX_leads_id_usuario_web",
                table: "leads",
                column: "id_usuario_web");

            migrationBuilder.AddForeignKey(
                name: "FK_leads_usuarios_web_id_usuario_web",
                table: "leads",
                column: "id_usuario_web",
                principalTable: "usuarios_web",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_leads_usuarios_web_id_usuario_web",
                table: "leads");

            migrationBuilder.DropTable(
                name: "propiedades_favoritas");

            migrationBuilder.DropTable(
                name: "usuarios_web");

            migrationBuilder.DropIndex(
                name: "IX_leads_id_usuario_web",
                table: "leads");

            migrationBuilder.DropColumn(
                name: "id_usuario_web",
                table: "leads");
        }
    }
}
