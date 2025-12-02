using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Controllers;

[ApiController]
[Route("catalogo")]
public class CatalogoController : ControllerBase
{
    private readonly CotacaoDbContext _db;
    public CatalogoController(CotacaoDbContext db) => _db = db;

    [HttpGet("produtos")]
    public async Task<IActionResult> GetProdutos()
    {
        var itens = await _db.Produtos
            .AsNoTracking()
            .Select(p => new { p.Id, p.Nome, p.Ativo })
            .OrderBy(p => p.Nome)
            .ToListAsync();
        return Ok(itens);
    }

    [HttpGet("produtos/{id:int}/coberturas")]
    public async Task<IActionResult> GetCoberturas(int id)
    {
        var existe = await _db.Produtos.AsNoTracking().AnyAsync(p => p.Id == id);
        if (!existe) return NotFound();

        var itens = await _db.Coberturas
            .AsNoTracking()
            .Where(c => c.ProdutoId == id)
            .Select(c => new { c.Id, c.Codigo, c.Descricao, c.Tipo, c.ValorMinimo, c.ValorMaximo })
            .OrderBy(c => c.Codigo)
            .ToListAsync();
        return Ok(itens);
    }
}
