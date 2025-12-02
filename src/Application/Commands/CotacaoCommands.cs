using Application.DTOs;
using MediatR;

namespace Application.Commands;

public record CriarRascunhoCotacaoCommand(CriarRascunhoCotacaoRequestDto Request) : IRequest<int>; // retorna Id
public record CalcularCotacaoCommand(int CotacaoId) : IRequest<bool>; // sucesso
public record AplicarDescontoComercialCommand(int CotacaoId, decimal Percentual) : IRequest<bool>;
public record AprovarCotacaoCommand(int CotacaoId) : IRequest<bool>;
public record CancelarCotacaoCommand(int CotacaoId) : IRequest<bool>;
