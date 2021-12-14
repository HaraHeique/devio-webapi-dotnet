using DevIO.Api.ViewModels;
using System;

namespace DevIO.IntegrationTests.Helpers
{
    public static class EnderecoViewModelTestsHelper
    {
        public static EnderecoViewModel ObterInstancia(Guid fornecedorId)
        {
            return new EnderecoViewModel
            {
                Id = Guid.NewGuid(),
                Bairro = "Jaraguá",
                Cep = "31260080",
                Cidade = "Belo Horizonte",
                Complemento = "Em algum local de MG",
                Estado = "MG",
                Logradouro = "Rua Radioamador",
                Numero = "43",
                FornecedorId = fornecedorId
            };
        }
    }
}
