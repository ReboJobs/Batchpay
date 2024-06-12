using System.Security.Claims;

namespace XeroApp.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string XeroUserId(this ClaimsPrincipal claims)
        {
            return claims.FindFirstValue("xero_userid");
        }

        public static string XeroName(this ClaimsPrincipal claims)
        {
            return claims.FindFirstValue("name");
        }

        public static string XeroEmail(this ClaimsPrincipal claims)
        {
            return claims.FindFirstValue("preferred_username");
        }

 
    }
}
