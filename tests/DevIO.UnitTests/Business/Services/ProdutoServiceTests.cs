using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Interfaces.Repositories;
using DevIO.Business.Models;
using DevIO.Business.Notifications;
using DevIO.Business.Services;
using NSubstitute;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace DevIO.UnitTests.Business.Services
{
    public class ProdutoServiceTests
    {
        private readonly ProdutoService _sut;

        private readonly IProdutoRepository _produtoRepository;
        private readonly INotificador _notificador;

        public ProdutoServiceTests()
        {
            _produtoRepository = Substitute.For<IProdutoRepository>();
            _notificador = new Notificador();

            _sut = new ProdutoService(_produtoRepository, _notificador);
        }

        #region Adicionar Produto

        [Fact]
        public async Task Adicionar_Produto_Com_Propriedades_Invalidas()
        {
            // Arrange
            Produto produto = ObterInstanciaProduto();
            produto.Id = Guid.Empty;
            produto.Descricao = "X";
            produto.Nome = string.Empty;

            int qntNotificacoesEsperadas = 4;
            string msgErroIdEsperada = $"O campo {nameof(produto.Id)} não pode ser vazio";
            string msgErroNomeEsperada = $"O campo {nameof(produto.Nome)} precisa ser fornecido";
            string msgErroNomeEsperada2 = $"O campo {nameof(produto.Nome)} precisa ter entre 2 e 200 caracteres";
            string msgErroDescricaoEsperada = $"O campo {nameof(produto.Descricao)} precisa ter entre 2 e 1000 caracteres";

            var msgErrosEsperadas = new string[] { msgErroIdEsperada, msgErroNomeEsperada, msgErroNomeEsperada2, msgErroDescricaoEsperada };

            // Act
            await _sut.Adicionar(produto);

            // Assert
            var notificacoes = _notificador.ObterNotificacoes();

            Assert.True(_notificador.TemNotificacao());
            Assert.NotEmpty(notificacoes);
            Assert.Equal(qntNotificacoesEsperadas, notificacoes.Count);
            Assert.Equal(msgErrosEsperadas, notificacoes.Select(n => n.Mensagem));
        }

        [Fact]
        public async Task Adicionar_Produto_Valido()
        {
            // Arrange
            Produto produto = ObterInstanciaProduto();

            // Act
            await _sut.Adicionar(produto);

            // Assert
            Assert.False(_notificador.TemNotificacao());
            Assert.Empty(_notificador.ObterNotificacoes());
        }

        #endregion

        #region Atualizar Produto

        [Fact]
        public async Task Atualizar_Produto_Com_Propriedades_Invalidas()
        {
            // Arrange
            Produto produto = ObterInstanciaProduto();
            produto.Id = Guid.Empty;
            produto.Valor = 0M;

            int qntNotificacoesEsperadas = 2;

            var msgErrosEsperadas = new string[]
            {
                $"O campo {nameof(produto.Id)} não pode ser vazio",
                $"O campo {nameof(produto.Valor)} precisa ser maior que 0"
            };

            // Act
            await _sut.Atualizar(produto);

            // Assert
            var notificacoes = _notificador.ObterNotificacoes();

            Assert.NotEmpty(notificacoes);
            Assert.True(_notificador.TemNotificacao());
            Assert.Equal(msgErrosEsperadas, notificacoes.Select(n => n.Mensagem));
            Assert.Equal(qntNotificacoesEsperadas, notificacoes.Count);
        }

        [Fact]
        public async Task Atualizar_Produto_Valido()
        {
            // Arrange
            Produto produto = ObterInstanciaProduto();

            // Act
            await _sut.Atualizar(produto);

            // Assert
            Assert.False(_notificador.TemNotificacao());
            Assert.Empty(_notificador.ObterNotificacoes());
        }

        #endregion

        #region Remover Produto

        [Fact]
        public async Task Remover_Produto_Valido()
        {
            // Arrange
            Produto produto = ObterInstanciaProduto();
            Guid produtoId = produto.Id;

            // Act
            await _sut.Remover(produtoId);

            // Assert
            await _produtoRepository.Received().Remover(produtoId);
        }

        #endregion

        #region Dispose Service

        [Fact]
        public void Deve_Liberar_Recursos_Service()
        {
            // Arrange

            // Act
            _sut.Dispose();

            // Assert
            _produtoRepository.Received().Dispose();
        }

        #endregion

        #region Dados de Teste

        private Produto ObterInstanciaProduto()
        {
            return new Produto
            {
                FornecedorId = Guid.NewGuid(),
                Nome = "Produto Teste",
                Descricao = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin vitae.",
                Imagem = string.Empty,
                Valor = (decimal)new Random().NextDouble() * 10 + 5000,
                Ativo = true
            };
        }

        #endregion
    }
}
