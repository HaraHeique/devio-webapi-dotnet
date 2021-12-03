using DevIO.Business.Enums;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Repositories;
using DevIO.Business.Models;
using DevIO.Business.Notifications;
using DevIO.Business.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using System.Linq.Expressions;
using System.Linq;

namespace DevIO.UnitTests.Business.Services
{
    public class FornecedorServiceTests
    {
        private readonly FornecedorService _sut;

        private readonly IFornecedorRepository _fornecedorRepositoryMock;
        private readonly IEnderecoRepository _enderecoRepositoryMock;
        private readonly INotificador _notificador;

        public FornecedorServiceTests()
        {
            _fornecedorRepositoryMock = Substitute.For<IFornecedorRepository>();
            _enderecoRepositoryMock = Substitute.For<IEnderecoRepository>();
            _notificador = new Notificador();

            _sut = new FornecedorService(_fornecedorRepositoryMock, _enderecoRepositoryMock, _notificador);
        }

        #region Adicionar Fornecedor

        [Fact]
        public async Task Adicionar_Fornecedor_Com_Propriedade_Invalida()
        {
            // Arrange
            Fornecedor fornecedor = ObterInstanciaFornecedor();
            fornecedor.Id = Guid.Empty;

            // Act
            await _sut.Adicionar(fornecedor);

            // Assert
            Assert.True(_notificador.TemNotificacao());
        }

        [Fact]
        public async Task Adicionar_Fornecedor_Invalido_Com_Documento_Repetido()
        {
            // Arrange
            Fornecedor fornecedor = ObterInstanciaFornecedor();

            var fornecedoresDocumentoRepetido = new List<Fornecedor> { fornecedor };
            _fornecedorRepositoryMock
                .BuscarTodos(Arg.Any<Expression<Func<Fornecedor, bool>>>())
                .Returns(fornecedoresDocumentoRepetido);

            // Act
            await _sut.Adicionar(fornecedor);

            // Assert
            Assert.True(_notificador.TemNotificacao());
            Assert.Contains(fornecedor, fornecedoresDocumentoRepetido);
        }

        [Fact]
        public async Task Adicionar_Fornecedor_Valido()
        {
            // Arrange
            Fornecedor fornecedor = ObterInstanciaFornecedor();

            // Act
            await _sut.Adicionar(fornecedor);

            // Assert
            Assert.False(_notificador.TemNotificacao());
        }

        #endregion

        #region Atualizar Fornecedor

        [Fact]
        public async Task Atualizar_Fornecedor_Com_Propriedade_Invalida()
        {
            // Arrange
            Fornecedor fornecedor = ObterInstanciaFornecedor();
            fornecedor.Id = Guid.Empty;

            // Act
            await _sut.Atualizar(fornecedor);

            // Assert
            Assert.True(_notificador.TemNotificacao());
        }

        [Fact]
        public async Task Atualizar_Fornecedor_Invalido_Com_Documento_Repetido()
        {
            // Arrange
            Fornecedor fornecedor = ObterInstanciaFornecedor();

            var fornecedoresDocumentoRepetido = new List<Fornecedor> { fornecedor };
            _fornecedorRepositoryMock
                .BuscarTodos(Arg.Any<Expression<Func<Fornecedor, bool>>>())
                .Returns(fornecedoresDocumentoRepetido);

            // Act
            await _sut.Atualizar(fornecedor);

            // Assert
            Assert.True(_notificador.TemNotificacao());
            Assert.Contains(fornecedor, fornecedoresDocumentoRepetido);
        }

        [Fact]
        public async Task Atualizar_Fornecedor_Valido()
        {
            // Arrange
            Fornecedor fornecedor = ObterInstanciaFornecedor();

            // Act
            await _sut.Atualizar(fornecedor);

            // Assert
            Assert.False(_notificador.TemNotificacao());
        }

        #endregion

        #region Atualizar Endereco

        [Fact]
        public async Task Atualizar_Endereco_Com_Propriedade_Invalida()
        {
            // Arrange
            Endereco endereco = ObterInstanciaFornecedor().Endereco;
            endereco.Id = Guid.Empty;

            // Act
            await _sut.AtualizarEndereco(endereco);

            // Assert
            Assert.True(_notificador.TemNotificacao());
        }

        [Fact]
        public async Task Atualizar_Endereco_Valido()
        {
            // Arrange
            Endereco endereco = ObterInstanciaFornecedor().Endereco;

            // Act
            await _sut.AtualizarEndereco(endereco);

            // Assert
            Assert.False(_notificador.TemNotificacao());
        }

        #endregion

        #region Remover Fornecedor

        [Fact]
        public async Task Remover_Fornecedor_Com_Produtos()
        {
            // Arrange
            Fornecedor fornecedorComProdutos = ObterInstanciaFornecedorComProdutos();
            Guid fornecedorId = fornecedorComProdutos.Id;

            _fornecedorRepositoryMock
                .ObterFornecedorProdutosEndereco(fornecedorId)
                .Returns(fornecedorComProdutos);

            string msgNotificaoEsperada = "O fornecedor possui produtos cadastrados!";

            // Act
            await _sut.Remover(fornecedorId);

            // Assert
            Assert.True(_notificador.TemNotificacao());
            Assert.Contains(msgNotificaoEsperada, _notificador.ObterNotificacoes().Select(n => n.Mensagem));
        }

        [Fact]
        public async Task Remover_Fornecedor_Valido()
        {
            // Arrange
            Endereco endereco = ObterInstanciaFornecedor().Endereco;
            Guid enderecoId = endereco.Id;
            Guid fornecedorId = endereco.FornecedorId;

            _fornecedorRepositoryMock
                .ObterFornecedorProdutosEndereco(fornecedorId)
                .Returns(new Fornecedor { Produtos = new List<Produto>() });

            _enderecoRepositoryMock
                .ObterEnderecoPorFornecedor(fornecedorId)
                .Returns(endereco);

            // Act
            await _sut.Remover(fornecedorId);

            // Assert
            Assert.False(_notificador.TemNotificacao());
            Assert.Empty(_notificador.ObterNotificacoes());

            await _enderecoRepositoryMock.Received().Remover(enderecoId);
            await _fornecedorRepositoryMock.Received().Remover(fornecedorId);
        }

        #endregion

        #region Dispose Service

        [Fact]
        public void Deve_Liberar_Recursos_Do_Service()
        {
            // Arrange

            // Act
            _sut.Dispose();

            // Assert
            _enderecoRepositoryMock.Received().Dispose();
            _fornecedorRepositoryMock.Received().Dispose();
        }

        #endregion

        #region Dados de Teste

        private Fornecedor ObterInstanciaFornecedor()
        {
            Guid fornecedorId = Guid.NewGuid();

            return new Fornecedor
            {
                Id = fornecedorId,
                Ativo = true,
                Documento = "35541185084",
                Nome = "Fornecedor Teste",
                TipoFornecedor = TipoFornecedor.PessoaFisica,
                Endereco = new Endereco
                {
                    Bairro = "Loteamento Brasiliense",
                    Cep = "75712285",
                    Cidade = "Catalão",
                    Complemento = "Em algum lugar",
                    Estado = "GO",
                    FornecedorId = fornecedorId,
                    Logradouro = "Rua",
                    Numero = "321"
                }
            };
        }

        private Fornecedor ObterInstanciaFornecedorComProdutos()
        {
            Fornecedor fornecedor = ObterInstanciaFornecedor();
            fornecedor.Produtos = new List<Produto>()
            {
                new Produto
                {
                    FornecedorId = fornecedor.Id,
                    Nome = "Produto Teste",
                    Descricao = "Lorem ipsum dolor sit amet.",
                    Imagem = string.Empty,
                    DataCadastro = DateTime.Now.AddDays(-7),
                    Valor = (decimal)new Random().NextDouble() * 10 + 1000,
                    Ativo = true
                }
            };

            return fornecedor;
        }

        #endregion
    }
}
