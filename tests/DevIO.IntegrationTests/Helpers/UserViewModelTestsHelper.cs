using DevIO.Api.ViewModels.Users;

namespace DevIO.IntegrationTests.Helpers
{
    public static class UserViewModelTestsHelper
    {
        public static RegisterUserViewModel ObterInstanciaRegistroUsuario(string email, string senha)
        {
            return new RegisterUserViewModel
            {
                Email = email,
                Password = senha,
                ConfirmPassword = senha
            };
        }

        public static LoginUserViewModel ObterInstanciaLoginUsuario(string email, string senha)
        {
            return new LoginUserViewModel
            {
                Email = email,
                Password = senha
            };
        }
    }
}
