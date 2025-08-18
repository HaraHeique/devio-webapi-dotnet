using DevIO.Api.ViewModels.Users;
using System;

namespace DevIO.IntegrationTests.Helpers
{
    public class PermissoesViewModelTestsHelper
    {
        public static RoleViewModel[] ObterInstaciansRolesPadroes()
        {
            return [
                new RoleViewModel { Id = Guid.NewGuid().ToString(), Name = "Admin" },
                new RoleViewModel { Id = Guid.NewGuid().ToString(), Name = "User" },
                new RoleViewModel { Id = Guid.NewGuid().ToString(), Name = "Guest" }
            ];
        }
    }
}
