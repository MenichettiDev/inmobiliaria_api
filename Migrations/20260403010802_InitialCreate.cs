using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inmobiliaria_api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "estados_inmobiliaria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_inmobiliaria", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "estados_lead",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_lead", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.CreateTable(
                name: "estados_propiedades_operativas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    color_hex = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_propiedades_operativas", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "estados_suscripcion",
                columns: table => new
                {
                    Id = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Codigo = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    permite_operar = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_suscripcion", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "estados_usuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estados_usuario", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fuentes_contacto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fuentes_contacto", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "planes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    precio_usd = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    max_propiedades = table.Column<int>(type: "int", nullable: true),
                    max_usuarios = table.Column<int>(type: "int", nullable: true),
                    max_leads_mes = table.Column<int>(type: "int", nullable: true),
                    white_label = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    dominio_personalizado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Automatizaciones = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    api_acceso = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Soporte = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    historial_estados = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    actividades_lead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    tipos_actividad = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    automatizacion_leads = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    lead_scoring = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    max_imagenes_propiedad = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_planes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "provincias",
                columns: table => new
                {
                    id_provincia = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    codigo_indec = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_provincias", x => x.id_provincia);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tipo_transaccion",
                columns: table => new
                {
                    id = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descripcion = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_transaccion", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuarios_web",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hash_contrasena = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    google_id = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    proveedor_auth = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    email_verificado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios_web", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "inmobiliarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subdominio = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dominio_personalizado = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_plan = table.Column<int>(type: "int", nullable: false),
                    id_estado = table.Column<int>(type: "int", nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    actualizado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    id_provincia = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inmobiliarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inmobiliarias_estados_inmobiliaria_id_estado",
                        column: x => x.id_estado,
                        principalTable: "estados_inmobiliaria",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inmobiliarias_planes_id_plan",
                        column: x => x.id_plan,
                        principalTable: "planes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inmobiliarias_provincias_id_provincia",
                        column: x => x.id_provincia,
                        principalTable: "provincias",
                        principalColumn: "id_provincia",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "localidades",
                columns: table => new
                {
                    id_localidad = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_provincia = table.Column<long>(type: "bigint", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    codigo_postal = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitud = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Longitud = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Municipio = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_partido = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_localidades", x => x.id_localidad);
                    table.ForeignKey(
                        name: "FK_localidades_provincias_id_provincia",
                        column: x => x.id_provincia,
                        principalTable: "provincias",
                        principalColumn: "id_provincia",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_inmobiliaria = table.Column<int>(type: "int", nullable: false),
                    nombre_completo = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dni = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefono = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    actualizado_en = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clientes_inmobiliarias_id_inmobiliaria",
                        column: x => x.id_inmobiliaria,
                        principalTable: "inmobiliarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "suscripciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_inmobiliaria = table.Column<int>(type: "int", nullable: false),
                    id_plan = table.Column<int>(type: "int", nullable: false),
                    inicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    fin = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    renovacion_automatica = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    id_estado = table.Column<byte>(type: "tinyint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suscripciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_suscripciones_estados_suscripcion_id_estado",
                        column: x => x.id_estado,
                        principalTable: "estados_suscripcion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_suscripciones_inmobiliarias_id_inmobiliaria",
                        column: x => x.id_inmobiliaria,
                        principalTable: "inmobiliarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_suscripciones_planes_id_plan",
                        column: x => x.id_plan,
                        principalTable: "planes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "uso_mensual",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_inmobiliaria = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    leads_generados = table.Column<int>(type: "int", nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uso_mensual", x => x.Id);
                    table.ForeignKey(
                        name: "FK_uso_mensual_inmobiliarias_id_inmobiliaria",
                        column: x => x.id_inmobiliaria,
                        principalTable: "inmobiliarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hash_contrasena = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_rol = table.Column<int>(type: "int", nullable: false),
                    id_inmobiliaria = table.Column<int>(type: "int", nullable: false),
                    id_estado = table.Column<int>(type: "int", nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usuarios_estados_usuario_id_estado",
                        column: x => x.id_estado,
                        principalTable: "estados_usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuarios_inmobiliarias_id_inmobiliaria",
                        column: x => x.id_inmobiliaria,
                        principalTable: "inmobiliarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_id_rol",
                        column: x => x.id_rol,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pagos_suscripcion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_inmobiliaria = table.Column<int>(type: "int", nullable: false),
                    id_plan = table.Column<int>(type: "int", nullable: false),
                    id_suscripcion = table.Column<int>(type: "int", nullable: true),
                    mp_preference_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mp_payment_id = table.Column<long>(type: "bigint", nullable: true),
                    estado = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    monto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    moneda = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dias_plan = table.Column<int>(type: "int", nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    actualizado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos_suscripcion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pagos_suscripcion_inmobiliarias_id_inmobiliaria",
                        column: x => x.id_inmobiliaria,
                        principalTable: "inmobiliarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pagos_suscripcion_planes_id_plan",
                        column: x => x.id_plan,
                        principalTable: "planes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pagos_suscripcion_suscripciones_id_suscripcion",
                        column: x => x.id_suscripcion,
                        principalTable: "suscripciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "propiedades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_inmobiliaria = table.Column<int>(type: "int", nullable: false),
                    id_agente_responsable = table.Column<int>(type: "int", nullable: true),
                    Titulo = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Precio = table.Column<decimal>(type: "decimal(15,2)", nullable: true),
                    Direccion = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Latitud = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    Longitud = table.Column<decimal>(type: "decimal(10,8)", nullable: true),
                    publicada_en = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    es_publicada = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    actualizado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    id_estado_admin = table.Column<int>(type: "int", nullable: false),
                    id_estado_operativo = table.Column<int>(type: "int", nullable: false),
                    id_localidad = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_propiedades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_propiedades_estados_propiedad_actidad_id_estado_admin",
                        column: x => x.id_estado_admin,
                        principalTable: "estados_propiedad_actidad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_propiedades_estados_propiedades_operativas_id_estado_operati~",
                        column: x => x.id_estado_operativo,
                        principalTable: "estados_propiedades_operativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_propiedades_inmobiliarias_id_inmobiliaria",
                        column: x => x.id_inmobiliaria,
                        principalTable: "inmobiliarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_propiedades_localidades_id_localidad",
                        column: x => x.id_localidad,
                        principalTable: "localidades",
                        principalColumn: "id_localidad",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_propiedades_usuarios_id_agente_responsable",
                        column: x => x.id_agente_responsable,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    id_inmobiliaria = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    expira_en = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    revocado_en = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    reemplazado_por = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip_origen = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "imagenes_propiedades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_propiedad = table.Column<int>(type: "int", nullable: false),
                    Url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    es_principal = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    r2_key = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_imagenes_propiedades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_imagenes_propiedades_propiedades_id_propiedad",
                        column: x => x.id_propiedad,
                        principalTable: "propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "leads",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_propiedad = table.Column<int>(type: "int", nullable: true),
                    id_inmobiliaria = table.Column<int>(type: "int", nullable: false),
                    id_usuario_asignado = table.Column<int>(type: "int", nullable: true),
                    id_cliente = table.Column<int>(type: "int", nullable: true),
                    id_usuario_web = table.Column<int>(type: "int", nullable: true),
                    nombre_completo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Mensaje = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_fuente = table.Column<int>(type: "int", nullable: false),
                    id_estado = table.Column<int>(type: "int", nullable: false),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    actualizado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_leads_clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_leads_estados_lead_id_estado",
                        column: x => x.id_estado,
                        principalTable: "estados_lead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_leads_fuentes_contacto_id_fuente",
                        column: x => x.id_fuente,
                        principalTable: "fuentes_contacto",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_leads_inmobiliarias_id_inmobiliaria",
                        column: x => x.id_inmobiliaria,
                        principalTable: "inmobiliarias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_leads_propiedades_id_propiedad",
                        column: x => x.id_propiedad,
                        principalTable: "propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_leads_usuarios_id_usuario_asignado",
                        column: x => x.id_usuario_asignado,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_leads_usuarios_web_id_usuario_web",
                        column: x => x.id_usuario_web,
                        principalTable: "usuarios_web",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "propiedades_favoritas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_usuario_web = table.Column<int>(type: "int", nullable: false),
                    id_propiedad = table.Column<int>(type: "int", nullable: false),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_propiedades_favoritas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_propiedades_favoritas_propiedades_id_propiedad",
                        column: x => x.id_propiedad,
                        principalTable: "propiedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_propiedades_favoritas_usuarios_web_id_usuario_web",
                        column: x => x.id_usuario_web,
                        principalTable: "usuarios_web",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transacciones_historial",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_cliente = table.Column<int>(type: "int", nullable: true),
                    id_inmobiliaria = table.Column<int>(type: "int", nullable: true),
                    id_propiedad = table.Column<int>(type: "int", nullable: true),
                    id_agente = table.Column<int>(type: "int", nullable: true),
                    id_tipo_transaccion = table.Column<byte>(type: "tinyint unsigned", nullable: true),
                    precio = table.Column<decimal>(type: "decimal(12,2)", nullable: true),
                    fecha_operacion = table.Column<DateTime>(type: "date", nullable: true),
                    observaciones = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transacciones_historial", x => x.id);
                    table.ForeignKey(
                        name: "FK_transacciones_historial_clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "clientes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_transacciones_historial_inmobiliarias_id_inmobiliaria",
                        column: x => x.id_inmobiliaria,
                        principalTable: "inmobiliarias",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_transacciones_historial_propiedades_id_propiedad",
                        column: x => x.id_propiedad,
                        principalTable: "propiedades",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_transacciones_historial_tipo_transaccion_id_tipo_transaccion",
                        column: x => x.id_tipo_transaccion,
                        principalTable: "tipo_transaccion",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_transacciones_historial_usuarios_id_agente",
                        column: x => x.id_agente,
                        principalTable: "usuarios",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "lead_estado_historial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_lead = table.Column<int>(type: "int", nullable: false),
                    id_estado_anterior = table.Column<int>(type: "int", nullable: false),
                    id_estado = table.Column<int>(type: "int", nullable: false),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    Comentario = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    creado_en = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lead_estado_historial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_lead_estado_historial_estados_lead_id_estado",
                        column: x => x.id_estado,
                        principalTable: "estados_lead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lead_estado_historial_estados_lead_id_estado_anterior",
                        column: x => x.id_estado_anterior,
                        principalTable: "estados_lead",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_lead_estado_historial_leads_id_lead",
                        column: x => x.id_lead,
                        principalTable: "leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_lead_estado_historial_usuarios_id_usuario",
                        column: x => x.id_usuario,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_id_inmobiliaria",
                table: "clientes",
                column: "id_inmobiliaria");

            migrationBuilder.CreateIndex(
                name: "IX_estados_lead_Nombre",
                table: "estados_lead",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fuentes_contacto_Nombre",
                table: "fuentes_contacto",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_imagenes_propiedades_id_propiedad",
                table: "imagenes_propiedades",
                column: "id_propiedad");

            migrationBuilder.CreateIndex(
                name: "IX_inmobiliarias_id_estado",
                table: "inmobiliarias",
                column: "id_estado");

            migrationBuilder.CreateIndex(
                name: "IX_inmobiliarias_id_plan",
                table: "inmobiliarias",
                column: "id_plan");

            migrationBuilder.CreateIndex(
                name: "IX_inmobiliarias_id_provincia",
                table: "inmobiliarias",
                column: "id_provincia");

            migrationBuilder.CreateIndex(
                name: "IX_inmobiliarias_Subdominio",
                table: "inmobiliarias",
                column: "Subdominio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lead_estado_historial_id_estado",
                table: "lead_estado_historial",
                column: "id_estado");

            migrationBuilder.CreateIndex(
                name: "IX_lead_estado_historial_id_estado_anterior",
                table: "lead_estado_historial",
                column: "id_estado_anterior");

            migrationBuilder.CreateIndex(
                name: "IX_lead_estado_historial_id_lead",
                table: "lead_estado_historial",
                column: "id_lead");

            migrationBuilder.CreateIndex(
                name: "IX_lead_estado_historial_id_usuario",
                table: "lead_estado_historial",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_leads_id_cliente",
                table: "leads",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_leads_id_estado",
                table: "leads",
                column: "id_estado");

            migrationBuilder.CreateIndex(
                name: "IX_leads_id_fuente",
                table: "leads",
                column: "id_fuente");

            migrationBuilder.CreateIndex(
                name: "IX_leads_id_inmobiliaria",
                table: "leads",
                column: "id_inmobiliaria");

            migrationBuilder.CreateIndex(
                name: "IX_leads_id_propiedad",
                table: "leads",
                column: "id_propiedad");

            migrationBuilder.CreateIndex(
                name: "IX_leads_id_usuario_asignado",
                table: "leads",
                column: "id_usuario_asignado");

            migrationBuilder.CreateIndex(
                name: "IX_leads_id_usuario_web",
                table: "leads",
                column: "id_usuario_web");

            migrationBuilder.CreateIndex(
                name: "IX_localidades_id_provincia",
                table: "localidades",
                column: "id_provincia");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_suscripcion_id_inmobiliaria_estado",
                table: "pagos_suscripcion",
                columns: new[] { "id_inmobiliaria", "estado" });

            migrationBuilder.CreateIndex(
                name: "IX_pagos_suscripcion_id_plan",
                table: "pagos_suscripcion",
                column: "id_plan");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_suscripcion_id_suscripcion",
                table: "pagos_suscripcion",
                column: "id_suscripcion");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_suscripcion_mp_preference_id",
                table: "pagos_suscripcion",
                column: "mp_preference_id");

            migrationBuilder.CreateIndex(
                name: "IX_propiedades_id_agente_responsable",
                table: "propiedades",
                column: "id_agente_responsable");

            migrationBuilder.CreateIndex(
                name: "IX_propiedades_id_estado_admin",
                table: "propiedades",
                column: "id_estado_admin");

            migrationBuilder.CreateIndex(
                name: "IX_propiedades_id_estado_operativo",
                table: "propiedades",
                column: "id_estado_operativo");

            migrationBuilder.CreateIndex(
                name: "IX_propiedades_id_inmobiliaria",
                table: "propiedades",
                column: "id_inmobiliaria");

            migrationBuilder.CreateIndex(
                name: "IX_propiedades_id_localidad",
                table: "propiedades",
                column: "id_localidad");

            migrationBuilder.CreateIndex(
                name: "IX_propiedades_favoritas_id_propiedad",
                table: "propiedades_favoritas",
                column: "id_propiedad");

            migrationBuilder.CreateIndex(
                name: "IX_propiedades_favoritas_id_usuario_web",
                table: "propiedades_favoritas",
                column: "id_usuario_web");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_id_usuario_revocado_en",
                table: "refresh_tokens",
                columns: new[] { "id_usuario", "revocado_en" });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Token",
                table: "refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_suscripciones_id_estado",
                table: "suscripciones",
                column: "id_estado");

            migrationBuilder.CreateIndex(
                name: "IX_suscripciones_id_inmobiliaria",
                table: "suscripciones",
                column: "id_inmobiliaria");

            migrationBuilder.CreateIndex(
                name: "IX_suscripciones_id_plan",
                table: "suscripciones",
                column: "id_plan");

            migrationBuilder.CreateIndex(
                name: "IX_transacciones_historial_id_agente",
                table: "transacciones_historial",
                column: "id_agente");

            migrationBuilder.CreateIndex(
                name: "IX_transacciones_historial_id_cliente",
                table: "transacciones_historial",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_transacciones_historial_id_inmobiliaria",
                table: "transacciones_historial",
                column: "id_inmobiliaria");

            migrationBuilder.CreateIndex(
                name: "IX_transacciones_historial_id_propiedad",
                table: "transacciones_historial",
                column: "id_propiedad");

            migrationBuilder.CreateIndex(
                name: "IX_transacciones_historial_id_tipo_transaccion",
                table: "transacciones_historial",
                column: "id_tipo_transaccion");

            migrationBuilder.CreateIndex(
                name: "IX_uso_mensual_id_inmobiliaria",
                table: "uso_mensual",
                column: "id_inmobiliaria");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_estado",
                table: "usuarios",
                column: "id_estado");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_inmobiliaria",
                table: "usuarios",
                column: "id_inmobiliaria");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_rol",
                table: "usuarios",
                column: "id_rol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "imagenes_propiedades");

            migrationBuilder.DropTable(
                name: "lead_estado_historial");

            migrationBuilder.DropTable(
                name: "pagos_suscripcion");

            migrationBuilder.DropTable(
                name: "propiedades_favoritas");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "transacciones_historial");

            migrationBuilder.DropTable(
                name: "uso_mensual");

            migrationBuilder.DropTable(
                name: "leads");

            migrationBuilder.DropTable(
                name: "suscripciones");

            migrationBuilder.DropTable(
                name: "tipo_transaccion");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "estados_lead");

            migrationBuilder.DropTable(
                name: "fuentes_contacto");

            migrationBuilder.DropTable(
                name: "propiedades");

            migrationBuilder.DropTable(
                name: "usuarios_web");

            migrationBuilder.DropTable(
                name: "estados_suscripcion");

            migrationBuilder.DropTable(
                name: "estados_propiedad_actidad");

            migrationBuilder.DropTable(
                name: "estados_propiedades_operativas");

            migrationBuilder.DropTable(
                name: "localidades");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "estados_usuario");

            migrationBuilder.DropTable(
                name: "inmobiliarias");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "estados_inmobiliaria");

            migrationBuilder.DropTable(
                name: "planes");

            migrationBuilder.DropTable(
                name: "provincias");
        }
    }
}
