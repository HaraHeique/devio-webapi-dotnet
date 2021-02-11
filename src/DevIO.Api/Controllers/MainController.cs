using DevIO.Api.ViewModels;
using DevIO.Business.Interfaces.Identity;
using DevIO.Business.Interfaces.Notifications;
using DevIO.Business.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Collections.Generic;
using System.Linq;

namespace DevIO.Api.Controllers
{
    [ApiController]
    public abstract class MainController : ControllerBase
    {
        private readonly INotificador _notificador;

        public MainController(INotificador notificador)
        {
            _notificador = notificador;
        }

        protected ActionResult CustomResponse(object result = null)
        {
            if (IsValidOperation())
            {
                return Ok(new ResponseViewModel 
                {
                    Success = true,
                    Data = result
                });
            }

            return BadRequest(new ResponseViewModel
            {
                Success = false,
                Errors = _notificador.ObterNotificacoes().Select(n => n.Mensagem).ToArray()
            });
        }

        protected ActionResult CustomResponse(ModelStateDictionary modelState)
        {
            if (modelState.IsValid) return CustomResponse();

            IEnumerable<ModelError> errors = modelState.Values.SelectMany(e => e.Errors);

            foreach (ModelError error in errors)
            {
                string message = error.Exception == null ? error.ErrorMessage : error.Exception.Message;
                _notificador.Handle(new Notificacao(message));
            }

            return CustomResponse();
        }

        protected ActionResult CustomErrorResponse(params string[] errors)
        {
            if (errors == null) return CustomResponse();

            foreach (string error in errors)
            {
                _notificador.Handle(new Notificacao(error));
            }

            return CustomResponse();
        }

        protected ActionResult NotFound(string error)
        {
            if (string.IsNullOrEmpty(error)) return NotFound();

            return NotFound(new ResponseViewModel 
            {
                Errors = new string[] { error },
                Success = false
            });
        }

        protected bool IsValidOperation()
        {
            return !_notificador.TemNotificacao();
        }
    }
}
