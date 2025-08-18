using DevIO.Api.ViewModels.Users;
using DevIO.IntegrationTests.Helpers;
using DevIO.IntegrationTests.Setups;
using DevIO.IntegrationTests.Setups.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using DevIO.Api.ViewModels;
using System;
using Newtonsoft.Json;

namespace DevIO.IntegrationTests.Api.Controllers
{
    public class PermissoesControllerTests : BaseIntegrationTests, IDisposable
    {
        private const string CommonUri = "api/v1/permissoes";

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly AuthUserTest _authUserWithClaimsTest;

        public PermissoesControllerTests(ApiWebApplicationFactory factory) : base(factory)
        {
            _userManager = ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            _roleManager = ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            _authUserWithClaimsTest = new AuthUserTest(new Claim(ClaimTypes.Role, "Admin")); // TODO: Extrair para uma classe de possíveis roles para evitar "strings mágicas"
        }

        public void Dispose()
        {
            _userManager.Dispose();
            _roleManager.Dispose();
        }

        [Fact]
        public async Task Obter_Roles_Cadastradas_No_Sistema_Com_Sucesso()
        {
            // Arrange
            var rolesRegistradasVM = PermissoesViewModelTestsHelper.ObterInstaciansRolesPadroes();

            foreach (var item in rolesRegistradasVM) await _roleManager.CreateAsync(new IdentityRole(item.Name));

            // Act
            HttpResponseMessage response = await base.CreateClient(_authUserWithClaimsTest)
                .GetAsync($"{CommonUri}/roles");

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);

            var dataResponse = JsonConvert.DeserializeObject<RoleViewModel[]>(JsonConvert.SerializeObject(result.Data)); // TODO: Colocar isto aqui em um utilitário!
            Assert.All(dataResponse, role =>
            {
                Assert.Contains(rolesRegistradasVM, x => x.Name == role.Name);
            });
        }
    }
}
