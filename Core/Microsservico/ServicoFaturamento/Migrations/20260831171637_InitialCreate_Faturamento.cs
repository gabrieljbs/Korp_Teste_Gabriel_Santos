using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ServicoFaturamento.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_Faturamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "faturamento");

            migrationBuilder.CreateTable(
                name: "NotasFiscais",
                schema: "faturamento",
                columns: table => new
                {
                    Numero = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasFiscais", x => x.Numero);
                });

            migrationBuilder.CreateTable(
                name: "ItensNotaFiscal",
                schema: "faturamento",
                columns: table => new
                {
                    NotaFiscalNumero = table.Column<long>(type: "bigint", nullable: false),
                    CodigoProduto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DescricaoProduto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensNotaFiscal", x => new { x.NotaFiscalNumero, x.CodigoProduto });
                    table.ForeignKey(
                        name: "FK_ItensNotaFiscal_NotasFiscais_NotaFiscalNumero",
                        column: x => x.NotaFiscalNumero,
                        principalSchema: "faturamento",
                        principalTable: "NotasFiscais",
                        principalColumn: "Numero",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItensNotaFiscal",
                schema: "faturamento");

            migrationBuilder.DropTable(
                name: "NotasFiscais",
                schema: "faturamento");
        }
    }
}
