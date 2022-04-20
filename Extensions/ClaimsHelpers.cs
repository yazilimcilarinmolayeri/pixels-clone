using System.Security.Claims;

namespace YmyPixels.Extensions;

public static class ClaimsHelpers
{
    public static string GetDiscordId(this ClaimsPrincipal identity)
    {
        return identity.Claims.First(c => c.Type == "urn:discord:id").Value;
    }
    
    public static string GetId(this ClaimsPrincipal identity)
    {
        return identity.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
    }
}