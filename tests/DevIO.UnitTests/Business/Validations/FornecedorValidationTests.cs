using DevIO.Business.Enums;
using DevIO.Business.Models;
using DevIO.Business.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace DevIO.UnitTests.Business.Validations
{
    public class FornecedorValidationTests
    {
        private readonly FornecedorValidation _sut;

        public FornecedorValidationTests()
        {
            _sut = new FornecedorValidation();
        }

        #region Propriedades Inválidas

        [Fact]
        public void Fornecedor_Invalido_Com_Id_Vazio()
        {
            // Arrange
            Fornecedor fornecedor = ObterFornecedorValido();
            fornecedor.Id = Guid.Empty;

            string msgErroEsperada = $"O campo {nameof(fornecedor.Id)} não pode ser vazio";

            // Act
            var validador = _sut.Validate(fornecedor);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Equal(msgErroEsperada, validador.Errors.SingleOrDefault()?.ErrorMessage);
        }

        [Fact]
        public void Fornecedor_Invalido_Com_Nome_Vazio()
        {
            // Arrange
            Fornecedor fornecedor = ObterFornecedorValido();
            fornecedor.Nome = string.Empty;

            string msgErroEsperada = $"O campo {nameof(fornecedor.Nome)} precisa ser fornecido.";

            // Act
            var validador = _sut.Validate(fornecedor);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Fornecedor_Invalido_Com_Nome_Menor_Dois_Caracteres()
        {
            // Arrange
            Fornecedor fornecedor = ObterFornecedorValido();
            fornecedor.Nome = "X";

            string msgErroEsperada = $"O campo {nameof(fornecedor.Nome)} precisa ter entre 2 e 100 caracteres.";

            // Act
            var validador = _sut.Validate(fornecedor);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Fact]
        public void Fornecedor_Invalido_Com_Nome_Maior_Cem_Caracteres()
        {
            // Arrange
            Fornecedor fornecedor = ObterFornecedorValido();
            fornecedor.Nome = @"
                Lorem Ipsum is simply dummy text of the printing and typesetting industry.
                Lorem Ipsum has been the industry's standard dummy text ever since the 1500s.
            ";

            string msgErroEsperada = $"O campo {nameof(fornecedor.Nome)} precisa ter entre 2 e 100 caracteres.";

            // Act
            var validador = _sut.Validate(fornecedor);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
        }

        [Theory]
        [InlineData("2694585604")]
        [InlineData("98727809")]
        [InlineData("058549710972")]
        public void Fornecedor_Invalido_Com_Tamanho_Cpf_Diferente_Onze(string cpfTamanhoInvalido)
        {
            // Arrange
            Fornecedor fornecedor = ObterFornecedorPessoaFisica();
            fornecedor.Documento = cpfTamanhoInvalido;

            string msgErroEsperada = $"O campo {nameof(fornecedor.Documento)} precisa ter 11 caracteres e foi fornecido {fornecedor.Documento.Length}.";

            // Act
            var validador = _sut.Validate(fornecedor);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
            Assert.Equal(TipoFornecedor.PessoaFisica, fornecedor.TipoFornecedor);
        }

        [Theory]
        [InlineData("81381334062")]
        [InlineData("72376637012")]
        [InlineData("62530517015")]
        public void Fornecedor_Invalido_Com_Cpf_Invalido(string cpfInvalido)
        {
            // Arrange
            Fornecedor fornecedor = ObterFornecedorPessoaFisica();
            fornecedor.Documento = cpfInvalido;

            string msgErroEsperada = $"O documento fornecido é inválido.";

            // Act
            var validador = _sut.Validate(fornecedor);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
            Assert.Equal(TipoFornecedor.PessoaFisica, fornecedor.TipoFornecedor);
        }

        [Theory]
        [InlineData("910741380001")]
        [InlineData("95157014000120443")]
        [InlineData("7231027")]
        public void Fornecedor_Invalido_Com_Tamanho_Cnpj_Diferente_Onze(string cnpjTamanhoInvalido)
        {
            // Arrange
            Fornecedor fornecedor = ObterFornecedorPessoaJuridica();
            fornecedor.Documento = cnpjTamanhoInvalido;

            string msgErroEsperada = $"O campo {nameof(fornecedor.Documento)} precisa ter 14 caracteres e foi fornecido {fornecedor.Documento.Length}.";

            // Act
            var validador = _sut.Validate(fornecedor);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
            Assert.Equal(TipoFornecedor.PessoaJuridica, fornecedor.TipoFornecedor);
        }

        [Theory]
        [InlineData("72585867060159")]
        [InlineData("97173206300111")]
        [InlineData("76221043040101")]
        public void Fornecedor_Invalido_Com_Cnpj_Invalido(string cpnjInvalido)
        {
            // Arrange
            Fornecedor fornecedor = ObterFornecedorPessoaJuridica();
            fornecedor.Documento = cpnjInvalido;

            string msgErroEsperada = $"O documento fornecido é inválido.";

            // Act
            var validador = _sut.Validate(fornecedor);

            // Assert
            Assert.False(validador.IsValid);
            Assert.Contains(msgErroEsperada, validador.Errors.Select(v => v.ErrorMessage));
            Assert.Equal(TipoFornecedor.PessoaJuridica, fornecedor.TipoFornecedor);
        }

        #endregion

        #region Propriedades Válidas

        [Fact]
        public void Fornecedor_Com_Todas_Propriedades_Validas()
        {
            // Arrange
            Fornecedor fornecedor = ObterFornecedorValido();

            // Act
            var validador = _sut.Validate(fornecedor);

            // Assert
            Assert.True(validador.IsValid);
            Assert.Empty(validador.Errors);
        }

        #endregion

        #region Dados de Teste

        private Fornecedor ObterFornecedorValido()
        {
            return new Fornecedor
            {
                Nome = "Fornecedor Teste",
                Documento = "22976603081",
                TipoFornecedor = TipoFornecedor.PessoaFisica,
                Ativo = true,
                Produtos = new List<Produto>()
            };
        }

        private Fornecedor ObterFornecedorPessoaFisica()
        {
            Fornecedor fornecedor = ObterFornecedorValido();

            fornecedor.Documento = "81381334067";
            fornecedor.TipoFornecedor = TipoFornecedor.PessoaFisica;

            return fornecedor;
        }

        private Fornecedor ObterFornecedorPessoaJuridica()
        {
            Fornecedor fornecedor = ObterFornecedorValido();

            fornecedor.Documento = "25289170000169";
            fornecedor.TipoFornecedor = TipoFornecedor.PessoaJuridica;

            return fornecedor;
        }

        #endregion
    }
}
