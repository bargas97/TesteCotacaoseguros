using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("Produtos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Ativo);
        builder.Property(x => x.PercentualComissao).HasColumnType("decimal(10,4)");
        builder.Property(x => x.IofPercentual).HasColumnType("decimal(10,4)");
        builder.Property(x => x.CustoServicos).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DescontoComercialMaxPercentual).HasColumnType("decimal(10,4)");
    }
}

public class CoberturaConfiguration : IEntityTypeConfiguration<Cobertura>
{
    public void Configure(EntityTypeBuilder<Cobertura> builder)
    {
        builder.ToTable("Coberturas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Codigo).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Tipo).HasConversion<int>();
        builder.Property(x => x.ValorMinimo).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ValorMaximo).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ProdutoId);
    }
}

public class FatorBonusConfiguration : IEntityTypeConfiguration<FatorBonus>
{
    public void Configure(EntityTypeBuilder<FatorBonus> builder)
    {
        builder.ToTable("FatorBonus");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ClasseBonus).IsRequired();
        builder.Property(x => x.Fator).HasColumnType("decimal(10,4)");
    }
}

public class FatorPerfilConfiguration : IEntityTypeConfiguration<FatorPerfil>
{
    public void Configure(EntityTypeBuilder<FatorPerfil> builder)
    {
        builder.ToTable("FatorPerfil");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FaixaIdade).HasConversion<int>();
        builder.Property(x => x.Genero).HasConversion<int>();
        builder.Property(x => x.Fator).HasColumnType("decimal(10,4)");
    }
}

public class FatorRegiaoConfiguration : IEntityTypeConfiguration<FatorRegiao>
{
    public void Configure(EntityTypeBuilder<FatorRegiao> builder)
    {
        builder.ToTable("FatorRegiao");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CepInicio).HasMaxLength(8).IsRequired();
        builder.Property(x => x.CepFim).HasMaxLength(8).IsRequired();
        builder.Property(x => x.Fator).HasColumnType("decimal(10,4)");
    }
}

public class FatorUtilizacaoConfiguration : IEntityTypeConfiguration<FatorUtilizacao>
{
    public void Configure(EntityTypeBuilder<FatorUtilizacao> builder)
    {
        builder.ToTable("FatorUtilizacao");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TipoUtilizacao).HasConversion<int>();
        builder.Property(x => x.Fator).HasColumnType("decimal(10,4)");
    }
}

public class FatorFranquiaConfiguration : IEntityTypeConfiguration<FatorFranquia>
{
    public void Configure(EntityTypeBuilder<FatorFranquia> builder)
    {
        builder.ToTable("FatorFranquia");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Franquia).HasMaxLength(50);
        builder.Property(x => x.Fator).HasColumnType("decimal(10,4)");
    }
}

public class ProponenteConfiguration : IEntityTypeConfiguration<Proponente>
{
    public void Configure(EntityTypeBuilder<Proponente> builder)
    {
        builder.ToTable("Proponentes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        builder.Property(x => x.CpfCnpj).HasMaxLength(14).IsRequired();
        builder.Property(x => x.Genero).HasConversion<int>();
        builder.Property(x => x.EstadoCivil).HasMaxLength(20);
        builder.Property(x => x.CepResidencial).HasMaxLength(8);
        builder.Property(x => x.CotacaoId);
    }
}

public class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> builder)
    {
        builder.ToTable("Veiculos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CodigoFipeOuVeiculo).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CepPernoite).HasMaxLength(8).IsRequired();
        builder.Property(x => x.TipoUtilizacao).HasConversion<int>();
        builder.Property(x => x.CotacaoId);
    }
}

public class CotacaoConfiguration : IEntityTypeConfiguration<Cotacao>
{
    public void Configure(EntityTypeBuilder<Cotacao> builder)
    {
        builder.ToTable("Cotacoes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Numero).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Status).HasConversion<int>();
        builder.Property(x => x.DtCriacao);
        builder.Property(x => x.DtValidade);
        builder.Property(x => x.PremioLiquido).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DescontoComercial).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PremioComercial).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Comissao).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PremioTotal).HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Proponente).WithOne().HasForeignKey<Proponente>(x => x.CotacaoId);
        builder.HasOne(x => x.Veiculo).WithOne().HasForeignKey<Veiculo>(x => x.CotacaoId);
        builder.HasMany(x => x.Coberturas).WithOne().HasForeignKey(x => x.CotacaoId);
    }
}

public class CotacaoCoberturaConfiguration : IEntityTypeConfiguration<CotacaoCobertura>
{
    public void Configure(EntityTypeBuilder<CotacaoCobertura> builder)
    {
        builder.ToTable("CotacaoCoberturas");
        builder.HasKey(x => new { x.CotacaoId, x.CoberturaId });
        builder.Property(x => x.ImportanciaSegurada).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PremioCobertura).HasColumnType("decimal(18,2)");
        builder.Property(x => x.FranquiaSelecionada).HasMaxLength(50);
        builder.Property(x => x.Contratada);
    }
}
