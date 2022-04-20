using AspNet.Security.OAuth.Discord;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YmyPixels.Entities.Configuration;
using YmyPixels.Extensions;
using YmyPixels.Services;

namespace YmyPixels.Controllers;

[ApiController]
[Route("/api/auth")]
public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly DiscordConfiguration _discordConfiguration;
    
    public AuthController(IAuthService authService, DiscordConfiguration discordConfiguration)
    {
        _authService = authService;
        _discordConfiguration = discordConfiguration;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        // Redirect users to Discord's challenge scheme
        return Challenge(new AuthenticationProperties()
        {
            RedirectUri = _discordConfiguration.Oauth.RedirectUri
        }, DiscordAuthenticationDefaults.AuthenticationScheme);
    }

    [HttpGet("discord/callback"), Authorize]
    public async Task<IActionResult> SigninDiscord()
    {
        var discordId = User.GetDiscordId();

        // Authenticate the user with jwt bearer token
        var user = await _authService.Authenticate(discordId);
        
        // If user is banned, send error message
        if (user.Banned)
            return Unauthorized(new
            {
                error_message = "You are banned from pixels API."
            });

        // All ok, return information back to the user
        return Ok(new
        {
            is_moderator = user.Moderator,
            discord_id = user.DiscordId,
            access_token = user.Token,
            expire_time = DateTime.UtcNow.AddMinutes(30)
        });
    }
}