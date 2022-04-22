using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using YmyPixels.Entities;
using YmyPixels.Extensions;
using YmyPixels.Utilities;
using Action = YmyPixels.Entities.Action;
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

    #region BuildPixels method

    /// <summary>
    /// Builds the referenced <see cref="Rgb24"/> array with given <see cref="Pixel"/> list
    /// </summary>
    /// <param name="pixels">Image pixel information to write</param>
    /// <param name="canvasPixels">List of <see cref="Pixel"/> information</param>
    /// <param name="canvasWidth">Canvas' width</param>
    private void BuildPixels(ref Rgb24[] pixels, ref List<Pixel> canvasPixels, int canvasWidth)
    {
        foreach (var t in canvasPixels)
        {
            if(t.X > canvasWidth) continue;
            if(t.Y * canvasWidth + t.X >= pixels.Length) continue;
            
            ref var p = ref pixels[t.Y * canvasWidth + t.X];
            p.R = (byte)(t.Color >> 16 & 0xff);
            p.G = (byte) (t.Color >> 8 & 0xff);
            p.B = (byte) (t.Color & 0xff);
        }
    }
    
    /// <summary>
    /// Builds the referenced <see cref="Rgb24"/> array with given <see cref="Action.Snapshot"/> list
    /// </summary>
    /// <param name="pixels"><see cref="Rgb24"/> image's array</param>
    /// <param name="snapshots">List of <see cref="Action.Snapshot"/> information</param>
    /// <param name="canvasWidth">Canvas' width</param>
    private void BuildPixels(ref Rgb24[] pixels, ref List<Action.Snapshot> snapshots, int canvasWidth)
    {
        foreach (var t in snapshots)
        {
            if(t.X > canvasWidth) continue;
            if(t.Y * canvasWidth + t.X >= pixels.Length) continue;
            
            ref var p = ref pixels[t.Y * canvasWidth + t.X];
            p.R = (byte)(t.Action.PixelSnapshotColor >> 16 & 0xff);
            p.G = (byte) (t.Action.PixelSnapshotColor >> 8 & 0xff);
            p.B = (byte) (t.Action.PixelSnapshotColor & 0xff);
        }
    }
    
    /// <summary>
    /// Builds the referenced <see cref="Rgb24"/> array with given <see cref="Action.Snapshot"/> list and overrides
    /// the drawing colors with <paramref name="colorOverride"/>
    /// </summary>
    /// <param name="pixels"><see cref="Rgb24"/> image's array</param>
    /// <param name="canvasPixels">List of <see cref="Action.Snapshot"/> information</param>
    /// <param name="canvasWidth">Canvas' width</param>
    /// <param name="colorOverride"><see cref="int"/> color to draw</param>
    private void BuildPixels(ref Rgb24[] pixels, ref List<Action.Snapshot> canvasPixels, int canvasWidth, int colorOverride)
    {
        foreach (var t in canvasPixels)
        {
            if(t.X > canvasWidth) continue;
            if(t.Y * canvasWidth + t.X >= pixels.Length) continue;
            
            ref var p = ref pixels[t.Y * canvasWidth + t.X];
            p.R = (byte)(colorOverride >> 16 & 0xff);
            p.G = (byte) (colorOverride >> 8 & 0xff);
            p.B = (byte) (colorOverride & 0xff);
        }
    }

    #endregion
    
    /// <summary>
    /// Creates a new <see cref="Canvas"/> with given <paramref name="data"/>
    /// </summary>
    /// <param name="data">Properties of the new <see cref="Canvas"/></param>
    /// <returns>Status 200 (OK) if successful</returns>
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

    /// <summary>
    /// Updates a <see cref="Canvas"/>
    /// </summary>
    /// <param name="canvasId">Id number of the <see cref="Canvas"/> that user wants to update</param>
    /// <param name="data">New parameters of the <see cref="Canvas"/></param>
    /// <returns>Status 200 (OK) if successful.</returns>
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

    /// <summary>
    /// Returns the heatmap of a <see cref="Canvas"/> in given time.
    /// </summary>
    /// <param name="canvasId">Id number of the requested <see cref="Canvas"/></param>
    /// <param name="fromTimestamp">UTC unix timestamp of start time</param>
    /// <param name="toTimestamp">UTC unix timestamp of end time</param>
    /// <param name="actionColor">Default: Red (#ff0000), color override</param>
    /// <returns>Heatmap image (Content-Type: image/webp) if successful</returns>
    [HttpGet("/api/canvas/{canvasId}/heatmap")]
    public async Task<IActionResult> Heatmap(int canvasId, [FromQuery] long fromTimestamp, [FromQuery] long? toTimestamp, [FromQuery] int actionColor = 0xff0000)
    {
        Canvas? canvas = await _data.GetCanvas(canvasId);
        
        // If current canvas doesn't exist
        if (canvas == null)
            return NotFound(new
            {
                error_message = "No active canvas found, try again later."
            });

        // If 'to' time is null, set to now
        toTimestamp ??= DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        // If request doesn't make a sense
        if (fromTimestamp >= toTimestamp)
            return BadRequest(new
            {
                error_message = "'fromTimestamp' cannot be bigger or equal to 'toTimestamp'"
            });
        
        // Get snapshot of pixels
        var actionList = await _data.GetActionsBetweenDates(canvas.Id, fromTimestamp, (long)toTimestamp);

        // Create an Rgb24 array to store pixel information
        var pixels = new Rgb24[canvas.Size.X * canvas.Size.Y];

        // Set every pixel white
        for (int i = 0; i < pixels.Length; i++)
            pixels[i].R = pixels[i].B = pixels[i].G = 0;

        // Then build pixels got from database
        BuildPixels(ref pixels, ref actionList, canvas.Size.X, actionColor);
        
        // Create the Image object from pixel data
        Image img = Image.LoadPixelData(pixels, canvas.Size.X, canvas.Size.Y);
        Stream s = new MemoryStream();
        // Copy the image into MemoryStream
        await img.SaveAsWebpAsync(s);
        s.Position = 0;
        
        // Finally, convert the stream to a file result
        return File(s, "image/webp");
    }
    
    /// <summary>
    /// Returns a snapshot of a <see cref="Canvas"/> as WEBP image at requested time
    /// </summary>
    /// <param name="canvasId">Id of the requested <see cref="Canvas"/></param>
    /// <param name="untilTimestamp">Requested UTC unix timestamp</param>
    /// <returns>Image representation (Content-Type: image/webp) of the snapshot at given time or error message if something bad happens.</returns>
    [HttpGet("/api/canvas/{canvasId}/snapshot/{untilTimestamp}")]
    public async Task<IActionResult> Snapshot(int canvasId, long untilTimestamp)
    {
        Canvas? canvas = await _data.GetCanvas(canvasId);
        
        // If current canvas doesn't exist
        if (canvas == null)
            return NotFound(new
            {
                error_message = "No active canvas found, try again later."
            });

        // Get snapshot of pixels at given timestamp
        var snapshotList = await _data.GetSnapshots(canvas.Id, untilTimestamp);

        // Create an Rgb24 array to store pixel information
        var pixels = new Rgb24[canvas.Size.X * canvas.Size.Y];

        // Set every pixel white
        for (int i = 0; i < pixels.Length; i++)
            pixels[i].R = pixels[i].B = pixels[i].G = 255;

        // Then build pixels got from database
        BuildPixels(ref pixels, ref snapshotList, canvas.Size.X);
        
        // Create the Image object from pixel data
        Image img = Image.LoadPixelData(pixels, canvas.Size.X, canvas.Size.Y);
        Stream s = new MemoryStream();
        // Copy the image into MemoryStream
        await img.SaveAsWebpAsync(s);
        s.Position = 0;
        
        // Finally, convert the stream to a file result
        return File(s, "image/webp");
    }
    
    /// <summary>
    /// Returns the requested <see cref="Canvas"/> (if <paramref name="canvasId"/> is null, searches for currently active canvas)
    /// as WEBP image or if Accept header is 'application/json', returns the <see cref="Canvas"/> information as JSON
    /// </summary>
    /// <param name="canvasId">OPTIONAL: Id of the requested <see cref="Canvas"/></param>
    /// <returns>Image representation (Content-Type: image/webp) of the <see cref="Canvas"/> or its information if Accept header is 'application/json'</returns>
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