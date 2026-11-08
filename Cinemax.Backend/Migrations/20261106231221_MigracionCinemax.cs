using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cinemax.Backend.Migrations
{
    /// <inheritdoc />
    public partial class MigracionCinemax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Combos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    ImagenUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Eliminado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Combos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Generos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Eliminado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Generos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Peliculas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Sinopsis = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Clasificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaEstreno = table.Column<DateOnly>(type: "date", nullable: false),
                    DuracionMinutos = table.Column<int>(type: "integer", nullable: false),
                    ImagenUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PosterUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EnCartelera = table.Column<bool>(type: "boolean", nullable: false),
                    Eliminado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Peliculas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ImagenUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Eliminado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Salas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Eliminado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Salas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Snacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    ImagenUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Eliminado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Snacks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GenerosPeliculas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GeneroId = table.Column<int>(type: "integer", nullable: false),
                    PeliculaId = table.Column<int>(type: "integer", nullable: false),
                    GeneroId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerosPeliculas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GenerosPeliculas_Generos_GeneroId",
                        column: x => x.GeneroId,
                        principalTable: "Generos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GenerosPeliculas_Generos_GeneroId1",
                        column: x => x.GeneroId1,
                        principalTable: "Generos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_GenerosPeliculas_Peliculas_PeliculaId",
                        column: x => x.PeliculaId,
                        principalTable: "Peliculas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonasPeliculas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PersonaId = table.Column<int>(type: "integer", nullable: false),
                    PeliculaId = table.Column<int>(type: "integer", nullable: false),
                    Rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonasPeliculas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonasPeliculas_Peliculas_PeliculaId",
                        column: x => x.PeliculaId,
                        principalTable: "Peliculas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PersonasPeliculas_Personas_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "Personas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Butacas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalaId = table.Column<int>(type: "integer", nullable: false),
                    Fila = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Columna = table.Column<int>(type: "integer", nullable: false),
                    Accesible = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Butacas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Butacas_Salas_SalaId",
                        column: x => x.SalaId,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Funciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SalaId = table.Column<int>(type: "integer", nullable: false),
                    PeliculaId = table.Column<int>(type: "integer", nullable: false),
                    Formato = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Idioma = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FechaHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PrecioEntrada = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Eliminado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Funciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Funciones_Peliculas_PeliculaId",
                        column: x => x.PeliculaId,
                        principalTable: "Peliculas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Funciones_Salas_SalaId",
                        column: x => x.SalaId,
                        principalTable: "Salas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComboSnacks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ComboId = table.Column<int>(type: "integer", nullable: false),
                    SnackId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboSnacks", x => x.Id);
                    table.CheckConstraint("CK_ComboSnack_Cantidad", "\"Cantidad\" > 0");
                    table.ForeignKey(
                        name: "FK_ComboSnacks_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ComboSnacks_Snacks_SnackId",
                        column: x => x.SnackId,
                        principalTable: "Snacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Inventarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SnackId = table.Column<int>(type: "integer", nullable: false),
                    DescripcionUnidad = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CantidadDisponible = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventarios", x => x.Id);
                    table.CheckConstraint("CK_Inventario_Cantidad", "\"CantidadDisponible\" >= 0");
                    table.ForeignKey(
                        name: "FK_Inventarios_Snacks_SnackId",
                        column: x => x.SnackId,
                        principalTable: "Snacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Butacas_SalaId_Fila_Columna",
                table: "Butacas",
                columns: new[] { "SalaId", "Fila", "Columna" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Butacas_SalaId_Fila_Numero",
                table: "Butacas",
                columns: new[] { "SalaId", "Fila", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Combos_Nombre",
                table: "Combos",
                column: "Nombre",
                unique: true,
                filter: "\"Eliminado\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ComboSnacks_ComboId_SnackId",
                table: "ComboSnacks",
                columns: new[] { "ComboId", "SnackId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComboSnacks_SnackId",
                table: "ComboSnacks",
                column: "SnackId");

            migrationBuilder.CreateIndex(
                name: "IX_Funciones_PeliculaId_FechaHora",
                table: "Funciones",
                columns: new[] { "PeliculaId", "FechaHora" });

            migrationBuilder.CreateIndex(
                name: "IX_Funciones_SalaId_FechaHora",
                table: "Funciones",
                columns: new[] { "SalaId", "FechaHora" });

            migrationBuilder.CreateIndex(
                name: "IX_Generos_Nombre",
                table: "Generos",
                column: "Nombre",
                unique: true,
                filter: "\"Eliminado\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_GenerosPeliculas_GeneroId",
                table: "GenerosPeliculas",
                column: "GeneroId");

            migrationBuilder.CreateIndex(
                name: "IX_GenerosPeliculas_GeneroId1",
                table: "GenerosPeliculas",
                column: "GeneroId1");

            migrationBuilder.CreateIndex(
                name: "IX_GenerosPeliculas_PeliculaId_GeneroId",
                table: "GenerosPeliculas",
                columns: new[] { "PeliculaId", "GeneroId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_SnackId",
                table: "Inventarios",
                column: "SnackId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Peliculas_Titulo_FechaEstreno",
                table: "Peliculas",
                columns: new[] { "Titulo", "FechaEstreno" },
                unique: true,
                filter: "\"Eliminado\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Personas_Nombre",
                table: "Personas",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "IX_PersonasPeliculas_PeliculaId_PersonaId_Rol",
                table: "PersonasPeliculas",
                columns: new[] { "PeliculaId", "PersonaId", "Rol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonasPeliculas_PersonaId",
                table: "PersonasPeliculas",
                column: "PersonaId");

            migrationBuilder.CreateIndex(
                name: "IX_Salas_Numero",
                table: "Salas",
                column: "Numero",
                unique: true,
                filter: "\"Eliminado\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Snacks_Nombre",
                table: "Snacks",
                column: "Nombre",
                unique: true,
                filter: "\"Eliminado\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Butacas");

            migrationBuilder.DropTable(
                name: "ComboSnacks");

            migrationBuilder.DropTable(
                name: "Funciones");

            migrationBuilder.DropTable(
                name: "GenerosPeliculas");

            migrationBuilder.DropTable(
                name: "Inventarios");

            migrationBuilder.DropTable(
                name: "PersonasPeliculas");

            migrationBuilder.DropTable(
                name: "Combos");

            migrationBuilder.DropTable(
                name: "Salas");

            migrationBuilder.DropTable(
                name: "Generos");

            migrationBuilder.DropTable(
                name: "Snacks");

            migrationBuilder.DropTable(
                name: "Peliculas");

            migrationBuilder.DropTable(
                name: "Personas");
        }
    }
}
