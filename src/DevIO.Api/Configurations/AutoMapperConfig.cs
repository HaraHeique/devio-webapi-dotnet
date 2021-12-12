using AutoMapper;
using DevIO.Api.ViewModels;
using DevIO.Business.Models;
using System;

namespace DevIO.Api.Configurations
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            // Commons
            CreateMap<string, Guid>()
                .ConvertUsing(x => string.IsNullOrEmpty(x) ? Guid.NewGuid() : Guid.Parse(x));

            CreateMap<string, Guid?>()
                .ConvertUsing(x => string.IsNullOrEmpty(x) ? (Guid?)null : Guid.Parse(x));

            CreateMap<Guid, string>()
                .ConvertUsing(x => Convert.ToString(x));

            CreateMap<Guid, Guid>()
                .ConvertUsing(x => x == Guid.Empty ? Guid.NewGuid() : x);

            // Both
            CreateMap<Fornecedor, FornecedorViewModel>().ReverseMap();
            CreateMap<Endereco, EnderecoViewModel>().ReverseMap();

            // Model to ViewModel
            CreateMap<Produto, ProdutoViewModel>()
                .ForMember(dest => dest.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome))
                .ForMember(dest => dest.Imagem, opt => opt.MapFrom(src => $"img/{src.Imagem}"));

            CreateMap<Produto, ProdutoImagemViewModel>()
                .ForMember(dest => dest.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome))
                .ForMember(dest => dest.Imagem, opt => opt.MapFrom(src => $"img/{src.Imagem}"));

            // ViewModel to Model
            CreateMap<ProdutoViewModel, Produto>();
            CreateMap<ProdutoImagemViewModel, Produto>();
        }
    }
}
