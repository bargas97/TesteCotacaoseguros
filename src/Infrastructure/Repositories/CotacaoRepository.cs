using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Threading;

namespace Infrastructure.Repositories;

public class CotacaoRepository : ICotacaoRepository
{
    private readonly CotacaoDbContext _ctx;
    private static int _memSeq = 0; // sequência em memória para testes InMemory
    public CotacaoRepository(CotacaoDbContext ctx){ _ctx = ctx; }

    public async Task<Cotacao> CriarRascunhoAsync(Cotacao cotacao, CancellationToken ct)
    {
        _ctx.Cotacoes.Add(cotacao);
        return cotacao;
    }

    public Task<Cotacao?> ObterPorIdAsync(int id, CancellationToken ct)
    {
        return _ctx.Cotacoes
            .Include(x => x.Proponente)
            .Include(x => x.Veiculo)
            .Include(x => x.Coberturas)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public Task<Produto?> ObterProdutoAsync(int produtoId, CancellationToken ct)
        => _ctx.Produtos.FirstOrDefaultAsync(x => x.Id == produtoId, ct);

    public Task<List<Cotacao>> BuscarAsync(string? cpf, StatusCotacao? status, CancellationToken ct)
    {
        var q = _ctx.Cotacoes
            .Include(x => x.Proponente)
            .Include(x => x.Veiculo)
            .Include(x => x.Coberturas)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(cpf)) q = q.Where(x => x.Proponente != null && x.Proponente.CpfCnpj == cpf);
        if (status.HasValue) q = q.Where(x => x.Status == status);
        return q.ToListAsync(ct);
    }

    public Task AtualizarAsync(Cotacao cotacao, CancellationToken ct)
    {
        _ctx.Cotacoes.Update(cotacao);
        return Task.CompletedTask;
    }

    public Task SalvarAlteracoesAsync(CancellationToken ct) => _ctx.SaveChangesAsync(ct);

    public Task<List<FatorBonus>> ObterFatoresBonusAsync(CancellationToken ct) => _ctx.FatoresBonus.ToListAsync(ct);
    public Task<List<FatorPerfil>> ObterFatoresPerfilAsync(CancellationToken ct) => _ctx.FatoresPerfil.ToListAsync(ct);
    public Task<List<FatorRegiao>> ObterFatoresRegiaoAsync(CancellationToken ct) => _ctx.FatoresRegiao.ToListAsync(ct);
    public Task<List<FatorUtilizacao>> ObterFatoresUtilizacaoAsync(CancellationToken ct) => _ctx.FatoresUtilizacao.ToListAsync(ct);
    public Task<List<FatorFranquia>> ObterFatoresFranquiaAsync(CancellationToken ct) => _ctx.FatoresFranquia.ToListAsync(ct);

    public async Task<string> GerarNumeroAsync(CancellationToken ct)
    {
        var ano = DateTime.UtcNow.Year;
        // Fallback para provider InMemory (não suporta sequence SQL)
        if (_ctx.Database.ProviderName?.Contains("InMemory") == true)
        {
            var seq = Interlocked.Increment(ref _memSeq);
            return $"{ano}-{seq:000000}";
        }

        var conn = _ctx.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open) await conn.OpenAsync(ct);
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT NEXT VALUE FOR dbo.CotacaoSeq";
        var result = await cmd.ExecuteScalarAsync(ct);
        long next = Convert.ToInt64(result);
        return $"{ano}-{next:000000}";
    }
}