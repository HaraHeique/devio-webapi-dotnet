using Asp.Versioning;
using DevIO.Api.ViewModels.Users;
using DevIO.Business.Interfaces.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace DevIO.Api.Controllers.V1
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/permissoes")]
    [Authorize(Roles = "Admin")]
    public class PermissoesController : MainController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public PermissoesController(
            INotificador notificador,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager
        ) : base(notificador)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet("roles")]        
        public IActionResult ObterTodasRoles()
        {
            var roles = _roleManager.Roles.Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name
            }).ToArray();

            return CustomResponse(roles);
        }

        [HttpPost("roles")]
        public async Task<IActionResult> CriarRole([FromBody] RoleViewModel model)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (await _roleManager.RoleExistsAsync(model.Name))
                return CustomErrorResponse("Role já existe.");

            var result = await _roleManager.CreateAsync(new IdentityRole(model.Name));

            if (!result.Succeeded) return CustomResponse(result);

            return CustomResponse(model);
        }

        [HttpDelete("roles/{roleId}")]
        public async Task<ActionResult> DeletarRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);

            if (role == null)
                return NotFound("Role não encontrada.");

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);

            if (usersInRole.Any())
                return CustomErrorResponse("Não é possível remover uma role associada a usuários.");

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
                return CustomResponse(result);

            return CustomResponse();
        }

        [HttpGet("por-usuario")]
        public async Task<ActionResult> ObterPermissoesUsuario([FromQuery] string email = null, [FromQuery] string id = null)
        {
            IdentityUser user = null;

            if (!string.IsNullOrEmpty(email))
                user = await _userManager.FindByEmailAsync(email);
            else if (!string.IsNullOrEmpty(id))
                user = await _userManager.FindByIdAsync(id);

            if (user == null) return NotFound("Usuário não encontrado.");

            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            var result = new UserPermitionsViewModel
            {
                UserId = user.Id,
                Roles = roles.Select(r => new RoleViewModel { Name = r, Id = null }).ToArray(),
                Claims = claims.Select(c => new ClaimsViewModel { Type = c.Type, Value = c.Value }).ToArray()
            };

            return CustomResponse(result);
        }

        [HttpPost("associar-usuario")]
        public async Task<ActionResult> AssociarUsuario([FromBody] UserPermitionsViewModel model)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null) return NotFound("Usuário não encontrado.");

            if (!model.Roles.Any() && !model.Claims.Any())
                return CustomErrorResponse("Nenhuma role ou claim informada para associar ao usuário.");

            // Roles
            var result = await AssociateRoles(model, user);

            if (!result.Succeeded) return CustomResponse(result);

            // Claims
            result = await AssociateClaims(model, user);

            if (!result.Succeeded) return CustomResponse(result);

            return CustomResponse();

            async Task<IdentityResult> AssociateRoles(UserPermitionsViewModel model, IdentityUser user)
            {
                var roleNames = model.Roles.Select(r => r.Name).ToArray();

                return await _userManager.AddToRolesAsync(user, roleNames);
            }

            async Task<IdentityResult> AssociateClaims(UserPermitionsViewModel model, IdentityUser user)
            {
                var claims = model.Claims
                    .Select(c => new System.Security.Claims.Claim(c.Type, c.Value));

                return await _userManager.AddClaimsAsync(user, claims);
            }
        }

        [HttpDelete("desassociar-usuario")]
        public async Task<ActionResult> DisassociarUsuario([FromBody] UserPermitionsViewModel model)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null) return NotFound("Usuário não encontrado.");

            if (!model.Roles.Any() && !model.Claims.Any())
                return CustomErrorResponse("Nenhuma role ou claim informada para associar ao usuário.");

            // Roles
            var roleResult = await DesassociateRoles(model, user);
            if (!roleResult.Succeeded) return CustomResponse(roleResult);

            // Claims
            var claimsResult = await DesassociateClaims(model, user);
            if (!claimsResult.Succeeded) return CustomResponse(claimsResult);

            return CustomResponse();

            async Task<IdentityResult> DesassociateRoles(UserPermitionsViewModel model, IdentityUser user)
            {
                var roleNames = model.Roles.Select(r => r.Name);
                claimsResult = await _userManager.RemoveFromRolesAsync(user, roleNames);

                return claimsResult;
            }
            
            async Task<IdentityResult> DesassociateClaims(UserPermitionsViewModel model, IdentityUser user)
            {
                var claims = model.Claims.Select(c => new System.Security.Claims.Claim(c.Type, c.Value)).ToList();
                var result = await _userManager.RemoveClaimsAsync(user, claims);

                return result;
            }
        }
    }
}
