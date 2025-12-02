using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Baseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Criação completa do schema inicial conforme Domain Model
            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    PercentualComissao = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    IofPercentual = table.Column<decimal>(type: "decimal(10,4)", nullable: false),
                    CustoServicos = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DescontoComercialMaxPercentual = table.Column<decimal>(type: "decimal(10,4)", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Produtos", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "FatorBonus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClasseBonus = table.Column<int>(type: "int", nullable: false),
                    Fator = table.Column<decimal>(type: "decimal(10,4)", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_FatorBonus", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "FatorFranquia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Franquia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Fator = table.Column<decimal>(type: "decimal(10,4)", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_FatorFranquia", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "FatorPerfil",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaixaIdade = table.Column<int>(type: "int", nullable: false),
                    Genero = table.Column<int>(type: "int", nullable: false),
                    Fator = table.Column<decimal>(type: "decimal(10,4)", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_FatorPerfil", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "FatorRegiao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CepInicio = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    CepFim = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Fator = table.Column<decimal>(type: "decimal(10,4)", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_FatorRegiao", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "FatorUtilizacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoUtilizacao = table.Column<int>(type: "int", nullable: false),
                    Fator = table.Column<decimal>(type: "decimal(10,4)", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_FatorUtilizacao", x => x.Id); });

            migrationBuilder.CreateTable(
                name: "Coberturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    ValorMinimo = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    ValorMaximo = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coberturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coberturas_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cotacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProdutoId = table.Column<int>(type: "int", nullable: false),
                    CorretorId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DtCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DtValidade = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PremioLiquido = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DescontoComercial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PremioComercial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comissao = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PremioTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cotacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proponentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CotacaoId = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CpfCnpj = table.Column<string>(type: "nvarchar(14)", maxLength: 14, nullable: false),
                    Genero = table.Column<int>(type: "int", nullable: false),
                    EstadoCivil = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DtNascimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CepResidencial = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proponentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Proponentes_Cotacoes_CotacaoId",
                        column: x => x.CotacaoId,
                        principalTable: "Cotacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Veiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CotacaoId = table.Column<int>(type: "int", nullable: false),
                    Placa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Chassi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CodigoFipeOuVeiculo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AnoModelo = table.Column<int>(type: "int", nullable: false),
                    AnoFabricacao = table.Column<int>(type: "int", nullable: false),
                    CepPernoite = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    TipoUtilizacao = table.Column<int>(type: "int", nullable: false),
                    ZeroKm = table.Column<bool>(type: "bit", nullable: false),
                    Blindado = table.Column<bool>(type: "bit", nullable: false),
                    KitGas = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Veiculos_Cotacoes_CotacaoId",
                        column: x => x.CotacaoId,
                        principalTable: "Cotacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CotacaoCoberturas",
                columns: table => new
                {
                    CotacaoId = table.Column<int>(type: "int", nullable: false),
                    CoberturaId = table.Column<int>(type: "int", nullable: false),
                    ImportanciaSegurada = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PremioCobertura = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FranquiaSelecionada = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Contratada = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CotacaoCoberturas", x => new { x.CotacaoId, x.CoberturaId });
                    table.ForeignKey(
                        name: "FK_CotacaoCoberturas_Cotacoes_CotacaoId",
                        column: x => x.CotacaoId,
                        principalTable: "Cotacoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Índices
            migrationBuilder.CreateIndex(name: "IX_Coberturas_ProdutoId", table: "Coberturas", column: "ProdutoId");
            migrationBuilder.CreateIndex(name: "IX_Proponentes_CotacaoId", table: "Proponentes", column: "CotacaoId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Veiculos_CotacaoId", table: "Veiculos", column: "CotacaoId", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Cotacoes_Numero", table: "Cotacoes", column: "Numero", unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CotacaoCoberturas");
            migrationBuilder.DropTable(name: "Veiculos");
            migrationBuilder.DropTable(name: "Proponentes");
            migrationBuilder.DropTable(name: "Coberturas");
            migrationBuilder.DropTable(name: "FatorUtilizacao");
            migrationBuilder.DropTable(name: "FatorRegiao");
            migrationBuilder.DropTable(name: "FatorPerfil");
            migrationBuilder.DropTable(name: "FatorFranquia");
            migrationBuilder.DropTable(name: "FatorBonus");
            migrationBuilder.DropTable(name: "Cotacoes");
            migrationBuilder.DropTable(name: "Produtos");
        }
    }
}
