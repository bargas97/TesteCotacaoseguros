using Application.DTOs;
using Application.Commands;
using FluentValidation;
using Domain.Entities;

namespace Application.Validators;

public class ProponenteValidator : AbstractValidator<ProponenteDto>
{
    public ProponenteValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().WithMessage("Nome do proponente é obrigatório.");
        RuleFor(x => x.CpfCnpj).NotEmpty().Must(CpfValido).WithMessage("CPF inválido.");
        RuleFor(x => x.DtNascimento).Must(d => d <= DateTime.Today.AddYears(-18)).WithMessage("Idade mínima 18 anos.");
        RuleFor(x => x.CepResidencial).NotEmpty().Length(8).WithMessage("CEP inválido.");
    }

    private bool CpfValido(string cpf)
    {
        cpf = new string(cpf.Where(char.IsDigit).ToArray());
        if (cpf.Length != 11) return false;
        if (cpf.Distinct().Count() == 1) return false;
        int[] mult1 = {10,9,8,7,6,5,4,3,2};
        int[] mult2 = {11,10,9,8,7,6,5,4,3,2};
        var temp = cpf[..9];
        int soma = 0;
        for(int i=0;i<9;i++) soma += int.Parse(temp[i].ToString()) * mult1[i];
        int resto = soma % 11; var dig1 = resto < 2 ? 0 : 11-resto;
        temp += dig1;
        soma = 0;
        for(int i=0;i<10;i++) soma += int.Parse(temp[i].ToString()) * mult2[i];
        resto = soma % 11; var dig2 = resto < 2 ? 0 : 11-resto;
        return cpf.EndsWith(dig1.ToString()+dig2.ToString());
    }
}

public class VeiculoValidator : AbstractValidator<VeiculoDto>
{
    public VeiculoValidator()
    {
        RuleFor(x => x.CodigoFipeOuVeiculo).NotEmpty();
        RuleFor(x => x.AnoModelo).GreaterThan(1900);
        RuleFor(x => x.CepPernoite).NotEmpty().Length(8);
    }
}

public class CotacaoCoberturaInputValidator : AbstractValidator<CotacaoCoberturaInputDto>
{
    public CotacaoCoberturaInputValidator()
    {
        RuleFor(x => x.CoberturaId).GreaterThan(0);
        RuleFor(x => x.ImportanciaSegurada).GreaterThan(0);
    }
}

public class CriarRascunhoCotacaoRequestDtoValidator : AbstractValidator<CriarRascunhoCotacaoRequestDto>
{
    public CriarRascunhoCotacaoRequestDtoValidator()
    {
        RuleFor(x => x.ProdutoId).GreaterThan(0);
        RuleFor(x => x.Proponente).SetValidator(new ProponenteValidator());
        RuleFor(x => x.Veiculo).SetValidator(new VeiculoValidator());
        RuleFor(x => x.Coberturas).NotEmpty().WithMessage("Pelo menos uma cobertura deve ser enviada.");
        RuleForEach(x => x.Coberturas).SetValidator(new CotacaoCoberturaInputValidator());
    }
}

public class CalcularCotacaoCommandValidator : AbstractValidator<CalcularCotacaoCommand>
{
    public CalcularCotacaoCommandValidator()
    {
        RuleFor(x => x.CotacaoId).GreaterThan(0);
    }
}

public class AplicarDescontoComercialCommandValidator : AbstractValidator<AplicarDescontoComercialCommand>
{
    public AplicarDescontoComercialCommandValidator()
    {
        RuleFor(x => x.CotacaoId).GreaterThan(0);
        RuleFor(x => x.Percentual).InclusiveBetween(0, 0.50m).WithMessage("Percentual de desconto inválido."); // limite genérico, validado novamente no domínio
    }
}

public class AprovarCotacaoCommandValidator : AbstractValidator<AprovarCotacaoCommand>
{
    public AprovarCotacaoCommandValidator(){ RuleFor(x => x.CotacaoId).GreaterThan(0); }
}

public class CancelarCotacaoCommandValidator : AbstractValidator<CancelarCotacaoCommand>
{
    public CancelarCotacaoCommandValidator(){ RuleFor(x => x.CotacaoId).GreaterThan(0); }
}
