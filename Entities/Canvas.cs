using System.Data;
using Npgsql;

namespace YmyPixels.Entities;

public class Canvas
{
    public Canvas()
    {
        Size = new Size(0, 0);
    }

    public Canvas(Size size)
    {
        Size = size;
    }

    public int Id { get; set; }
    public Size Size { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateClosed { get; set; }
    public DateTime DateExpire { get; set; }

    /// <summary>
    /// Creates a <see cref="Canvas"/> object from an <see cref="NpgsqlDataReader"/> reference.
    /// </summary>
    /// <param name="r"><see cref="NpgsqlDataReader"/> reference to read data from</param>
    /// <returns><see cref="Canvas"/> object that is read from referenced <see cref="NpgsqlDataReader"/></returns>
    public static Canvas FromDatabase(ref NpgsqlDataReader r)
    {
        var obj = new Canvas();
        obj.Id = r.GetInt32("id");
        obj.Size = new Size(r.GetInt32("sizeX"), r.GetInt32("sizeY"));
        obj.DateCreated = r.GetDateTime("dateCreated");
        obj.DateClosed = r.IsDBNull("dateClosed") ? null : r.GetDateTime("dateClosed");
        obj.DateExpire = r.GetDateTime("dateExpire");
        return obj;
    }
}