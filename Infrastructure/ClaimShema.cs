using KiraNet.GutsMvc.Filter;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;

namespace KiraNet.GutsMvc.BBS.Infrastructure
{
    /// <summary>
    /// 认证
    /// </summary>
    public class ClaimShema : IClaimSchema
    {
        public IPrincipal CreateSchema(HttpContext httpContext)
        {
            IPrincipal principal;
            if (!httpContext.TryGetUserInfo(out var userInfo))
            {
                principal = new ClaimsPrincipal();
                Thread.CurrentPrincipal = principal;

                return principal;
            }

            var claims = new List<Claim>();
            
            foreach (var role in userInfo.Roles.Trim().Split(','))
            {
                if (!String.IsNullOrWhiteSpace(role))
                    claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, userInfo.Id.ToString());
            principal = new ClaimsPrincipal(identity);
            Thread.CurrentPrincipal = principal;

            return principal;
        }
    }
}
