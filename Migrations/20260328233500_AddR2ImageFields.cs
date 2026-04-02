using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inmobiliaria_api.Migrations
{
    /// <inheritdoc />
    public partial class AddR2ImageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "es_principal",
                table: "imagenes_propiedades",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "r2_key",
                table: "imagenes_propiedades",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "max_imagenes_propiedad",
                table: "planes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_imagenes_propiedades_id_propiedad_es_principal",
                table: "imagenes_propiedades",
                columns: new[] { "id_propiedad", "es_principal" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_imagenes_propiedades_id_propiedad_es_principal",
                table: "imagenes_propiedades");

            migrationBuilder.DropColumn(
                name: "es_principal",
                table: "imagenes_propiedades");

            migrationBuilder.DropColumn(
                name: "r2_key",
                table: "imagenes_propiedades");

            migrationBuilder.DropColumn(
                name: "max_imagenes_propiedad",
                table: "planes");
        }
    }
}
