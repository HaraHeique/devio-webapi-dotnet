using DevIO.Business.Interfaces.Identity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace DevIO.Api.Extensions
{
    public class AspNetUser : IUser
    {
        private readonly IHttpContextAccessor _accessor;

        public AspNetUser(IHttpContextAccessor acessor)
        {
            _accessor = acessor;
        }

        public string Name => _accessor.HttpContext.User.Identity.Name;

        public Guid GetUserId()
        {
            return IsAuthenticated() ? Guid.Parse(_accessor.HttpContext.User.GetUserId()) : Guid.Empty;
        }

        public string GetUserEmail()
        {
            return IsAuthenticated() ? _accessor.HttpContext.User.GetUserEmail() : "";
        }

        public bool IsAuthenticated()
        {
            return _accessor.HttpContext.User.Identity.IsAuthenticated;
        }

        public bool IsInRole(string role)
        {
            return _accessor.HttpContext.User.IsInRole(role);
        }

        public IEnumerable<Claim> GetClaimsIdentity()
        {
            return _accessor.HttpContext.User.Claims;
        }

        public IEnumerable<string> GetUserRoles()
        {
            return IsAuthenticated() ? _accessor.HttpContext.User.GetUserRoles() : Array.Empty<string>();
        }

        public IEnumerable<Claim> GetCustomClaims(string[] claimsType)
        {
            return IsAuthenticated() ? _accessor.HttpContext.User.GetUserCustomClaims(claimsType) : Array.Empty<Claim>();
        }
    }
}
