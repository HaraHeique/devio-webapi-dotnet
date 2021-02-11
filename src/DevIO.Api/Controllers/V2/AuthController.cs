using DevIO.Api.Extensions;
using DevIO.Api.ViewModels.Users;
using DevIO.Business.Interfaces.Identity;
using DevIO.Business.Interfaces.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Controllers.V2
{
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/conta")]
    public class AuthController : MainController
    {
        private readonly IUser _currentUser;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSettings;

        public AuthController(INotificador notificador,
                              SignInManager<IdentityUser> signInManager,
                              UserManager<IdentityUser> userManager,
                              IOptions<AppSettings> appSettings, 
                              IUser currentUser) : base(notificador)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _appSettings = appSettings.Value;
            _currentUser = currentUser;
        }

        [Authorize]
        [HttpGet("obter-usuario-corrente")]
        public async Task<CurrentUserViewModel> ObterUsuarioCorrente()
        {
            var claims = await _userManager.GetClaimsAsync(await _userManager.GetUserAsync(User));

            return new CurrentUserViewModel
            {
                Id = _currentUser.GetUserId(),
                Email = _currentUser.GetUserEmail(),
                Roles = _currentUser.GetUserRoles().ToArray(),
                Claims = claims.Select(c => new CurrentUserClaimViewModel 
                {
                    Type = c.Type,
                    Values = c.Value.Split(",")
                })
            };
        }

        [HttpPost("registrar")]
        public async Task<ActionResult<string>> Registrar([FromBody] RegisterUserViewModel registroUsuarioVM)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = new IdentityUser
            {
                UserName = registroUsuarioVM.Email,
                Email = registroUsuarioVM.Email,
                EmailConfirmed = true,
            };

            var result = await _userManager.CreateAsync(user, registroUsuarioVM.Password);

            if (result.Succeeded)
            {
                // Logar o usuário na app
                await _signInManager.SignInAsync(user, false);
                LoginResponseViewModel token = await JwtGenerator.GerarToken(registroUsuarioVM.Email, _appSettings, _userManager);

                return CustomResponse(token);
            }
            else
            {
                // Erros capturados ao criar usuário
                var errors = result.Errors.Select(e => e.Description).ToArray();

                return CustomErrorResponse(errors);
            }
        }

        [HttpPost("entrar")]
        public async Task<ActionResult<string>> Entrar([FromBody] LoginUserViewModel loginVM)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var result = await _signInManager.PasswordSignInAsync(loginVM.Email, loginVM.Password, false, true);

            if (result.Succeeded)
            {
                LoginResponseViewModel token = await JwtGenerator.GerarToken(loginVM.Email, _appSettings, _userManager);

                return CustomResponse(token);
            }

            if (result.IsLockedOut)
            {
                return CustomErrorResponse("Usuário temporiariamente bloqueado por ter feito várias tentativas de entrada inválidas!");
            }

            return CustomErrorResponse("Usuário ou senha incorretos! Tente novamente.");
        }
    }
}
