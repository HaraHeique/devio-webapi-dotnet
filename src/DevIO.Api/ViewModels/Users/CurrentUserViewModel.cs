using System;
using System.Collections.Generic;

namespace DevIO.Api.ViewModels.Users
{
    public class CurrentUserViewModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string[] Roles { get; set; }
        public IEnumerable<CurrentUserClaimViewModel> Claims { get; set; }
    }

    public class CurrentUserClaimViewModel
    {
        public string Type { get; set; }
        public string[] Values { get; set; }
    }
}
