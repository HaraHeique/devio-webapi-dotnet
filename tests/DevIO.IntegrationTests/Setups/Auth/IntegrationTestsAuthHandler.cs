using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace DevIO.IntegrationTests.Setups.Auth
{
    public class IntegrationTestsAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly AuthUserTest _authUserTest;

        public IntegrationTestsAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, 
                                           ILoggerFactory logger, 
                                           UrlEncoder encoder, 
                                           ISystemClock clock,
                                           AuthUserTest authUserTest) : base(options, logger, encoder, clock)
        {
            _authUserTest = authUserTest;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!_authUserTest.Claims.Any())
            {
                return Task.FromResult(AuthenticateResult.Fail("O mock do usuário não foi configurado via DI."));
            }

            // Cria claims principal (identity) e seu ticket
            var identity = new ClaimsIdentity(_authUserTest.Claims, AuthConstants.DefaultScheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, AuthConstants.DefaultScheme);

            // Autentica a request sempre como sucesso
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
