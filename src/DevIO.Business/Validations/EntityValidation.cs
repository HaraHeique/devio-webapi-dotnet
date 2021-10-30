using DevIO.Business.Models;
using FluentValidation;

namespace DevIO.Business.Validations
{
    public class EntityValidation : AbstractValidator<Entity>
    {
        public EntityValidation()
        {
            RuleFor(e => e.Id)
                .NotEmpty().WithMessage("O campo {PropertyName} não pode ser vazio");
        }
    }
}
