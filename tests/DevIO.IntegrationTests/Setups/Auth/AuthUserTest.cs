using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace DevIO.IntegrationTests.Setups.Auth
{
    public class AuthUserTest
    {
        public List<Claim> Claims { get; private set; }

        public AuthUserTest(params Claim[] claims)
        {
            this.Claims = claims.ToList();
        }
    }
}
