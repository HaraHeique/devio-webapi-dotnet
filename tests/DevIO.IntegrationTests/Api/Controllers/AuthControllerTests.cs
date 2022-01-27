using DevIO.Api;
using DevIO.Api.ViewModels;
using DevIO.Api.ViewModels.Users;
using DevIO.IntegrationTests.Helpers;
using DevIO.IntegrationTests.Setups.Auth;
using DevIO.IntegrationTests.Setups.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace DevIO.IntegrationTests.Api.Controllers
{
    public class AuthControllerTests : IntegrationTestsFixture, IDisposable
    {
        private const string CommonUri = "api/v2/conta";

        private readonly IServiceScope _scope;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthControllerTests(ApiWebApplicationFactory<Startup> factory) : base(factory) 
        {
            _scope = base.Factory.Services.CreateScope();
            _userManager = _scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        }

        public void Dispose()
        {
            _userManager.Dispose();
            _scope.Dispose();
        }

        [Fact]
        public async Task Obter_Usuario_Corrente_Com_Sucesso()
        {
            // Arrange
            RegisterUserViewModel usuarioRegistradoVM = UserViewModelTestsHelper.ObterInstanciaRegistroUsuario("default.user@test.com", "Dale@2020");

            await RegistrarUsuarioParaTestes(usuarioRegistradoVM);

            string usuarioRegistradoId = (await _userManager.FindByEmailAsync(usuarioRegistradoVM.Email)).Id;

            var userClaims = new AuthUserTest(
                new Claim(ClaimTypes.NameIdentifier, usuarioRegistradoId),
                new Claim(ClaimTypes.Email, usuarioRegistradoVM.Email)
            );

            // Act
            HttpResponseMessage response = await base.CreateClient(userClaims).GetAsync($"{CommonUri}/obter-usuario-corrente");

            // Assert
            var result = await ContentHelper.ExtractObject<CurrentUserViewModel>(response.Content);

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Registrar_Usuario_Com_Sucesso()
        {
            // Arrange
            RegisterUserViewModel registroVM = UserViewModelTestsHelper.ObterInstanciaRegistroUsuario("hsantos@otimize.io", "Dale%LoL5424633");
            HttpContent dataRequest = ContentHelper.CreateJsonContent(registroVM);

            // Act
            HttpResponseMessage response = await base.Client.PostAsync($"{CommonUri}/registrar", dataRequest);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Single(_userManager.Users.ToList());
            Assert.Contains(_userManager.Users.ToList(), u => u.Email == "hsantos@otimize.io");
        }
        
        [Theory]
        [InlineData("hsantosotimize", "Dale%")]
        [InlineData("hsantos@otimize.com", "Dale%LoL")]
        public async Task Registrar_Usuario_Com_Propriedades_Invalidas(string email, string password)
        {
            // Arrange
            RegisterUserViewModel registroVM = UserViewModelTestsHelper.ObterInstanciaRegistroUsuario(email, password);
            HttpContent dataRequest = ContentHelper.CreateJsonContent(registroVM);

            // Act
            HttpResponseMessage response = await base.Client.PostAsync($"{CommonUri}/registrar", dataRequest);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Empty(_userManager.Users.ToList());
        }

        [Fact]
        public async Task Login_Usuario_Com_Sucesso()
        {
            // Arrange
            string email = "hsantos@otimize.io";
            string password = "Dale%LoL5424633";

            RegisterUserViewModel registroVM = UserViewModelTestsHelper.ObterInstanciaRegistroUsuario(email, password);
            await RegistrarUsuarioParaTestes(registroVM);

            LoginUserViewModel LoginVM = UserViewModelTestsHelper.ObterInstanciaLoginUsuario(email, password);
            HttpContent dataRequest = ContentHelper.CreateJsonContent(LoginVM);

            // Act
            HttpResponseMessage response = await base.Client.PostAsync($"{CommonUri}/entrar", dataRequest);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(_userManager.Users.ToList(), u => u.Email == email);
        }
        
        [Fact]
        public async Task Login_Usuario_Com_Propriedades_Invalidas()
        {
            // Arrange
            LoginUserViewModel LoginVM = UserViewModelTestsHelper.ObterInstanciaLoginUsuario("hsantos", string.Empty);
            HttpContent dataRequest = ContentHelper.CreateJsonContent(LoginVM);

            // Act
            HttpResponseMessage response = await base.Client.PostAsync($"{CommonUri}/entrar", dataRequest);

            // Assert
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Login_Usuario_Com_Credenciais_Incorretas()
        {
            // Arrange
            LoginUserViewModel LoginVM = UserViewModelTestsHelper.ObterInstanciaLoginUsuario("email.teste@hotmail.com", "Fmfks123&4322");
            HttpContent dataRequest = ContentHelper.CreateJsonContent(LoginVM);

            // Act
            HttpResponseMessage response = await base.Client.PostAsync($"{CommonUri}/entrar", dataRequest);

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Single(result.Errors);
            Assert.Equal("Usuário ou senha incorretos! Tente novamente.", result.Errors.SingleOrDefault());
        }
        
        [Fact]
        public async Task Login_Deve_Bloquear_Usuario_Apos_Cinco_Tentativas_Incorretas()
        {
            // Arrange
            RegisterUserViewModel registroVM = UserViewModelTestsHelper.ObterInstanciaRegistroUsuario("email.teste@gmail.com", "GMfks123$5555");
            await RegistrarUsuarioParaTestes(registroVM);

            LoginUserViewModel LoginVM = UserViewModelTestsHelper.ObterInstanciaLoginUsuario("email.teste@gmail.com", "Fmfks123&4322");
            HttpContent dataRequest = ContentHelper.CreateJsonContent(LoginVM);

            // Act
            HttpResponseMessage response = null;

            for (int i = 0; i <= 4; i++)
            {
                response?.Dispose();
                response = await base.Client.PostAsync($"{CommonUri}/entrar", dataRequest);
            }

            // Assert
            var result = await ContentHelper.ExtractObject<ResponseViewModel>(response.Content);

            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Single(result.Errors);
            Assert.Equal("Usuário temporiariamente bloqueado por ter feito várias tentativas de entrada inválidas!", result.Errors.SingleOrDefault());
        }

        private async Task RegistrarUsuarioParaTestes(RegisterUserViewModel registroVM)
        {
            var user = new IdentityUser
            {
                UserName = registroVM.Email,
                Email = registroVM.Email,
                EmailConfirmed = true
            };

            await _userManager.CreateAsync(user, registroVM.Password);
        }
    }
}
