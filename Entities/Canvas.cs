using System.Data;
using Npgsql;

namespace YmyPixels.Entities;

public class Canvas
{
    public Canvas()
    {
        Size = new Size(0, 0);
    }

    public int Id { get; set; }
    public bool Active { get; set; }
    public Size Size { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateClosed { get; set; }
    public DateTime? DateExpire { get; set; }

    public static Canvas FromDatabase(ref NpgsqlDataReader r)
    {
        var obj = new Canvas();
        obj.Id = r.GetInt32("id");
        obj.Size = new Size(r.GetInt32("sizeX"), r.GetInt32("sizeY"));
        obj.Active = r.GetBoolean("isActive");
        obj.DateCreated = r.GetDateTime("dateCreated");
        obj.DateClosed = r.IsDBNull("dateClosed") ? null : r.GetDateTime("dateClosed");
        obj.DateExpire = r.IsDBNull("dateExpire") ? null : r.GetDateTime("dateExpire");
        return obj;
    }
}