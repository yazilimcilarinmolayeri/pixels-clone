using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YmyPixels.Entities;
using YmyPixels.Extensions;
using YmyPixels.Middleware;
using YmyPixels.Middleware.Models;
using YmyPixels.Utilities;

namespace YmyPixels.Controllers;

[ApiController]
[Route("/api/pixel")]
public class PixelController : Controller
{
#pragma warning disable CS8618
    public class SetPixelModel
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Color { get; set; }
    }
#pragma warning restore CS8618
    
    private readonly Data _data;
    private readonly WebSocketConnectionManager _manager;
    
    public PixelController(Data data, WebSocketConnectionManager connectionManager)
    {
        _manager = connectionManager;
        _data = data;
    }
    
    [Route("{x}-{y}")]
    public async Task<IActionResult> GetPixel(int x, int y)
    {
        var canvas = await _data.GetCurrentCanvas();
        
        // If current canvas doesn't exist
        if(canvas == null)
            return NotFound(new
            {
                error_message = "No active canvas found, try again later."
            });
        
        // Check if x or y is outside the canvas
        if(x > canvas.Size.X || y > canvas.Size.Y)
            return BadRequest(new
            {
                error_message = "You are trying to get a pixel that is outside of current canvases bounds."
            });

        dynamic? pixel = await _data.GetPixel(canvas.Id, x, y);
        
        // If pixel is not found, return white
        if(pixel == null)
            pixel = new
            {
                X = x,
                Y = y,
                Color = 0xffffff
            };
        
        return Ok(new
        {
            x = pixel.X,
            y = pixel.Y,
            color = $"#{pixel.Color.ToString("x6")}"
        });
    }

    [HttpPut, Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> SetPixel([FromBody] SetPixelModel data)
    {
        // If color data is not 6 characters
        if (data.Color.Length != 6)
            return BadRequest(new
            {
                error_message = "Not a hex value."
            });

        // Get authenticated user and its last action date
        string discordId = User.GetDiscordId();
        var user = await _data.GetUser(discordId);
        var lastActionDate = await _data.GetLastActionDate(user!.Id);
        
        // Check user's last action date and if it is past 1 minute, continue. If not, error.
        // If user is a moderator, it can bypass this restriction
        if (!user.Moderator && lastActionDate.AddMinutes(1) > DateTime.Now)
            return StatusCode(403, new
            {
                error_message = "It has not been 1 minute since your last action."
            });
        
        // Declare a color value that will be parsed from user data
        int c;

        // Try to convert the given hex color to an integer
        try { c = Convert.ToInt32(data.Color, 16); }
        // If conversion fails, return a bad request error to the user
        catch (FormatException)
        {
            return BadRequest(new
            {
                error_message = "Cannot parse hex value."
            });
        }

        // Get current active canvas
        var canvas = await _data.GetCurrentCanvas();
        
        // If there is no active canvas
        if(canvas == null)
            return NotFound(new
            {
                error_message = "No canvas is active right now."
            });
        
        // Check if x or y is outside the canvas
        if(data.X > canvas.Size.X || data.Y > canvas.Size.Y)
            return BadRequest(new
            {
                error_message = "You are trying to set a pixel that is outside of current canvases bounds."
            });

        // Get snapshot of the pixel before setting new value
        Pixel? pixelSnapshot = await _data.GetPixel(canvas.Id, data.X, data.Y);
        
        // All good, set pixel of canvas
        var pixel = new Pixel()
        {
            X = data.X,
            Y = data.Y,
            Color = c,
            CanvasId = canvas.Id
        };
        pixel.Id = await _data.SetPixel(pixel);

        // Log this action to the database
        // If previous pixel exists, log its color. If not, log #ffffff
        await _data.InsertAction(new Entities.Action()
        {
            PixelId = pixel.Id,
            UserId = int.Parse(User.GetId()),
            PixelSnapshotColor = pixelSnapshot?.Color ?? 0xffffff
        });

        // Send new pixel's value to all websocket listeners
        await _manager.BroadcastPixelUpdate(new PixelUpdateData()
        {
            pixel = pixel,
            discordUser = ulong.Parse(discordId)
        });
        
        // All ok, return 200
        return StatusCode(200);
    }
}