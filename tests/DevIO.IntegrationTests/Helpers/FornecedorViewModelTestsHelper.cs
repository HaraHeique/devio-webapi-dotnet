using DevIO.Api.ViewModels;
using System;

namespace DevIO.IntegrationTests.Helpers
{
    public static class FornecedorViewModelTestsHelper
    {
        public static FornecedorViewModel ObterInstancia(int tipo, string documento)
        {
            Guid id = Guid.NewGuid();

            return new FornecedorViewModel
            {
                Id = id,
                Ativo = true,
                Documento = documento,
                Nome = "Fornecedor Teste",
                TipoFornecedor = tipo,
                Endereco = EnderecoViewModelTestsHelper.ObterInstancia(id)
            };
        }
    }
}
