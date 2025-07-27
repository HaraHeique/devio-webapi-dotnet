using DevIO.Business.Interfaces.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;

namespace DevIO.Api.Controllers.V1
{
    [Authorize]
    [Route("api/v{version:apiVersion}/fornecedores")]
    public class PermissaoController : MainController
    {
        public PermissaoController(INotificador notificador) : base(notificador)
        {
        }
    }
}
