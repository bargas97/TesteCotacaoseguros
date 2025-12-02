using System;
using System.Collections.Generic;
using Domain.Entities;

namespace Application.DTOs;

public class ProponenteDto
{
    public string Nome { get; set; } = string.Empty;
    public string CpfCnpj { get; set; } = string.Empty;
    public Genero Genero { get; set; }
    public string EstadoCivil { get; set; } = string.Empty;
    public DateTime DtNascimento { get; set; }
    public string CepResidencial { get; set; } = string.Empty;
}

public class VeiculoDto
{
    public string CodigoFipeOuVeiculo { get; set; } = string.Empty;
    public int AnoModelo { get; set; }
    public int AnoFabricacao { get; set; }
    public string CepPernoite { get; set; } = string.Empty;
    public TipoUtilizacao TipoUtilizacao { get; set; }
    public bool ZeroKm { get; set; }
    public bool Blindado { get; set; }
    public bool KitGas { get; set; }
    public string? Placa { get; set; }
    public string? Chassi { get; set; }
}

public class CotacaoCoberturaInputDto
{
    public int CoberturaId { get; set; }
    public decimal ImportanciaSegurada { get; set; }
    public bool Contratada { get; set; }
    public string? FranquiaSelecionada { get; set; }
}

public class CriarRascunhoCotacaoRequestDto
{
    public int ProdutoId { get; set; }
    public ProponenteDto Proponente { get; set; } = new();
    public VeiculoDto Veiculo { get; set; } = new();
    public List<CotacaoCoberturaInputDto> Coberturas { get; set; } = new();
}

public class CalcularCotacaoRequestDto
{
    public int CotacaoId { get; set; }
}

public class AplicarDescontoRequestDto
{
    public int CotacaoId { get; set; }
    public decimal Percentual { get; set; }
}

public class CotacaoCoberturaDto
{
    public int CoberturaId { get; set; }
    public decimal ImportanciaSegurada { get; set; }
    public decimal PremioCobertura { get; set; }
    public string? FranquiaSelecionada { get; set; }
    public bool Contratada { get; set; }
}

public class CotacaoResponseDto
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int ProdutoId { get; set; }
    public StatusCotacao Status { get; set; }
    public DateTime DtCriacao { get; set; }
    public DateTime? DtValidade { get; set; }
    public decimal PremioLiquido { get; set; }
    public decimal DescontoComercial { get; set; }
    public decimal PremioComercial { get; set; }
    public decimal Comissao { get; set; }
    public decimal PremioTotal { get; set; }
    public decimal IofValor { get; set; }
    public decimal CustoServicosAplicado { get; set; }
    public ProponenteDto? Proponente { get; set; }
    public VeiculoDto? Veiculo { get; set; }
    public List<CotacaoCoberturaDto> Coberturas { get; set; } = new();
}
