using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TDFSantaLucia.Migrations
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
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nombre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Primer_Apellido = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Segundo_Apellido = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Direccion_Exacta = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cedula = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Correo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedUserName = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NormalizedEmail = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SecurityStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ConcurrencyStamp = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumber = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhoneNumberConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetime(6)", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Categoria_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Categoria_Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimType = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClaimValue = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderKey = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProviderDisplayName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LoginProvider = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Cliente_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Fecha_Nacimiento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Puntos_Acumulados = table.Column<int>(type: "int", nullable: false),
                    Usuario_ID = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Cliente_Id);
                    table.ForeignKey(
                        name: "FK_Clientes_AspNetUsers_Usuario_ID",
                        column: x => x.Usuario_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Cupones",
                columns: table => new
                {
                    Cupon_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descripcion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo_Descuento = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Valor_Descuento = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Fecha_Expiracion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Estado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Usuario_Id = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cupones", x => x.Cupon_Id);
                    table.ForeignKey(
                        name: "FK_Cupones_AspNetUsers_Usuario_Id",
                        column: x => x.Usuario_Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Empleados",
                columns: table => new
                {
                    Empleado_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Cedula = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Puesto = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Direccion_Exacta = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SalarioNeto = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    SalarioBruto = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Usuario_ID = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empleados", x => x.Empleado_Id);
                    table.ForeignKey(
                        name: "FK_Empleados_AspNetUsers_Usuario_ID",
                        column: x => x.Usuario_ID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Producto_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Categoria_Id = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Descripcion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Marca = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Imagen_URL = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Receta = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Producto_Id);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_Categoria_Id",
                        column: x => x.Categoria_Id,
                        principalTable: "Categorias",
                        principalColumn: "Categoria_Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CarritoItems",
                columns: table => new
                {
                    CarritoItem_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Cliente_Id = table.Column<int>(type: "int", nullable: false),
                    Producto_Id = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Imagen_URL = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Marca = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Precio = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Receta = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Fecha_Agregado = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarritoItems", x => x.CarritoItem_Id);
                    table.ForeignKey(
                        name: "FK_CarritoItems_Clientes_Cliente_Id",
                        column: x => x.Cliente_Id,
                        principalTable: "Clientes",
                        principalColumn: "Cliente_Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tratamientos",
                columns: table => new
                {
                    Tratamiento_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre_Medicamento = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Dosis = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Duracion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fecha_Inicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Fecha_Fin = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Estado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Cliente_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tratamientos", x => x.Tratamiento_Id);
                    table.ForeignKey(
                        name: "FK_Tratamientos_Clientes_Cliente_Id",
                        column: x => x.Cliente_Id,
                        principalTable: "Clientes",
                        principalColumn: "Cliente_Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClientesCupones",
                columns: table => new
                {
                    Cupon_Cliente_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Fecha_Asignacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Utilizado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Fecha_Uso = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Cliente_Id = table.Column<int>(type: "int", nullable: false),
                    Cupon_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesCupones", x => x.Cupon_Cliente_Id);
                    table.ForeignKey(
                        name: "FK_ClientesCupones_Clientes_Cliente_Id",
                        column: x => x.Cliente_Id,
                        principalTable: "Clientes",
                        principalColumn: "Cliente_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientesCupones_Cupones_Cupon_Id",
                        column: x => x.Cupon_Id,
                        principalTable: "Cupones",
                        principalColumn: "Cupon_Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    Pedido_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Numero_Orden = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Total = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Descripcion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tipo_Entrega = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Metodo_Pago = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Direccion_Entrega = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono_Contacto = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Fecha_Actualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Cliente_Id = table.Column<int>(type: "int", nullable: false),
                    Cupon_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Pedido_Id);
                    table.ForeignKey(
                        name: "FK_Pedidos_Clientes_Cliente_Id",
                        column: x => x.Cliente_Id,
                        principalTable: "Clientes",
                        principalColumn: "Cliente_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pedidos_Cupones_Cupon_Id",
                        column: x => x.Cupon_Id,
                        principalTable: "Cupones",
                        principalColumn: "Cupon_Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Citas",
                columns: table => new
                {
                    Cita_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Servicio = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Estado = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observaciones = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cliente_Id = table.Column<int>(type: "int", nullable: false),
                    Empleado_Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citas", x => x.Cita_Id);
                    table.ForeignKey(
                        name: "FK_Citas_Clientes_Cliente_Id",
                        column: x => x.Cliente_Id,
                        principalTable: "Clientes",
                        principalColumn: "Cliente_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Citas_Empleados_Empleado_Id",
                        column: x => x.Empleado_Id,
                        principalTable: "Empleados",
                        principalColumn: "Empleado_Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Expedientes",
                columns: table => new
                {
                    Expediente_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descripcion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fecha_Creacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Fecha_Actualizacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Cliente_Id = table.Column<int>(type: "int", nullable: false),
                    Empleado_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expedientes", x => x.Expediente_Id);
                    table.ForeignKey(
                        name: "FK_Expedientes_Clientes_Cliente_Id",
                        column: x => x.Cliente_Id,
                        principalTable: "Clientes",
                        principalColumn: "Cliente_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expedientes_Empleados_Empleado_Id",
                        column: x => x.Empleado_Id,
                        principalTable: "Empleados",
                        principalColumn: "Empleado_Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HorariosEmpleados",
                columns: table => new
                {
                    Horario_Empleado_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Hora_Ingreso = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Hora_Salida = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Inicio_Almuerzo = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Fin_Almuerzo = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Empleado_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosEmpleados", x => x.Horario_Empleado_Id);
                    table.ForeignKey(
                        name: "FK_HorariosEmpleados_Empleados_Empleado_Id",
                        column: x => x.Empleado_Id,
                        principalTable: "Empleados",
                        principalColumn: "Empleado_Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Inventarios",
                columns: table => new
                {
                    Inventario_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Cantidad_Disponible = table.Column<int>(type: "int", nullable: false),
                    Cantidad_Minima = table.Column<int>(type: "int", nullable: false),
                    Fecha_Vencimiento = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Fecha_Ingreso = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Numero_Lote = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Estado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Proveedor = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Producto_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventarios", x => x.Inventario_Id);
                    table.ForeignKey(
                        name: "FK_Inventarios_Productos_Producto_Id",
                        column: x => x.Producto_Id,
                        principalTable: "Productos",
                        principalColumn: "Producto_Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RecordatoriosTratamiento",
                columns: table => new
                {
                    Recordatorio_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Hora = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    Confirmacion = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Tratamiento_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordatoriosTratamiento", x => x.Recordatorio_Id);
                    table.ForeignKey(
                        name: "FK_RecordatoriosTratamiento_Tratamientos_Tratamiento_Id",
                        column: x => x.Tratamiento_Id,
                        principalTable: "Tratamientos",
                        principalColumn: "Tratamiento_Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DetallesPedido",
                columns: table => new
                {
                    Detalle_Pedido_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Precio_Unitario = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Pedido_Id = table.Column<int>(type: "int", nullable: false),
                    Producto_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesPedido", x => x.Detalle_Pedido_Id);
                    table.ForeignKey(
                        name: "FK_DetallesPedido_Pedidos_Pedido_Id",
                        column: x => x.Pedido_Id,
                        principalTable: "Pedidos",
                        principalColumn: "Pedido_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesPedido_Productos_Producto_Id",
                        column: x => x.Producto_Id,
                        principalTable: "Productos",
                        principalColumn: "Producto_Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    Factura_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Numero_Factura = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Subtotal = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Descuento = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Impuesto = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Estado = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fecha_Emision = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Cliente_Id = table.Column<int>(type: "int", nullable: false),
                    Pedido_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.Factura_Id);
                    table.ForeignKey(
                        name: "FK_Facturas_Clientes_Cliente_Id",
                        column: x => x.Cliente_Id,
                        principalTable: "Clientes",
                        principalColumn: "Cliente_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Facturas_Pedidos_Pedido_Id",
                        column: x => x.Pedido_Id,
                        principalTable: "Pedidos",
                        principalColumn: "Pedido_Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RecetasMedicas",
                columns: table => new
                {
                    Receta_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descripcion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Frecuencia = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Producto_Id = table.Column<int>(type: "int", nullable: false),
                    Expediente_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecetasMedicas", x => x.Receta_Id);
                    table.ForeignKey(
                        name: "FK_RecetasMedicas_Expedientes_Expediente_Id",
                        column: x => x.Expediente_Id,
                        principalTable: "Expedientes",
                        principalColumn: "Expediente_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecetasMedicas_Productos_Producto_Id",
                        column: x => x.Producto_Id,
                        principalTable: "Productos",
                        principalColumn: "Producto_Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DetallesFactura",
                columns: table => new
                {
                    Detalle_Factura_Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Precio_Unitario = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Factura_Id = table.Column<int>(type: "int", nullable: false),
                    Producto_Id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetallesFactura", x => x.Detalle_Factura_Id);
                    table.ForeignKey(
                        name: "FK_DetallesFactura_Facturas_Factura_Id",
                        column: x => x.Factura_Id,
                        principalTable: "Facturas",
                        principalColumn: "Factura_Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DetallesFactura_Productos_Producto_Id",
                        column: x => x.Producto_Id,
                        principalTable: "Productos",
                        principalColumn: "Producto_Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarritoItems_Cliente_Id",
                table: "CarritoItems",
                column: "Cliente_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_Cliente_Id",
                table: "Citas",
                column: "Cliente_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_Empleado_Id",
                table: "Citas",
                column: "Empleado_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Usuario_ID",
                table: "Clientes",
                column: "Usuario_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientesCupones_Cliente_Id",
                table: "ClientesCupones",
                column: "Cliente_Id");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesCupones_Cupon_Id",
                table: "ClientesCupones",
                column: "Cupon_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Cupones_Usuario_Id",
                table: "Cupones",
                column: "Usuario_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesFactura_Factura_Id",
                table: "DetallesFactura",
                column: "Factura_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesFactura_Producto_Id",
                table: "DetallesFactura",
                column: "Producto_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedido_Pedido_Id",
                table: "DetallesPedido",
                column: "Pedido_Id");

            migrationBuilder.CreateIndex(
                name: "IX_DetallesPedido_Producto_Id",
                table: "DetallesPedido",
                column: "Producto_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Empleados_Usuario_ID",
                table: "Empleados",
                column: "Usuario_ID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expedientes_Cliente_Id",
                table: "Expedientes",
                column: "Cliente_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Expedientes_Empleado_Id",
                table: "Expedientes",
                column: "Empleado_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_Cliente_Id",
                table: "Facturas",
                column: "Cliente_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_Pedido_Id",
                table: "Facturas",
                column: "Pedido_Id");

            migrationBuilder.CreateIndex(
                name: "IX_HorariosEmpleados_Empleado_Id",
                table: "HorariosEmpleados",
                column: "Empleado_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Inventarios_Producto_Id",
                table: "Inventarios",
                column: "Producto_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_Cliente_Id",
                table: "Pedidos",
                column: "Cliente_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_Cupon_Id",
                table: "Pedidos",
                column: "Cupon_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_Categoria_Id",
                table: "Productos",
                column: "Categoria_Id");

            migrationBuilder.CreateIndex(
                name: "IX_RecetasMedicas_Expediente_Id",
                table: "RecetasMedicas",
                column: "Expediente_Id");

            migrationBuilder.CreateIndex(
                name: "IX_RecetasMedicas_Producto_Id",
                table: "RecetasMedicas",
                column: "Producto_Id");

            migrationBuilder.CreateIndex(
                name: "IX_RecordatoriosTratamiento_Tratamiento_Id",
                table: "RecordatoriosTratamiento",
                column: "Tratamiento_Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tratamientos_Cliente_Id",
                table: "Tratamientos",
                column: "Cliente_Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CarritoItems");

            migrationBuilder.DropTable(
                name: "Citas");

            migrationBuilder.DropTable(
                name: "ClientesCupones");

            migrationBuilder.DropTable(
                name: "DetallesFactura");

            migrationBuilder.DropTable(
                name: "DetallesPedido");

            migrationBuilder.DropTable(
                name: "HorariosEmpleados");

            migrationBuilder.DropTable(
                name: "Inventarios");

            migrationBuilder.DropTable(
                name: "RecetasMedicas");

            migrationBuilder.DropTable(
                name: "RecordatoriosTratamiento");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Facturas");

            migrationBuilder.DropTable(
                name: "Expedientes");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Tratamientos");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Empleados");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Cupones");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
