using Application.Commands;
using Application.DTOs;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

/// <summary>
/// Operações de cotação de seguro auto.
/// </summary>
/// <remarks>
/// Exemplo criar rascunho:
/// {
///   "produtoId": 1,
///   "proponente": {
///     "nome": "Maria Teste",
///     "cpfCnpj": "52998224725",
///     "genero": 0,
///     "estadoCivil": "Solteiro",
///     "dtNascimento": "1995-05-10",
///     "cepResidencial": "06000000"
///   },
///   "veiculo": {
///     "codigoFipeOuVeiculo": "FIPE123",
///     "anoModelo": 2024,
///     "anoFabricacao": 2024,
///     "cepPernoite": "06000000",
///     "tipoUtilizacao": 1,
///     "zeroKm": false,
///     "blindado": false,
///     "kitGas": false
///   },
///   "coberturas": [
///     {
///       "coberturaId": 1,
///       "importanciaSegurada": 10000,
///       "contratada": true,
///       "franquiaSelecionada": "Alta"
///     }
///   ]
/// }
/// </remarks>
/// <response code="201">Retorna id da nova cotação</response>
/// <response code="400">Dados inválidos</response>
[ApiController]
[Route("cotacoes")]
public class CotacoesController : ControllerBase
{
    private readonly IMediator _mediator;
    public CotacoesController(IMediator mediator) { _mediator = mediator; }

    /// <summary>Criar cotação em rascunho.</summary>
    /// <param name="dto">Payload de criação.</param>
    /// <returns>Id da cotação.</returns>
    [HttpPost]
    public async Task<IActionResult> CriarRascunho([FromBody] CriarRascunhoCotacaoRequestDto dto)
    {
        var id = await _mediator.Send(new CriarRascunhoCotacaoCommand(dto));
        return Created($"/cotacoes/{id}", new { id });
    }

    /// <summary>Calcular cotação.</summary>
    /// <remarks>Necessário: proponente, veículo e ao menos uma cobertura contratada.</remarks>
    /// <response code="204">Calculada com sucesso</response>
    /// <response code="400">Falta de dados mínimos</response>
    [HttpPut("{id}/calcular")]
    public async Task<IActionResult> Calcular(int id)
    {
        await _mediator.Send(new CalcularCotacaoCommand(id));
        return NoContent();
    }

    /// <summary>Aplicar desconto comercial.</summary>
    /// <remarks>Ex: { "percentual": 0.10 }</remarks>
    /// <response code="204">Desconto aplicado</response>
    /// <response code="400">Acima do teto permitido</response>
    [HttpPut("{id}/desconto-comercial")]
    public async Task<IActionResult> Desconto(int id, [FromBody] AplicarDescontoRequestDto dto)
    {
        await _mediator.Send(new AplicarDescontoComercialCommand(id, dto.Percentual));
        return NoContent();
    }

    /// <summary>Aprovar cotação calculada.</summary>
    /// <response code="204">Cotação aprovada</response>
    /// <response code="400">Cotação não está calculada</response>
    [HttpPut("{id}/aprovar")]
    public async Task<IActionResult> Aprovar(int id)
    {
        await _mediator.Send(new AprovarCotacaoCommand(id));
        return NoContent();
    }

    /// <summary>Cancelar cotação (Rascunho ou Calculada).</summary>
    /// <response code="204">Cotação cancelada</response>
    /// <response code="400">Status não permite cancelamento</response>
    [HttpPut("{id}/cancelar")]
    public async Task<IActionResult> Cancelar(int id)
    {
        await _mediator.Send(new CancelarCotacaoCommand(id));
        return NoContent();
    }

    /// <summary>Obter cotação por id.</summary>
    /// <response code="200">Cotação encontrada</response>
    /// <response code="404">Não encontrada</response>
    [HttpGet("{id}")]
    public async Task<IActionResult> Obter(int id)
    {
        var resp = await _mediator.Send(new ObterCotacaoQuery(id));
        return resp == null ? NotFound() : Ok(resp);
    }

    /// <summary>Buscar cotações por CPF e/ou status.</summary>
    [HttpGet]
    public async Task<IActionResult> Buscar([FromQuery] string? cpf, [FromQuery] Domain.Entities.StatusCotacao? status)
    {
        var lista = await _mediator.Send(new BuscarCotacoesQuery(cpf, status));
        return Ok(lista);
    }
}
