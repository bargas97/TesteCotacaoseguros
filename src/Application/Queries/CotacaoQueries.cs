using Application.DTOs;
using MediatR;

namespace Application.Queries;

public record ObterCotacaoQuery(int Id) : IRequest<CotacaoResponseDto?>;
public record BuscarCotacoesQuery(string? Cpf, Domain.Entities.StatusCotacao? Status) : IRequest<List<CotacaoResponseDto>>;
