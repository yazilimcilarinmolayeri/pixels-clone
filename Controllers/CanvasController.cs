using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using YmyPixels.Entities;
using YmyPixels.Utilities;

namespace YmyPixels.Controllers;

[Route("/api/canvas")]
[ApiController]
public class CanvasController : Controller
{
    private readonly Data _data;
    public CanvasController(Data data)
    {
        _data = data;
    }

    private void BuildPixels(ref Rgb24[] pixels, ref List<Pixel> canvasPixels, ref Canvas canvas)
    {
        foreach (var t in canvasPixels)
        {
            ref var p = ref pixels[t.Y * canvas.Size.X + t.X];
            p.R = (byte)(t.Color >> 16 & 0xff);
            p.G = (byte) (t.Color >> 8 & 0xff);
            p.B = (byte) (t.Color & 0xff);
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Get current active canvas
        var canvas = await _data.GetCurrentCanvas();
        
        // If current canvas doesn't exist
        if (canvas == null)
            return NotFound(new
            {
                error_message = "No active canvas found, try again later."
            });
        
        // Get all pixels of canvas
        var canvasPixels = await _data.GetPixels(canvas.Id);
        
        // Create an Rgb24 array to store pixel information
        var pixels = new Rgb24[canvas.Size.X * canvas.Size.Y];

        // Set every pixel white
        for (int i = 0; i < pixels.Length; i++)
            pixels[i].R = pixels[i].B = pixels[i].G = 255;

        // Then build pixels got from database
        BuildPixels(ref pixels, ref canvasPixels, ref canvas);
        
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