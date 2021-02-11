using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace DevIO.Business.Interfaces.Identity
{
    public interface IUser
    {
        string Name { get; }
        Guid GetUserId();
        string GetUserEmail();
        bool IsAuthenticated();
        bool IsInRole(string role);
        IEnumerable<Claim> GetClaimsIdentity();
        IEnumerable<string> GetUserRoles();
        IEnumerable<Claim> GetCustomClaims(string[] claimsType);
    }
}
