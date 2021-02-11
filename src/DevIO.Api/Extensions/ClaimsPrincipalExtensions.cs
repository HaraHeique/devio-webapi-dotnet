using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace DevIO.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentException(nameof(principal));

            var claim = principal.FindFirst(ClaimTypes.NameIdentifier);

            return claim?.Value;
        }

        public static string GetUserEmail(this ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentException(nameof(principal));

            var claim = principal.FindFirst(ClaimTypes.Email);

            return claim?.Value;
        }

        public static IEnumerable<string> GetUserRoles(this ClaimsPrincipal principal)
        {
            if (principal == null) throw new ArgumentException(nameof(principal));

            var claims = principal.FindAll(x => x.Type == ClaimTypes.Role);

            return claims.Select(c => c.Value);
        }
        
        public static IEnumerable<Claim> GetUserCustomClaims(this ClaimsPrincipal principal, string[] claimTypes)
        {
            if (principal == null) throw new ArgumentException(nameof(principal));

            var claims = principal.FindAll(x => claimTypes.Contains(x.Type));

            return claims;
        }
    }
}
