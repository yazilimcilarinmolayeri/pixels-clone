using Microsoft.AspNetCore.Mvc;
using YmyPixels.Entities;
using YmyPixels.Models.Pixel;
using YmyPixels.Utilities;

namespace YmyPixels.Controllers;

[ApiController]
[Route("/api/pixel")]
public class PixelController : Controller
{
    private readonly Data _data;
    
    public PixelController(Data data)
    {
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

    // TODO: Make SetPixel route require authentication
    [HttpPut]
    public async Task<IActionResult> SetPixel([FromBody] SetPixel data)
    {
        // If color data is not 6 characters
        if (data.Color.Length != 6)
            return BadRequest(new
            {
                error_message = "Not a hex value."
            });
        
        // TODO: Get authenticated user data
        
        // Declare a color value that will be parsed from user data
        int c;

        try { c = Convert.ToInt32(data.Color, 16); }
        catch (FormatException)
        {
            return BadRequest(new
            {
                error_message = "Cannot parse hex value."
            });
        }

        var canvas = await _data.GetCurrentCanvas();
        
        // If there is no active canvas
        if(canvas == null)
            return NotFound(new
            {
                error_message = "No canvas is active right now."
            });
        
        // Check if x or y is outside the canvas
        if(data.x > canvas.Size.X || data.y > canvas.Size.Y)
            return BadRequest(new
            {
                error_message = "You are trying to set a pixel that is outside of current canvases bounds."
            });

        // All good, set pixel of canvas
        await _data.SetPixel(new Pixel()
        {
            X = data.x,
            Y = data.y,
            Color = c,
            CanvasId = canvas.Id
        });
        
        // TODO: Insert user action with date information

        return StatusCode(200);
    }
}