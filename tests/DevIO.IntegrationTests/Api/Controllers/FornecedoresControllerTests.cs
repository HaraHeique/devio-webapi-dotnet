using DevIO.Api;
using DevIO.Api.ViewModels;
using DevIO.IntegrationTests.Setups.Fixtures;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace DevIO.IntegrationTests.Api.Controllers
{
    public class FornecedoresControllerTests : IntegrationTestsFixture
    {
        public FornecedoresControllerTests(ApiWebApplicationFactory<Startup> factory) : base(factory) { }

        [Fact]
        public async Task Obter_Todos_Fornecedores_Com_Sucesso()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "api/v2/fornecedores");

            // Act
            HttpResponseMessage response = await base.Client.SendAsync(request);

            // Assert
            var result = JsonConvert.DeserializeObject<ResponseViewModel>(await response.Content.ReadAsStringAsync());

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result.Data);
        }
    }
}
