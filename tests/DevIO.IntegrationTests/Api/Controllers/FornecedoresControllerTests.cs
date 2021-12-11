using AutoMapper;
using DevIO.Api;
using DevIO.Api.ViewModels;
using DevIO.Business.Interfaces.Repositories;
using DevIO.Business.Models;
using DevIO.IntegrationTests.Helpers;
using DevIO.IntegrationTests.Setups.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DevIO.IntegrationTests.Api.Controllers
{
    public class FornecedoresControllerTests : IntegrationTestsFixture
    {
        private const string CommonUri = "api/v2/fornecedores";

        private IFornecedorRepository _fornecedorRepository;
        private IMapper _mapper;

        public FornecedoresControllerTests(ApiWebApplicationFactory<Startup> factory) : base(factory) { }

        [Fact]
        public async Task Obter_Todos_Fornecedores_Com_Sucesso()
        {
            // Arrange
            await AdicionarObjsParaTestes(
                FornecedorViewModelTestsHelper.ObterInstancia(1, "70169374025"),
                FornecedorViewModelTestsHelper.ObterInstancia(2, "44249780000183")
            );

            var request = new HttpRequestMessage(HttpMethod.Get, CommonUri);

            // Act
            HttpResponseMessage response = await base.Client.SendAsync(request);

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task Obter_Fornecedor_Por_Id_Com_Sucesso()
        {
            // Arrange
            var fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(2, "10821757000108");
            Guid fornecedorId = fornecedorVM.Id;

            await AdicionarObjsParaTestes(fornecedorVM);

            var request = new HttpRequestMessage(HttpMethod.Get, $"{CommonUri}/{fornecedorId}");

            // Act
            HttpResponseMessage response = await base.Client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Obter_Endereco_Por_Id_Com_Sucesso()
        {
            // Arrange
            var fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "67701372083");
            Guid fornecedorId = fornecedorVM.Id;

            await AdicionarObjsParaTestes(fornecedorVM);

            // Act
            HttpResponseMessage response = await base.Client.GetAsync($"{CommonUri}/obter-endereco/{fornecedorId}");

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result.Data);
        }

        [Fact]
        public async Task Adicionar_Fornecedor_Com_Sucesso()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "64307756070");
            HttpContent dataRequest = ContentHelper.CreateJsonContent(fornecedorVM);

            // Act
            HttpResponseMessage response = await base.Client.PostAsync(CommonUri, dataRequest);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        
        [Theory]
        [InlineData("")]
        [InlineData("877.793.460-13")]
        [InlineData("89356026045")]
        public async Task Adicionar_Fornecedor_Com_Erro_Bad_Request(string documento)
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, documento);
            HttpContent dataRequest = ContentHelper.CreateJsonContent(fornecedorVM);

            // Act
            HttpResponseMessage response = await base.Client.PostAsync(CommonUri, dataRequest);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Fact]
        public async Task Atualizar_Fornecedor_Com_Sucesso()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "92076602020");
            Guid fornecedorId = fornecedorVM.Id;

            await AdicionarObjsParaTestes(fornecedorVM);

            fornecedorVM.Nome = "Produto Teste 2.0";
            fornecedorVM.Documento = "34065310000106";
            fornecedorVM.TipoFornecedor = 2;
            fornecedorVM.Ativo = false;

            HttpContent dataRequest = ContentHelper.CreateJsonContent(fornecedorVM);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/{fornecedorId}", dataRequest);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("XXXXX")]
        public async Task Atualizar_Fornecedor_Com_Erro_Bad_Request_Por_Propriedades_Validas(string documento)
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "92076602020");

            await AdicionarObjsParaTestes(fornecedorVM);

            fornecedorVM.Nome = "Produto Teste com proriedades inválidas";
            fornecedorVM.Documento = documento;
            fornecedorVM.TipoFornecedor = 2;

            HttpContent dataRequest = ContentHelper.CreateJsonContent(fornecedorVM);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/{fornecedorVM.Id}", dataRequest);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Theory]
        [InlineData("7904b20e-2feb-4a11-a912-8b368f79a37d")]
        public async Task Atualizar_Fornecedor_Com_Erro_Bad_Request_Por_Id_Rota_Diferente(string rotaId)
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "92076602020");
            HttpContent dataRequest = ContentHelper.CreateJsonContent(fornecedorVM);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/{rotaId}", dataRequest);

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("O ID informado não é o mesmo que foi passado na rota!", result.Errors);
        }
        
        [Fact]
        public async Task Atualizar_Fornecedor_Com_Erro_Not_Found()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "92076602020");
            HttpContent dataRequest = ContentHelper.CreateJsonContent(fornecedorVM);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/{fornecedorVM.Id}", dataRequest);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Atualizar_Endereco_Com_Sucesso()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "27552509090");

            await AdicionarObjsParaTestes(fornecedorVM);

            EnderecoViewModel enderecoVM = fornecedorVM.Endereco;

            enderecoVM.Cep = "77405060";
            enderecoVM.Logradouro = "Rua Eurídice Rodrigues Brito";
            enderecoVM.Bairro = "Setor Central";
            enderecoVM.Cidade = "Gurupi";
            enderecoVM.Estado = "TO";
            enderecoVM.Numero = "542";
            enderecoVM.Complemento = "Em algum local de Tocantins";

            HttpContent dataRequest = ContentHelper.CreateJsonContent(enderecoVM);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/atualizar-endereco/{enderecoVM.Id}", dataRequest);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Atualizar_Endereco_Com_Erro_Bad_Request_Por_Propriedades_Validas()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "92076602020");

            await AdicionarObjsParaTestes(fornecedorVM);

            EnderecoViewModel enderecoVM = fornecedorVM.Endereco;

            enderecoVM.Cep = "432";
            enderecoVM.Logradouro = string.Empty;
            enderecoVM.Bairro = string.Empty;
            enderecoVM.Cidade = string.Empty;
            enderecoVM.Estado = string.Empty;
            enderecoVM.Numero = string.Empty;
            enderecoVM.Complemento = string.Empty;

            HttpContent dataRequest = ContentHelper.CreateJsonContent(enderecoVM);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/atualizar-endereco/{enderecoVM.Id}", dataRequest);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Atualizar_Endereco_Com_Erro_Bad_Request_Por_Id_Rota_Diferente()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "92076602020");
            EnderecoViewModel enderecoVM = fornecedorVM.Endereco;
            Guid rotaId = Guid.NewGuid();

            HttpContent dataRequest = ContentHelper.CreateJsonContent(enderecoVM);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/atualizar-endereco/{rotaId}", dataRequest);

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("O ID informado não é o mesmo que foi passado na rota!", result.Errors);
        }

        [Fact]
        public async Task Atualizar_Endereco_Com_Erro_Not_Found()
        {
            // Arrange
            EnderecoViewModel enderecoVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "64248979098").Endereco;
            HttpContent dataRequest = ContentHelper.CreateJsonContent(enderecoVM);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/atualizar-endereco/{enderecoVM.Id}", dataRequest);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Remover_Fornecedor_Com_Sucesso()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "34446436082");

            await AdicionarObjsParaTestes(fornecedorVM);

            // Act
            HttpResponseMessage response = await base.Client.DeleteAsync($"{CommonUri}/{fornecedorVM.Id}");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        
        [Fact]
        public async Task Remover_Fornecedor_Com_Erro_Not_Found()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "34446436082");

            // Act
            HttpResponseMessage response = await base.Client.DeleteAsync($"{CommonUri}/{fornecedorVM.Id}");

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task AdicionarObjsParaTestes(params FornecedorViewModel[] objs)
        {
            using IServiceScope scope = base.Factory.Services.CreateScope();
            _fornecedorRepository = scope.ServiceProvider.GetRequiredService<IFornecedorRepository>();
            _mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

            foreach (FornecedorViewModel item in objs)
            {
                await _fornecedorRepository.Adicionar(_mapper.Map<Fornecedor>(item));
            }
        }
    }
}
