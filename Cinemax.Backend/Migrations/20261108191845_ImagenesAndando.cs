using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cinemax.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ImagenesAndando : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Imagenes_Productos_ProductoId",
                table: "Imagenes");

            migrationBuilder.DropTable(
                name: "DetallesIngresos");

            migrationBuilder.DropTable(
                name: "DetallesVentas");

            migrationBuilder.DropTable(
                name: "Ingresos");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropIndex(
                name: "IX_Pagos_VentaId",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Imagenes_ProductoId",
                table: "Imagenes");

            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Snacks");

            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Personas");

            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Peliculas");

            migrationBuilder.DropColumn(
                name: "PosterUrl",
                table: "Peliculas");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "Imagenes");

            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Combos");

            migrationBuilder.AddColumn<int>(
                name: "FuncionId",
                table: "Ventas",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Pagos",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "ComboId",
                table: "Imagenes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PeliculaId",
                table: "Imagenes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PersonaId",
                table: "Imagenes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SnackId",
                table: "Imagenes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoImagen",
                table: "Imagenes",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ConsumosStockVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    SnackId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumosStockVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumosStockVenta_Snacks_SnackId",
                        column: x => x.SnackId,
                        principalTable: "Snacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsumosStockVenta_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesVentaSnack",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    SnackId = table.Column<int>(type: "integer", nullable: true),
                    ComboId = table.Column<int>(type: "integer", nullable: true),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesVentaSnack", x => x.Id);
                    table.CheckConstraint("CK_DetalleVentaSnack_SnackOCombo", "(\"SnackId\" IS NOT NULL AND \"ComboId\" IS NULL) OR (\"SnackId\" IS NULL AND \"ComboId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_DetallesVentaSnack_Combos_ComboId",
                        column: x => x.ComboId,
                        principalTable: "Combos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesVentaSnack_Snacks_SnackId",
                        column: x => x.SnackId,
                        principalTable: "Snacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesVentaSnack_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EntradasVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    FuncionId = table.Column<int>(type: "integer", nullable: false),
                    ButacaId = table.Column<int>(type: "integer", nullable: false),
                    Precio = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Activa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntradasVenta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntradasVenta_Butacas_ButacaId",
                        column: x => x.ButacaId,
                        principalTable: "Butacas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntradasVenta_Funciones_FuncionId",
                        column: x => x.FuncionId,
                        principalTable: "Funciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EntradasVenta_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Estado_FechaExpiracion",
                table: "Ventas",
                columns: new[] { "Estado", "FechaExpiracion" });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_FuncionId",
                table: "Ventas",
                column: "FuncionId");

            migrationBuilder.CreateIndex(
                name: "UX_Pago_Venta_Pendiente",
                table: "Pagos",
                column: "VentaId",
                unique: true,
                filter: "\"Estado\" = 'Pendiente'");

            migrationBuilder.CreateIndex(
                name: "IX_Imagenes_ComboId",
                table: "Imagenes",
                column: "ComboId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Imagenes_PeliculaId_TipoImagen",
                table: "Imagenes",
                columns: new[] { "PeliculaId", "TipoImagen" },
                unique: true,
                filter: "\"PeliculaId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Imagenes_PersonaId",
                table: "Imagenes",
                column: "PersonaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Imagenes_SnackId",
                table: "Imagenes",
                column: "SnackId",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Imagen_UnaSolaEntidad",
                table: "Imagenes",
                sql: "(\r\n    (\r\n        \"SnackId\" IS NOT NULL\r\n        AND \"ComboId\" IS NULL\r\n        AND \"PersonaId\" IS NULL\r\n        AND \"PeliculaId\" IS NULL\r\n        AND \"TipoImagen\" = 'Unica'\r\n    )\r\n    OR\r\n    (\r\n        \"SnackId\" IS NULL\r\n        AND \"ComboId\" IS NOT NULL\r\n        AND \"PersonaId\" IS NULL\r\n        AND \"PeliculaId\" IS NULL\r\n        AND \"TipoImagen\" = 'Unica'\r\n    )\r\n    OR\r\n    (\r\n        \"SnackId\" IS NULL\r\n        AND \"ComboId\" IS NULL\r\n        AND \"PersonaId\" IS NOT NULL\r\n        AND \"PeliculaId\" IS NULL\r\n        AND \"TipoImagen\" = 'Unica'\r\n    )\r\n    OR\r\n    (\r\n        \"SnackId\" IS NULL\r\n        AND \"ComboId\" IS NULL\r\n        AND \"PersonaId\" IS NULL\r\n        AND \"PeliculaId\" IS NOT NULL\r\n        AND \"TipoImagen\" IN ('Principal', 'Poster')\r\n    )\r\n)");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumosStockVenta_SnackId",
                table: "ConsumosStockVenta",
                column: "SnackId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumosStockVenta_VentaId_SnackId",
                table: "ConsumosStockVenta",
                columns: new[] { "VentaId", "SnackId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVentaSnack_ComboId",
                table: "DetallesVentaSnack",
                column: "ComboId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVentaSnack_SnackId",
                table: "DetallesVentaSnack",
                column: "SnackId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVentaSnack_VentaId",
                table: "DetallesVentaSnack",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasVenta_ButacaId",
                table: "EntradasVenta",
                column: "ButacaId");

            migrationBuilder.CreateIndex(
                name: "IX_EntradasVenta_VentaId_ButacaId",
                table: "EntradasVenta",
                columns: new[] { "VentaId", "ButacaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_EntradaVenta_Funcion_Butaca_Activa",
                table: "EntradasVenta",
                columns: new[] { "FuncionId", "ButacaId" },
                unique: true,
                filter: "\"Activa\" = true");

            migrationBuilder.AddForeignKey(
                name: "FK_Imagenes_Combos_ComboId",
                table: "Imagenes",
                column: "ComboId",
                principalTable: "Combos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Imagenes_Peliculas_PeliculaId",
                table: "Imagenes",
                column: "PeliculaId",
                principalTable: "Peliculas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Imagenes_Personas_PersonaId",
                table: "Imagenes",
                column: "PersonaId",
                principalTable: "Personas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Imagenes_Snacks_SnackId",
                table: "Imagenes",
                column: "SnackId",
                principalTable: "Snacks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Ventas_Funciones_FuncionId",
                table: "Ventas",
                column: "FuncionId",
                principalTable: "Funciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Imagenes_Combos_ComboId",
                table: "Imagenes");

            migrationBuilder.DropForeignKey(
                name: "FK_Imagenes_Peliculas_PeliculaId",
                table: "Imagenes");

            migrationBuilder.DropForeignKey(
                name: "FK_Imagenes_Personas_PersonaId",
                table: "Imagenes");

            migrationBuilder.DropForeignKey(
                name: "FK_Imagenes_Snacks_SnackId",
                table: "Imagenes");

            migrationBuilder.DropForeignKey(
                name: "FK_Ventas_Funciones_FuncionId",
                table: "Ventas");

            migrationBuilder.DropTable(
                name: "ConsumosStockVenta");

            migrationBuilder.DropTable(
                name: "DetallesVentaSnack");

            migrationBuilder.DropTable(
                name: "EntradasVenta");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_Estado_FechaExpiracion",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "IX_Ventas_FuncionId",
                table: "Ventas");

            migrationBuilder.DropIndex(
                name: "UX_Pago_Venta_Pendiente",
                table: "Pagos");

            migrationBuilder.DropIndex(
                name: "IX_Imagenes_ComboId",
                table: "Imagenes");

            migrationBuilder.DropIndex(
                name: "IX_Imagenes_PeliculaId_TipoImagen",
                table: "Imagenes");

            migrationBuilder.DropIndex(
                name: "IX_Imagenes_PersonaId",
                table: "Imagenes");

            migrationBuilder.DropIndex(
                name: "IX_Imagenes_SnackId",
                table: "Imagenes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Imagen_UnaSolaEntidad",
                table: "Imagenes");

            migrationBuilder.DropColumn(
                name: "FuncionId",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "ComboId",
                table: "Imagenes");

            migrationBuilder.DropColumn(
                name: "PeliculaId",
                table: "Imagenes");

            migrationBuilder.DropColumn(
                name: "PersonaId",
                table: "Imagenes");

            migrationBuilder.DropColumn(
                name: "SnackId",
                table: "Imagenes");

            migrationBuilder.DropColumn(
                name: "TipoImagen",
                table: "Imagenes");

            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Snacks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Personas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Peliculas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PosterUrl",
                table: "Peliculas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Pagos",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "Imagenes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Combos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Descripcion = table.Column<string>(type: "character varying(600)", maxLength: 600, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CUIT = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Eliminado = table.Column<bool>(type: "boolean", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    RazonSocial = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoriaId = table.Column<int>(type: "integer", nullable: false),
                    Eliminado = table.Column<bool>(type: "boolean", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    PrecioCompra = table.Column<decimal>(type: "numeric", nullable: false),
                    PrecioVenta = table.Column<decimal>(type: "numeric", nullable: false),
                    Stock = table.Column<int>(type: "integer", nullable: false),
                    StockMinimo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ingresos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProveedorId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Anulado = table.Column<bool>(type: "boolean", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Total = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingresos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingresos_Proveedores_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ingresos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesVentas",
                columns: table => new
                {
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    VentaId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesVentas", x => new { x.ProductoId, x.VentaId });
                    table.ForeignKey(
                        name: "FK_DetallesVentas_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesVentas_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetallesIngresos",
                columns: table => new
                {
                    ProductoId = table.Column<int>(type: "integer", nullable: false),
                    IngresoId = table.Column<int>(type: "integer", nullable: false),
                    Cantidad = table.Column<int>(type: "integer", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesIngresos", x => new { x.ProductoId, x.IngresoId });
                    table.ForeignKey(
                        name: "FK_DetallesIngresos_Ingresos_IngresoId",
                        column: x => x.IngresoId,
                        principalTable: "Ingresos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesIngresos_Productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_VentaId",
                table: "Pagos",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Imagenes_ProductoId",
                table: "Imagenes",
                column: "ProductoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetallesIngresos_IngresoId",
                table: "DetallesIngresos",
                column: "IngresoId");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesVentas_VentaId",
                table: "DetallesVentas",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_ProveedorId",
                table: "Ingresos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingresos_UsuarioId",
                table: "Ingresos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_CategoriaId",
                table: "Productos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_CUIT",
                table: "Proveedores",
                column: "CUIT",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proveedores_Email",
                table: "Proveedores",
                column: "Email",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Imagenes_Productos_ProductoId",
                table: "Imagenes",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
