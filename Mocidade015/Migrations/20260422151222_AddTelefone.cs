using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mocidade015.Migrations
{
    /// <inheritdoc />
    public partial class AddTelefone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Onibus",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    terminalsaida = table.Column<string>(type: "text", nullable: false),
                    horariosaida = table.Column<TimeSpan>(type: "interval", nullable: false),
                    lotacaomaxima = table.Column<int>(type: "integer", nullable: false),
                    dataviagem = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Onibus", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    rg = table.Column<string>(type: "text", nullable: true),
                    Telefone = table.Column<string>(type: "text", nullable: true),
                    senhahash = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    datacriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Assentos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    onibusid = table.Column<Guid>(type: "uuid", nullable: false),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    ocupado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assentos", x => x.id);
                    table.ForeignKey(
                        name: "FK_Assentos_Onibus_onibusid",
                        column: x => x.onibusid,
                        principalTable: "Onibus",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Acompanhantes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuarioresponsavelid = table.Column<Guid>(type: "uuid", nullable: false),
                    nome = table.Column<string>(type: "text", nullable: false),
                    rgcpf = table.Column<string>(type: "text", nullable: true),
                    Telefone = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acompanhantes", x => x.id);
                    table.ForeignKey(
                        name: "FK_Acompanhantes_Usuarios_usuarioresponsavelid",
                        column: x => x.usuarioresponsavelid,
                        principalTable: "Usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListaEspera",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuarioid = table.Column<Guid>(type: "uuid", nullable: false),
                    terminaldesejado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    datasolicitacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListaEspera", x => x.id);
                    table.ForeignKey(
                        name: "FK_ListaEspera_Usuarios_usuarioid",
                        column: x => x.usuarioid,
                        principalTable: "Usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuarioid = table.Column<Guid>(type: "uuid", nullable: false),
                    assentoid = table.Column<Guid>(type: "uuid", nullable: false),
                    acompanhanteid = table.Column<Guid>(type: "uuid", nullable: true),
                    valor = table.Column<decimal>(type: "numeric", nullable: false),
                    datareserva = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.id);
                    table.ForeignKey(
                        name: "FK_Reservas_Acompanhantes_acompanhanteid",
                        column: x => x.acompanhanteid,
                        principalTable: "Acompanhantes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Reservas_Assentos_assentoid",
                        column: x => x.assentoid,
                        principalTable: "Assentos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reservas_Usuarios_usuarioid",
                        column: x => x.usuarioid,
                        principalTable: "Usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Acompanhantes_usuarioresponsavelid",
                table: "Acompanhantes",
                column: "usuarioresponsavelid");

            migrationBuilder.CreateIndex(
                name: "IX_Assentos_onibusid_numero",
                table: "Assentos",
                columns: new[] { "onibusid", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListaEspera_usuarioid",
                table: "ListaEspera",
                column: "usuarioid");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_acompanhanteid",
                table: "Reservas",
                column: "acompanhanteid");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_assentoid",
                table: "Reservas",
                column: "assentoid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_usuarioid",
                table: "Reservas",
                column: "usuarioid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ListaEspera");

            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Acompanhantes");

            migrationBuilder.DropTable(
                name: "Assentos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Onibus");
        }
    }
}
