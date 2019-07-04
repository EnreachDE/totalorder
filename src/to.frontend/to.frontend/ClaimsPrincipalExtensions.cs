using System.Linq;
using System.Security.Claims;
using to.contracts;
using to.frontend.Constants;

namespace to.frontend
{
    public static class ClaimsPrincipalExtensions
    {
        public static bool HasPermission(this ClaimsPrincipal user, Permission permission)
        {
            if (user.Identity.IsAuthenticated)
            {
                return user.Claims.Any(c =>
                {
                    var typeEqual = c.Type.Equals(CustomClaims.Permission);
                    var valueEqual = c.Value.Equals(permission.ToString());
                    return typeEqual && valueEqual;
                });
            }

            return false;
        }

        public static int GetId(this ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated)
            {
                return int.Parse(user.Claims.First(x => x.Type == CustomClaims.UserId).Value);
            }

            return -1;
        }
    }
}
