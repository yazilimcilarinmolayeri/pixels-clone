using System.Data;
using Npgsql;

namespace YmyPixels.Entities;

public class Pixel
{
    public int Id { get; set; }
    public int CanvasId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Color { get; set; }

    /// <summary>
    /// Creates a <see cref="Pixel"/> object from an <see cref="NpgsqlDataReader"/> reference.
    /// </summary>
    /// <param name="r"><see cref="NpgsqlDataReader"/> reference to read data from</param>
    /// <returns><see cref="Pixel"/> object that is read from referenced <see cref="NpgsqlDataReader"/></returns>
    public static Pixel FromDatabase(ref NpgsqlDataReader r)
    {
        var obj = new Pixel();
        obj.Id = r.GetInt32("id");
        obj.CanvasId = r.GetInt32("canvasId");
        obj.X = r.GetInt32("xPos");
        obj.Y = r.GetInt32("yPos");
        obj.Color = r.GetInt32("color");
        return obj;
    }
}