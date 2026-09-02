using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ServicoEstoque.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_Estoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "estoque");

            migrationBuilder.CreateTable(
                name: "Produtos",
                schema: "estoque",
                columns: table => new
                {
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Saldo = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Codigo);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Produtos",
                schema: "estoque");
        }
    }
}
