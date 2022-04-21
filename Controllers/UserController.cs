using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YmyPixels.Entities;
using YmyPixels.Extensions;
using YmyPixels.Utilities;

namespace YmyPixels.Controllers;

[ApiController]
[Route("/api/user")]
public class UserController : Controller
{
    private readonly Data _data;
    
    public UserController(Data data)
    {
        _data = data;
    }
    
    [HttpPatch("{discordId}/ban"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> BanUser(string discordId)
    {
        string sourceDiscordId = User.GetDiscordId();
        User? source = await _data.GetUser(sourceDiscordId);

        // If request user is not a moderator
        if (!source.Moderator)
            return Unauthorized(new
            {
                error_message = "You do not have the privilege to ban a user."
            });
        
        // Check if given discord id is found
        User? user = await _data.GetUser(discordId);
        if (user == null)
            return NotFound(new
            {
                error_message = $"User not found with Discord id: {discordId}"
            });

        // Prevent moderators from banning themselves
        if (user.DiscordId.ToString() == sourceDiscordId)
            return Unauthorized(new
            {
                error_message = "You cannot ban yourself."
            });
        
        // Update the user
        bool alreadyBanned = user.Banned;
        if (!alreadyBanned)
        {
            user.Banned = true;
            user.Moderator = false;
            await _data.UpdateUser(user);
        }

        return Ok(new
        {
            message = $"User {(alreadyBanned ? "already banned" : "banned")}."
        });
    }
}