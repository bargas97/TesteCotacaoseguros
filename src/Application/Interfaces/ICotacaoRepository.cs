using Domain.Entities;

namespace Application.Interfaces;

public interface ICotacaoRepository
{
    Task<Cotacao> CriarRascunhoAsync(Cotacao cotacao, CancellationToken ct);
    Task<Cotacao?> ObterPorIdAsync(int id, CancellationToken ct);
    Task<Produto?> ObterProdutoAsync(int produtoId, CancellationToken ct);
    Task<List<Cotacao>> BuscarAsync(string? cpf, StatusCotacao? status, CancellationToken ct);
    Task AtualizarAsync(Cotacao cotacao, CancellationToken ct);
    Task SalvarAlteracoesAsync(CancellationToken ct);

    // Fatores
    Task<List<FatorBonus>> ObterFatoresBonusAsync(CancellationToken ct);
    Task<List<FatorPerfil>> ObterFatoresPerfilAsync(CancellationToken ct);
    Task<List<FatorRegiao>> ObterFatoresRegiaoAsync(CancellationToken ct);
    Task<List<FatorUtilizacao>> ObterFatoresUtilizacaoAsync(CancellationToken ct);
    Task<List<FatorFranquia>> ObterFatoresFranquiaAsync(CancellationToken ct);

    // Número único por sequência anual
    Task<string> GerarNumeroAsync(CancellationToken ct);
}