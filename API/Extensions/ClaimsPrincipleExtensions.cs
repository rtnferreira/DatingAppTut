using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        public static string GetUserName(this ClaimsPrincipal user)
        {
            /* return user.FindFirst(ClaimTypes.NameIdentifier)?.Value; */
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static int GetUserID(this ClaimsPrincipal user)
        {
            string nameIdentifier = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(nameIdentifier);
        }
    }
}