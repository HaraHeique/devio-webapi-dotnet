using AutoMapper;
using DevIO.Api.ViewModels;
using DevIO.Business.Models;
using System.IO;

namespace DevIO.Api.Configurations
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            // Both
            CreateMap<Fornecedor, FornecedorViewModel>().ReverseMap();
            CreateMap<Endereco, EnderecoViewModel>().ReverseMap();

            // Model to ViewModel
            CreateMap<Produto, ProdutoViewModel>()
                .ForMember(dest => dest.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome))
                .ForMember(dest => dest.Imagem, opt => opt.MapFrom(src => Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", src.Imagem)));

            CreateMap<Produto, ProdutoImagemViewModel>()
                .ForMember(dest => dest.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome))
                .ForMember(dest => dest.Imagem, opt => opt.MapFrom(src => Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", src.Imagem)));

            // ViewModel to Model
            CreateMap<ProdutoViewModel, Produto>();
            CreateMap<ProdutoImagemViewModel, Produto>();
        }
    }
}
