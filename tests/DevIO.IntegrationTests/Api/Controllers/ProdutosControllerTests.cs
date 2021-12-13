using AutoMapper;
using DevIO.Api;
using DevIO.Api.ViewModels;
using DevIO.Business.Interfaces.Repositories;
using DevIO.Business.Models;
using DevIO.IntegrationTests.Helpers;
using DevIO.IntegrationTests.Setups.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DevIO.IntegrationTests.Api.Controllers
{
    public class ProdutosControllerTests : IntegrationTestsFixture
    {
        private const string CommonUri = "api/v2/produtos";

        public ProdutosControllerTests(ApiWebApplicationFactory<Startup> factory) : base(factory) { }

        [Fact]
        public async Task Obter_Todos_Produtos_Com_Sucesso()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(2, "34477026000147");

            await AdicionarObjsParaTestes(fornecedorVM,
                ProdutoViewModelTestsHelper.ObterInstancia(fornecedorVM),
                ProdutoViewModelTestsHelper.ObterInstancia(fornecedorVM),
                ProdutoViewModelTestsHelper.ObterInstancia(fornecedorVM)
            );

            // Act
            HttpResponseMessage response = await base.Client.GetAsync(CommonUri);

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result.Data);
        }
        
        [Fact]
        public async Task Obter_Produto_Por_Id_Com_Sucesso()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(2, "17839729000159");
            ProdutoViewModel produtoVM = ProdutoViewModelTestsHelper.ObterInstancia(fornecedorVM);

            await AdicionarObjsParaTestes(fornecedorVM, produtoVM);

            // Act
            HttpResponseMessage response = await base.Client.GetAsync($"{CommonUri}/{produtoVM.Id}");

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result.Data);
        }
        
        [Fact]
        public async Task Adicionar_Produto_Com_Imagem_Base64_Com_Sucesso()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "49157344043");
            await AdicionarObjsParaTestes(fornecedorVM);

            ProdutoViewModel produtoVM = ProdutoViewModelTestsHelper.ObterInstancia(fornecedorVM);
            HttpContent dataRequest = ContentHelper.CreateJsonContent(produtoVM);

            // Act
            HttpResponseMessage response = await base.Client.PostAsync(CommonUri, dataRequest);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Clean up image
            await DeletarImagemProduto(produtoVM.Id);
        }
        
        [Fact]
        public async Task Adicionar_Produto_Com_Imagem_Base64_Com_Erro_Por_Propriedades_Invalidas()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "49157344043");
            await AdicionarObjsParaTestes(fornecedorVM);

            ProdutoViewModel produtoVM = ProdutoViewModelTestsHelper.ObterInstancia(fornecedorVM);

            produtoVM.Descricao = string.Empty;
            produtoVM.Nome = string.Empty;

            HttpContent dataRequest = ContentHelper.CreateJsonContent(produtoVM);

            // Act
            HttpResponseMessage response = await base.Client.PostAsync(CommonUri, dataRequest);

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotEmpty(result.Errors);
            Assert.Equal(4, result.Errors.Length);

            // Clean up image
            await DeletarImagemProduto(produtoVM.Id);
        }
        
        [Fact]
        public async Task Adicionar_Produto_Com_Imagem_Base64_Invalida()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "49157344043");
            await AdicionarObjsParaTestes(fornecedorVM);

            ProdutoViewModel produtoVM = ProdutoViewModelTestsHelper.ObterInstancia(fornecedorVM);

            produtoVM.ImagemUpload = string.Empty;

            HttpContent dataRequest = ContentHelper.CreateJsonContent(produtoVM);

            // Act
            HttpResponseMessage response = await base.Client.PostAsync(CommonUri, dataRequest);

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Single(result.Errors);
            Assert.Contains("Favor fornecer uma imagem para este produto!", result.Errors);

            // Clean up image
            await DeletarImagemProduto(produtoVM.Id);
        }
        
        [Fact]
        public async Task Adicionar_Produto_Com_Imagem_Grande_Com_Sucesso()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "49157344043");
            await AdicionarObjsParaTestes(fornecedorVM);

            (ProdutoImagemViewModel produtoVM, Stream imageStream) = ProdutoViewModelTestsHelper.ObterInstanciaParaImagemGrande(fornecedorVM);
            HttpContent dataRequest = CriarFormDataRequest(produtoVM, imageStream);

            // Act
            HttpResponseMessage response = await base.Client.PostAsync($"{CommonUri}/adicionar-produto-imagem", dataRequest);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Clean up
            await DeletarImagemProduto(produtoVM.Id);
            await imageStream.DisposeAsync();
            dataRequest.Dispose();
        }

        [Fact]
        public async Task Adicionar_Produto_Com_Imagem_Grande_Com_Erro_Por_Propriedades_Invalidas()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(2, "08340579000143");
            await AdicionarObjsParaTestes(fornecedorVM);

            (ProdutoImagemViewModel produtoVM, Stream imageStream) = ProdutoViewModelTestsHelper.ObterInstanciaParaImagemGrande(fornecedorVM);

            produtoVM.Descricao = string.Empty;
            produtoVM.Nome = string.Empty;

            HttpContent dataRequest = CriarFormDataRequest(produtoVM, imageStream);

            // Act
            HttpResponseMessage response = await base.Client.PostAsync($"{CommonUri}/adicionar-produto-imagem", dataRequest);

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotEmpty(result.Errors);

            // Clean up
            await DeletarImagemProduto(produtoVM.Id);
            await imageStream.DisposeAsync();
            dataRequest.Dispose();
        }

        [Fact]
        public async Task Atualizar_Produto_Com_Imagem_Base64_Com_Sucesso()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "49157344043");
            ProdutoViewModel produtoVM = ProdutoViewModelTestsHelper.ObterInstancia(fornecedorVM, $"{base.Factory.Env.WebRootPath}/img");

            await AdicionarObjsParaTestes(fornecedorVM, produtoVM);

            produtoVM.Nome = "Produto de teste com atualização";
            produtoVM.Valor = 321.43M;
            produtoVM.Descricao = "É o produto teste não um lorem";
            produtoVM.Ativo = false;

            HttpContent dataRequest = ContentHelper.CreateJsonContent(produtoVM);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/{produtoVM.Id}", dataRequest);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Clean up
            await DeletarImagemProduto(produtoVM.Id);
        }
        
        [Fact]
        public async Task Atualizar_Produto_Com_Imagem_Base64_Com_Erro_Por_Propriedades_Invalidas()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "49157344043");
            ProdutoViewModel produtoVM = ProdutoViewModelTestsHelper.ObterInstancia(fornecedorVM, $"{base.Factory.Env.WebRootPath}/img");

            await AdicionarObjsParaTestes(fornecedorVM, produtoVM);

            produtoVM.Nome = "x";
            produtoVM.Descricao = string.Empty;

            HttpContent dataRequest = ContentHelper.CreateJsonContent(produtoVM);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/{produtoVM.Id}", dataRequest);

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotEmpty(result.Errors);

            // Clean up
            await DeletarImagemProduto(produtoVM.Id);
        }
        
        [Fact]
        public async Task Atualizar_Produto_Com_Imagem_Base64_Com_Erro_Por_Id_Rota_Diferente()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "49157344043");
            ProdutoViewModel produtoVM = ProdutoViewModelTestsHelper.ObterInstancia(fornecedorVM);
            Guid rotaId = Guid.NewGuid();

            HttpContent dataRequest = ContentHelper.CreateJsonContent(produtoVM);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/{rotaId}", dataRequest);

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("O ID informado não é o mesmo que foi passado na rota!", result.Errors);

            // Clean up
            await DeletarImagemProduto(produtoVM.Id);
        }
        
        [Fact]
        public async Task Atualizar_Produto_Com_Imagem_Base64_Nao_Encontrado()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "49157344043");
            ProdutoViewModel produtoVM = ProdutoViewModelTestsHelper.ObterInstancia(fornecedorVM);

            HttpContent dataRequest = ContentHelper.CreateJsonContent(produtoVM);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/{produtoVM.Id}", dataRequest);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // Clean up
            await DeletarImagemProduto(produtoVM.Id);
        }
        
        [Fact]
        public async Task Atualizar_Produto_Com_Imagem_Grande_Com_Sucesso()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(2, "40405549000180");
            (ProdutoImagemViewModel produtoVM, Stream imageStream) = ProdutoViewModelTestsHelper.ObterInstanciaParaImagemGrande(fornecedorVM, $"{base.Factory.Env.WebRootPath}/img");

            await AdicionarObjsParaTestes(fornecedorVM, ProdutoViewModelTestsHelper.ObterInstanciaPorTransformacao(produtoVM));

            produtoVM.Nome = "Produto de teste com atualização para imagem grande";
            produtoVM.Valor = 999.43M;
            produtoVM.Descricao = "É o produto teste não um lorem para imagem grande!";
            produtoVM.Ativo = false;

            HttpContent dataRequest = CriarFormDataRequest(produtoVM, imageStream);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/atualizar-produto-imagem/{produtoVM.Id}", dataRequest);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Clean up
            await DeletarImagemProduto(produtoVM.Id);
            await imageStream.DisposeAsync();
            dataRequest.Dispose();
        }

        [Fact]
        public async Task Atualizar_Produto_Com_Imagem_Grande_Com_Erro_Por_Propriedades_Invalidas()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(2, "40405549000180");
            (ProdutoImagemViewModel produtoVM, Stream imageStream) = ProdutoViewModelTestsHelper.ObterInstanciaParaImagemGrande(fornecedorVM, $"{base.Factory.Env.WebRootPath}/img");

            await AdicionarObjsParaTestes(fornecedorVM, ProdutoViewModelTestsHelper.ObterInstanciaPorTransformacao(produtoVM));

            produtoVM.Nome = string.Empty;
            produtoVM.Valor = 999.43M;
            produtoVM.Descricao = string.Empty;

            HttpContent dataRequest = CriarFormDataRequest(produtoVM, imageStream);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/atualizar-produto-imagem/{produtoVM.Id}", dataRequest);

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotEmpty(result.Errors);

            // Clean up
            await DeletarImagemProduto(produtoVM.Id);
            await imageStream.DisposeAsync();
            dataRequest.Dispose();
        }

        [Fact]
        public async Task Atualizar_Produto_Com_Imagem_Grande_Com_Erro_Por_Id_Rota_Diferente()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(2, "40405549000180");
            (ProdutoImagemViewModel produtoVM, Stream imageStream) = ProdutoViewModelTestsHelper.ObterInstanciaParaImagemGrande(fornecedorVM);
            Guid rotaId = Guid.NewGuid();

            HttpContent dataRequest = CriarFormDataRequest(produtoVM, imageStream);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/atualizar-produto-imagem/{rotaId}", dataRequest);

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Contains("O ID informado não é o mesmo que foi passado na rota!", result.Errors);

            // Clean up
            await DeletarImagemProduto(produtoVM.Id);
            await imageStream.DisposeAsync();
            dataRequest.Dispose();
        }

        [Fact]
        public async Task Atualizar_Produto_Com_Imagem_Grande_Nao_Encontrado()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(2, "40405549000180");
            (ProdutoImagemViewModel produtoVM, Stream imageStream) = ProdutoViewModelTestsHelper.ObterInstanciaParaImagemGrande(fornecedorVM);

            HttpContent dataRequest = CriarFormDataRequest(produtoVM, imageStream);

            // Act
            HttpResponseMessage response = await base.Client.PutAsync($"{CommonUri}/atualizar-produto-imagem/{produtoVM.Id}", dataRequest);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // Clean up
            await DeletarImagemProduto(produtoVM.Id);
            await imageStream.DisposeAsync();
            dataRequest.Dispose();
            response.Dispose();
        }
        
        [Fact]
        public async Task Remover_Produto_Com_Sucesso()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "86710738078");
            ProdutoViewModel produtoVM = ProdutoViewModelTestsHelper.ObterInstancia(fornecedorVM, $"{base.Factory.Env.WebRootPath}/img");

            await AdicionarObjsParaTestes(fornecedorVM, produtoVM);

            // Act
            HttpResponseMessage response = await base.Client.DeleteAsync($"{CommonUri}/{produtoVM.Id}");

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Clean up
            await DeletarImagemProduto(produtoVM.Id);
            response.Dispose();
        }
        
        [Fact]
        public async Task Remover_Produto_Nao_Encontrado()
        {
            // Arrange
            FornecedorViewModel fornecedorVM = FornecedorViewModelTestsHelper.ObterInstancia(1, "86710738078");
            ProdutoViewModel produtoVM = ProdutoViewModelTestsHelper.ObterInstancia(fornecedorVM);

            // Act
            HttpResponseMessage response = await base.Client.DeleteAsync($"{CommonUri}/{produtoVM.Id}");

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            // Clean up
            await DeletarImagemProduto(produtoVM.Id);
            response.Dispose();
        }

        private async Task AdicionarObjsParaTestes(FornecedorViewModel fornecedorVM, params ProdutoViewModel[] objs)
        {
            using IServiceScope scope = base.Factory.Services.CreateScope();
            var fornecedorRepository = scope.ServiceProvider.GetRequiredService<IFornecedorRepository>();
            var produtoRepository = scope.ServiceProvider.GetRequiredService<IProdutoRepository>();
            var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

            await fornecedorRepository.Adicionar(mapper.Map<Fornecedor>(fornecedorVM));

            foreach (ProdutoViewModel item in objs)
            {
                await produtoRepository.Adicionar(mapper.Map<Produto>(item));
            }
        }

        private async Task DeletarImagemProduto(Guid produtoVmId)
        {
            using IServiceScope scope = base.Factory.Services.CreateScope();
            var produtoRepository = scope.ServiceProvider.GetRequiredService<IProdutoRepository>();

            string imagem = (await produtoRepository.Buscar(p => p.Id == produtoVmId))?.Imagem;

            if (string.IsNullOrEmpty(imagem)) return;

            string wwwrootPath = base.Factory.Env.WebRootPath;
            string imagemAbsolutePath = Path.Combine(wwwrootPath, "img", imagem);

            if (File.Exists(imagemAbsolutePath)) File.Delete(imagemAbsolutePath);
        }

        private HttpContent CriarFormDataRequest(ProdutoImagemViewModel produtoVM, Stream imageStream)
        {
            var content = new MultipartFormDataContent
            {
                // file
                { new StreamContent(imageStream), nameof(produtoVM.ImagemUpload), produtoVM.Imagem },

                // payload
                { new StringContent(produtoVM.Id.ToString()), nameof(produtoVM.Id) },
                { new StringContent(produtoVM.FornecedorId.ToString()), nameof(produtoVM.FornecedorId) },
                { new StringContent(produtoVM.Nome), nameof(produtoVM.Nome) },
                { new StringContent(produtoVM.Descricao), nameof(produtoVM.Descricao) },
                { new StringContent(produtoVM.Valor.ToString(new CultureInfo("pt-BR"))), nameof(produtoVM.Valor) },
                { new StringContent(produtoVM.Ativo.ToString().ToLower()), nameof(produtoVM.Ativo) }
            };

            return content;
        }
    }
}
