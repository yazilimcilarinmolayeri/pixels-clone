using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using YmyPixels.Entities;
using YmyPixels.Extensions;
using YmyPixels.Utilities;
using Size = YmyPixels.Entities.Size;

namespace YmyPixels.Controllers;

[Route("/api/canvas")]
[ApiController]
public class CanvasController : Controller
{
#pragma warning disable CS8618
    public class PutCanvasModel
    {
        public Size Size { get; set; }
        public long DateExpire { get; set; }
    }
    public class PatchCanvasModel
    {
        public Size? Size { get; set; }
        public long? DateExpire { get; set; }
    }
#pragma warning restore CS8618
    
    private readonly Data _data;
    
    public CanvasController(Data data)
    {
        _data = data;
    }

    /// <summary>
    /// Builds the canvas using given <paramref name="canvasPixels"/> information
    /// </summary>
    /// <param name="pixels">Image pixel information to write</param>
    /// <param name="canvasPixels">User pixel information to read</param>
    /// <param name="canvasWidth">Canvas' width</param>
    private void BuildPixels(ref Rgb24[] pixels, ref List<Pixel> canvasPixels, int canvasWidth)
    {
        foreach (var t in canvasPixels)
        {
            ref var p = ref pixels[t.Y * canvasWidth + t.X];
            p.R = (byte)(t.Color >> 16 & 0xff);
            p.G = (byte) (t.Color >> 8 & 0xff);
            p.B = (byte) (t.Color & 0xff);
        }
    }
    
    [HttpPut, Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Create([FromBody] PutCanvasModel data)
    {
        string discordId = User.GetDiscordId();
        User? user = await _data.GetUser(discordId);
        
        // If user is not a moderator
        if(!user.Moderator)
            return Unauthorized(new
            {
                error_message = "You do not have the privilege to create a canvas."
            });

        // If requested size is lower than minimum acceptable size
        if (data.Size.X < 300 || data.Size.Y < 300)
            return BadRequest(new
            {
                error_message = "Canvas' width or height cannot be lower than 300px."
            });
        
        // If requested expire date is sooner than 30 minutes later
        DateTimeOffset dateExpire = DateTimeOffset.FromUnixTimeSeconds(data.DateExpire);
        if (dateExpire < DateTime.UtcNow.AddMinutes(30))
            return BadRequest(new
            {
                error_message = "DateExpire cannot be sooner than 30 minutes later."
            });
        
        // Finally, create a canvas
        Canvas canvas = new Canvas(data.Size)
        {
            DateCreated = DateTime.UtcNow,
            DateExpire = dateExpire.DateTime
        };
        canvas.Id = await _data.NewCanvas(canvas);

        return Ok(canvas);
    }

    [HttpPatch("/api/canvas/{canvasId}"), Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> Patch(int canvasId, [FromBody] PatchCanvasModel data)
    {
        string discordId = User.GetDiscordId();
        User? user = await _data.GetUser(discordId);
        
        // If user is not a moderator
        if(!user.Moderator)
            return Unauthorized(new
            {
                error_message = "You do not have the privilege to update a canvas."
            });
        
        Canvas? canvas = await _data.GetCanvas(canvasId);
        if (canvas == null)
            return NotFound(new
            {
                error_message = "Canvas not found, try again later."
            });

        if (data.Size != null)
        {
            // If requested size is lower than minimum acceptable size
            if (data.Size.X < 300 || data.Size.Y < 300)
                return BadRequest(new
                {
                    error_message = "Canvas' width or height cannot be lower than 300px."
                });

            canvas.Size = data.Size;
        }

        if (data.DateExpire != null)
        {
            // If requested expire date is sooner than 30 minutes later
            DateTimeOffset dateExpire = DateTimeOffset.FromUnixTimeSeconds((long)data.DateExpire);
            if (dateExpire < DateTime.UtcNow.AddMinutes(30))
                return BadRequest(new
                {
                    error_message = "DateExpire cannot be sooner than 30 minutes later."
                });

            canvas.DateExpire = dateExpire.DateTime;
        }

        await _data.UpdateCanvas(canvas);
        
        return Ok();
    }
    
    [HttpGet("/api/canvas/{canvasId?}")]
    public async Task<IActionResult> Index(int? canvasId)
    {
        Canvas? canvas;

        // If specific canvas is requested, search it, if not, get current active canvas.
        if (canvasId != null)
            canvas = await _data.GetCanvas((int)canvasId);
        else
            canvas = await _data.GetCurrentCanvas();
        
        // If current canvas doesn't exist
        if (canvas == null)
            return NotFound(new
            {
                error_message = "No active canvas found, try again later."
            });
        
        // If accept header has 'application/json', simply return canvases details
        if (Request.Headers.Accept == "application/json")
        {
            return Ok(new
            {
                canvas.Id,
                canvas.Size,
                dateCreated = ((DateTimeOffset)canvas.DateCreated).ToUnixTimeSeconds(),
                dateClosed = canvas.DateClosed == null ? (long?) null : ((DateTimeOffset)canvas.DateClosed).ToUnixTimeSeconds(),
                dateExpire = ((DateTimeOffset)canvas.DateExpire).ToUnixTimeSeconds()
            });
        }
        
        // Get all pixels of canvas
        var canvasPixels = await _data.GetPixels(canvas.Id);
        
        // Create an Rgb24 array to store pixel information
        var pixels = new Rgb24[canvas.Size.X * canvas.Size.Y];

        // Set every pixel white
        for (int i = 0; i < pixels.Length; i++)
            pixels[i].R = pixels[i].B = pixels[i].G = 255;

        // Then build pixels got from database
        BuildPixels(ref pixels, ref canvasPixels, canvas.Size.X);
        
        // Create the Image object from pixel data
        Image img = Image.LoadPixelData(pixels, canvas.Size.X, canvas.Size.Y);
        Stream s = new MemoryStream();
        // Copy the image into MemoryStream
        await img.SaveAsWebpAsync(s);
        s.Position = 0;

        // Finally, convert the stream to a file result
        return File(s, "image/webp");
    }
}