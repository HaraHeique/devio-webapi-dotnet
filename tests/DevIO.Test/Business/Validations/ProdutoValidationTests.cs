using DevIO.Business.Models;
using DevIO.Business.Validations;
using System;
using System.Linq;
using Xunit;

namespace DevIO.Test.Business.Validations
{
    public class ProdutoValidationTests
    {
        private readonly ProdutoValidation _sut;

        public ProdutoValidationTests()
        {
            _sut = new ProdutoValidation();
        }

        #region Propriedades Inválidas

        [Fact]
        public void Produto_Invalido_Com_Id_Vazio()
        {
            // Arrange
            Produto produto = ObterProdutoValido();
            produto.Id = Guid.Empty;

            string msgErroEsperada = $"O campo {nameof(produto.Id)} não pode ser vazio";

            // Act
            var validador = _sut.Validate(produto);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Single(validador.Errors);
            Assert.Equal(msgErroEsperada, validador.Errors.SingleOrDefault()?.ErrorMessage);
        }

        [Fact]
        public void Produto_Invalido_Com_Nome_Vazio()
        {
            // Arrange
            Produto produto = ObterProdutoValido();
            produto.Nome = string.Empty;

            string msgErroEsperada = $"O campo {nameof(produto.Nome)} precisa ser fornecido";

            // Act
            var validador = _sut.Validate(produto);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Produto_Invalido_Com_Nome_Menor_Dois_Caracteres()
        {
            // Arrange
            Produto produto = ObterProdutoValido();
            produto.Nome = "P";

            string msgErroEsperada = $"O campo {nameof(produto.Nome)} precisa ter entre 2 e 200 caracteres";

            // Act
            var validador = _sut.Validate(produto);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Produto_Invalido_Com_Nome_Maior_Duzentos_Caracteres()
        {
            // Arrange
            Produto produto = ObterProdutoValido();
            produto.Nome = @"
                Donec commodo a tellus at eleifend. Maecenas laoreet tempor viverra. Pellentesque faucibus ipsum ac molestie mollis. 
                Pellentesque vehicula vel dolor dapibus venenatis. In in euismod dui. Morbi aliquet urna quis finibus congue.
            ";

            string msgErroEsperada = $"O campo {nameof(produto.Nome)} precisa ter entre 2 e 200 caracteres";

            // Act
            var validador = _sut.Validate(produto);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Produto_Invalido_Com_Descricao_Vazia()
        {
            // Arrange
            Produto produto = ObterProdutoValido();
            produto.Descricao = string.Empty;

            string msgErroEsperada = $"O campo {nameof(produto.Descricao)} precisa ser fornecido";

            // Act
            var validador = _sut.Validate(produto);

            // Assert
            Assert.False(validador.IsValid);
            Assert.NotEmpty(validador.Errors);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Produto_Invalido_Com_Descricao_Menor_Dois_Caracteres()
        {
            // Arrange
            Produto produto = ObterProdutoValido();
            produto.Descricao = "X";

            string msgErroEsperada = $"O campo {nameof(produto.Descricao)} precisa ter entre 2 e 1000 caracteres";

            // Act
            var validador = _sut.Validate(produto);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Produto_Invalido_Com_Descricao_Maior_Mil_Caracteres()
        {
            // Arrange
            Produto produto = ObterProdutoValido();
            produto.Descricao = @"
                Maecenas in metus ut ligula sodales posuere a a est. Vivamus tristique accumsan turpis semper sagittis.
                Vestibulum sed neque tellus. Suspendisse ante nisi, ultricies id neque sed, pharetra tempus felis. Sed at est urna.
                Maecenas non pretium tellus, vel dictum leo. Suspendisse nec enim sit amet risus cursus consectetur.
                Praesent felis eros, vehicula nec lectus et, auctor condimentum lectus. Phasellus metus purus, elementum ut dolor quis,
                varius placerat quam. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia curae; Curabitur
                interdum tempus velit, vel laoreet nulla pretium a. Interdum et malesuada fames ac ante ipsum primis in faucibus.
                Donec eget tellus tincidunt, suscipit libero ac, sollicitudin urna. Donec euismod efficitur ipsum a placerat.
                Phasellus euismod odio in lacus varius dapibus. Duis tempor egestas augue at suscipit. Nunc posuere, erat at luctus tincidunt,
                felis sem posuere justo, at iaculis enim justo et nisi. Phasellus a nibh hendrerit, vehicula ante eget, fermentum turpis.
                Nunc scelerisque volutpat ultricies. Aenean at venenatis ipsum. Integer ut urna in mauris vulputate auctor.
            ";

            string msgErroEsperada = $"O campo {nameof(produto.Descricao)} precisa ter entre 2 e 1000 caracteres";

            // Act
            var validador = _sut.Validate(produto);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Produto_Invalido_Com_Valor_Menor_Ou_Igual_Zero()
        {
            // Arrange
            Produto produto = ObterProdutoValido();
            produto.Valor = 0M;

            string msgErroEsperada = $"O campo {nameof(produto.Valor)} precisa ser maior que 0";

            // Act
            var validador = _sut.Validate(produto);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        #endregion

        #region Propriedades Válidas

        [Fact]
        public void Produto_Com_Todas_Propriedades_Validas()
        {
            // Arrange
            Produto produto = ObterProdutoValido();

            // Act
            var validador = _sut.Validate(produto);

            // Assert
            Assert.True(validador.IsValid);
            Assert.Empty(validador.Errors);
        }

        #endregion

        #region Dados de Teste

        private Produto ObterProdutoValido()
        {
            return new Produto
            {
                FornecedorId = Guid.NewGuid(),
                Nome = "Produto Teste 2",
                Descricao = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. In posuere nulla non justo sodales, ut.",
                Imagem = string.Empty,
                Valor = (decimal)new Random().NextDouble() * 10 + 2000,
                Ativo = true
            };
        }

        #endregion
    }
}
