using Application.Commands;
using Application.DTOs;
using Application.Interfaces;
using Application.Queries;
using AutoMapper;
using Domain.Entities;
using Domain.Services;
using MediatR;

namespace Application.Handlers;

public class CriarRascunhoCotacaoCommandHandler : IRequestHandler<CriarRascunhoCotacaoCommand, int>
{
    private readonly ICotacaoRepository _repo;
    private readonly IMapper _mapper;
    public CriarRascunhoCotacaoCommandHandler(ICotacaoRepository repo, IMapper mapper) { _repo = repo; _mapper = mapper; }

    public async Task<int> Handle(CriarRascunhoCotacaoCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Request;
        var cotacao = new Cotacao(dto.ProdutoId);
        var numero = await _repo.GerarNumeroAsync(cancellationToken);
        cotacao.DefinirNumero(numero);
        var prop = new Proponente(dto.Proponente.Nome, dto.Proponente.CpfCnpj, dto.Proponente.Genero, dto.Proponente.EstadoCivil, dto.Proponente.DtNascimento, dto.Proponente.CepResidencial);
        var vei = new Veiculo(dto.Veiculo.CodigoFipeOuVeiculo, dto.Veiculo.AnoModelo, dto.Veiculo.AnoFabricacao, dto.Veiculo.CepPernoite, dto.Veiculo.TipoUtilizacao, dto.Veiculo.ZeroKm);
        cotacao.DefinirProponente(prop);
        cotacao.DefinirVeiculo(vei);
        foreach (var c in dto.Coberturas)
        {
            cotacao.AdicionarCobertura(new CotacaoCobertura(c.CoberturaId, c.ImportanciaSegurada, c.Contratada, c.FranquiaSelecionada));
        }
        await _repo.CriarRascunhoAsync(cotacao, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
        return cotacao.Id;
    }
}

public class CalcularCotacaoCommandHandler : IRequestHandler<CalcularCotacaoCommand, bool>
{
    private readonly ICotacaoRepository _repo;
    private readonly CalculoPremioService _service;
    public CalcularCotacaoCommandHandler(ICotacaoRepository repo, CalculoPremioService service){ _repo = repo; _service = service; }

    public async Task<bool> Handle(CalcularCotacaoCommand request, CancellationToken cancellationToken)
    {
        var cotacao = await _repo.ObterPorIdAsync(request.CotacaoId, cancellationToken) ?? throw new KeyNotFoundException("Cotação não encontrada.");
        var produto = await _repo.ObterProdutoAsync(cotacao.ProdutoId, cancellationToken) ?? throw new KeyNotFoundException("Produto não encontrado.");
        var bonus = await _repo.ObterFatoresBonusAsync(cancellationToken);
        var perfil = await _repo.ObterFatoresPerfilAsync(cancellationToken);
        var regiao = await _repo.ObterFatoresRegiaoAsync(cancellationToken);
        var utilizacao = await _repo.ObterFatoresUtilizacaoAsync(cancellationToken);
        var franquia = await _repo.ObterFatoresFranquiaAsync(cancellationToken);
        _service.CalcularPremios(cotacao, produto, bonus, perfil, regiao, utilizacao, franquia);
        await _repo.AtualizarAsync(cotacao, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
        return true;
    }
}

public class AplicarDescontoComercialCommandHandler : IRequestHandler<AplicarDescontoComercialCommand, bool>
{
    private readonly ICotacaoRepository _repo;
    public AplicarDescontoComercialCommandHandler(ICotacaoRepository repo){ _repo = repo; }
    public async Task<bool> Handle(AplicarDescontoComercialCommand request, CancellationToken cancellationToken)
    {
        var cotacao = await _repo.ObterPorIdAsync(request.CotacaoId, cancellationToken) ?? throw new KeyNotFoundException("Cotação não encontrada.");
        var produto = await _repo.ObterProdutoAsync(cotacao.ProdutoId, cancellationToken) ?? throw new KeyNotFoundException("Produto não encontrado.");
        cotacao.AplicarDesconto(request.Percentual, produto);
        await _repo.AtualizarAsync(cotacao, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
        return true;
    }
}

public class AprovarCotacaoCommandHandler : IRequestHandler<AprovarCotacaoCommand, bool>
{
    private readonly ICotacaoRepository _repo;
    public AprovarCotacaoCommandHandler(ICotacaoRepository repo){ _repo = repo; }
    public async Task<bool> Handle(AprovarCotacaoCommand request, CancellationToken cancellationToken)
    {
        var cotacao = await _repo.ObterPorIdAsync(request.CotacaoId, cancellationToken) ?? throw new KeyNotFoundException("Cotação não encontrada.");
        cotacao.MarcarAprovada();
        await _repo.AtualizarAsync(cotacao, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
        return true;
    }
}

public class CancelarCotacaoCommandHandler : IRequestHandler<CancelarCotacaoCommand, bool>
{
    private readonly ICotacaoRepository _repo;
    public CancelarCotacaoCommandHandler(ICotacaoRepository repo){ _repo = repo; }
    public async Task<bool> Handle(CancelarCotacaoCommand request, CancellationToken cancellationToken)
    {
        var cotacao = await _repo.ObterPorIdAsync(request.CotacaoId, cancellationToken) ?? throw new KeyNotFoundException("Cotação não encontrada.");
        cotacao.MarcarCancelada();
        await _repo.AtualizarAsync(cotacao, cancellationToken);
        await _repo.SalvarAlteracoesAsync(cancellationToken);
        return true;
    }
}

public class ObterCotacaoQueryHandler : IRequestHandler<ObterCotacaoQuery, CotacaoResponseDto?>
{
    private readonly ICotacaoRepository _repo;
    private readonly IMapper _mapper;
    public ObterCotacaoQueryHandler(ICotacaoRepository repo, IMapper mapper){ _repo = repo; _mapper = mapper; }
    public async Task<CotacaoResponseDto?> Handle(ObterCotacaoQuery request, CancellationToken cancellationToken)
    {
        var cotacao = await _repo.ObterPorIdAsync(request.Id, cancellationToken);
        return cotacao == null ? null : _mapper.Map<CotacaoResponseDto>(cotacao);
    }
}

public class BuscarCotacoesQueryHandler : IRequestHandler<BuscarCotacoesQuery, List<CotacaoResponseDto>>
{
    private readonly ICotacaoRepository _repo;
    private readonly IMapper _mapper;
    public BuscarCotacoesQueryHandler(ICotacaoRepository repo, IMapper mapper){ _repo = repo; _mapper = mapper; }
    public async Task<List<CotacaoResponseDto>> Handle(BuscarCotacoesQuery request, CancellationToken cancellationToken)
    {
        var lista = await _repo.BuscarAsync(request.Cpf, request.Status, cancellationToken);
        return lista.Select(x => _mapper.Map<CotacaoResponseDto>(x)).ToList();
    }
}
