using DevIO.Business.Models;
using DevIO.Business.Validations;
using System;
using System.Linq;
using Xunit;

namespace DevIO.Test.Business.Validations
{
    public class EnderecoValidationTests
    {
        private readonly EnderecoValidation _sut;

        public EnderecoValidationTests()
        {
            _sut = new EnderecoValidation();
        }

        #region Propriedades Válidas

        [Fact]
        public void Endereco_Invalido_Com_Id_Vazio()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Id = Guid.Empty;

            string msgErroEsperada = $"O campo {nameof(endereco.Id)} não pode ser vazio";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Single(validador.Errors);
            Assert.Equal(msgErroEsperada, validador.Errors.FirstOrDefault()?.ErrorMessage);
        }

        [Fact]
        public void Endereco_Invalido_Com_Logradouro_Vazio()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Logradouro = string.Empty;

            string msgErroEsperada = $"O campo {nameof(endereco.Logradouro)} precisa ser fornecido";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Produto_Invalido_Com_Logradouro_Menor_Dois_Caracteres()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Logradouro = "Y";

            string msgErroEsperada = $"O campo {nameof(endereco.Logradouro)} precisa ter entre 2 e 200 caracteres";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Produto_Invalido_Com_Logradouro_Maior_Duzentos_Caracteres()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Logradouro = @"
                Donec commodo a tellus at eleifend. Maecenas laoreet tempor viverra. Pellentesque faucibus ipsum ac molestie mollis. 
                Pellentesque vehicula vel dolor dapibus venenatis. In in euismod dui.
            ";

            string msgErroEsperada = $"O campo {nameof(endereco.Logradouro)} precisa ter entre 2 e 200 caracteres";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Endereco_Invalido_Com_Bairro_Vazio()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Bairro = string.Empty;

            string msgErroEsperada = $"O campo {nameof(endereco.Bairro)} precisa ser fornecido";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Produto_Invalido_Com_Bairro_Menor_Dois_Caracteres()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Bairro = "B";

            string msgErroEsperada = $"O campo {nameof(endereco.Bairro)} precisa ter entre 2 e 100 caracteres";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Produto_Invalido_Com_Logradouro_Maior_Cem_Caracteres()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Bairro = @"
                Morbi mollis maximus mauris, sit amet condimentum neque lobortis sit amet. Sed scelerisque odio eget pretium sagittis.
                Aliquam eu fermentum neque, vitae pharetra sapien.
            ".Trim();

            string msgErroEsperada = $"O campo {nameof(endereco.Bairro)} precisa ter entre 2 e 100 caracteres";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Endereco_Invalido_Com_Cep_Vazio()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Cep = string.Empty;

            string msgErroEsperada = $"O campo {nameof(endereco.Cep)} precisa ser fornecido";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Theory]
        [InlineData("")]
        [InlineData("768058")]
        [InlineData("975476033232")]
        [InlineData("03")]
        public void Produto_Invalido_Com_Logradouro_Diferente_Oito_Caracteres(string cepInvalido)
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Cep = cepInvalido;

            string msgErroEsperada = $"O campo {nameof(endereco.Cep)} precisa ter 8 caracteres";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Endereco_Invalido_Com_Cidade_Vazia()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Cidade = string.Empty;

            string msgErroEsperada = $"O campo {nameof(endereco.Cidade)} precisa ser fornecida";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Produto_Invalido_Com_Cidade_Menor_Dois_Caracteres()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Cidade = "C";

            string msgErroEsperada = $"O campo {nameof(endereco.Cidade)} precisa ter entre 2 e 100 caracteres";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Produto_Invalido_Com_Cidade_Maior_Cem_Caracteres()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Cidade = @"
                Morbi mollis maximus mauris, sit amet condimentum neque lobortis sit amet. Sed scelerisque odio eget pretium sagittis.
                Aliquam eu fermentum neque.
            ".Trim();

            string msgErroEsperada = $"O campo {nameof(endereco.Cidade)} precisa ter entre 2 e 100 caracteres";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }
        
        [Fact]
        public void Endereco_Invalido_Com_Estado_Vazio()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Estado = string.Empty;

            string msgErroEsperada = $"O campo {nameof(endereco.Estado)} precisa ser fornecido";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Produto_Invalido_Com_Estado_Menor_Dois_Caracteres()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Estado = "E";

            string msgErroEsperada = $"O campo {nameof(endereco.Estado)} precisa ter entre 2 e 50 caracteres";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Produto_Invalido_Com_Cidade_Maior_Ciquenta_Caracteres()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Estado = "Vivamus imperdiet, massa in volutpat posuere, odio ex commodo neque.";

            string msgErroEsperada = $"O campo {nameof(endereco.Estado)} precisa ter entre 2 e 50 caracteres";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Endereco_Invalido_Com_Numero_Vazio()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Numero = string.Empty;

            string msgErroEsperada = $"O campo {nameof(endereco.Numero)} precisa ser fornecido";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Theory]
        [InlineData("123123123123123213463543059849039841239843902894823572309483294893250723")]
        [InlineData("1231231231231232134635430598490398412398439028453136")]
        public void Produto_Invalido_Com_Numero_Maior_Ciquenta_Caracteres(string enderecoNumeroInvalido)
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();
            endereco.Numero = enderecoNumeroInvalido;

            string msgErroEsperada = $"O campo {nameof(endereco.Numero)} precisa ter entre 1 e 50 caracteres";

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        #endregion

        #region Propriedades Inválidas

        [Fact]
        public void Endereco_Com_Todas_Propriedades_Validas()
        {
            // Arrange
            Endereco endereco = ObterEnderecoValido();

            // Act
            var validador = _sut.Validate(endereco);

            // Assert
            Assert.True(validador.IsValid);
            Assert.Empty(validador.Errors);
        }

        #endregion

        #region Dados de Teste

        private Endereco ObterEnderecoValido()
        {
            return new Endereco
            {
                FornecedorId = Guid.NewGuid(),
                Logradouro = "Industrial",
                Numero = "542",
                Complemento = "Próximo de algum local de Manaus",
                Cep = "69007032",
                Bairro = "Distrito Industrial II",
                Cidade = "Manaus",
                Estado = "AM"
            };
        }

        #endregion
    }
}
