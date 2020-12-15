using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace Phoenix.Api.App_Plugins
{
    public static class IPrincipalExtension
    {
        public static IEnumerable<string> getRoles(this IPrincipal user)
        {
            ClaimsPrincipal claimsPrincipal = user as ClaimsPrincipal;

            if (claimsPrincipal == null)
                return Array.Empty<string>();

            return claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
        }

        public static string getNameIdentifier(this IPrincipal user)
        {
            Claim claim = (user as ClaimsPrincipal)?.FindFirst(ClaimTypes.NameIdentifier);

            return claim != null ? claim.Value : string.Empty;
        }
    }

}
