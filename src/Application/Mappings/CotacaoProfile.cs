using AutoMapper;
using Application.DTOs;
using Domain.Entities;

namespace Application.Mappings;

public class CotacaoProfile : Profile
{
    public CotacaoProfile()
    {
        CreateMap<Proponente, ProponenteDto>().ReverseMap();
        CreateMap<Veiculo, VeiculoDto>().ReverseMap();
        CreateMap<CotacaoCobertura, CotacaoCoberturaDto>();
        CreateMap<Cotacao, CotacaoResponseDto>()
            .ForMember(d => d.Coberturas, o => o.MapFrom(s => s.Coberturas))
            .ForMember(d => d.Proponente, o => o.MapFrom(s => s.Proponente))
            .ForMember(d => d.Veiculo, o => o.MapFrom(s => s.Veiculo))
            .ForMember(d => d.IofValor, o => o.MapFrom(s => s.IofValor))
            .ForMember(d => d.CustoServicosAplicado, o => o.MapFrom(s => s.CustoServicosAplicado));
    }
}