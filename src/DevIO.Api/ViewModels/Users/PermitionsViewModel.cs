using System.ComponentModel.DataAnnotations;

#nullable disable
namespace DevIO.Api.ViewModels.Users
{
    public record UserPermitionsViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "O campo {0} é obrigatório")]
        public string UserId { get; set; }
        public RoleViewModel[] Roles { get; set; }
        public ClaimsViewModel[] Claims { get; set; }
    }

    public record RoleViewModel
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "O campo {0} é obrigatório")]
        public string Id { get; init; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "O campo {0} é obrigatório")]
        public string Name { get; init; }
    }

    public record ClaimsViewModel
    {
        public string Type { get; init; }
        public string Value { get; init; }
    }
}
