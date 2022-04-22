using System.Data;
using Npgsql;

namespace YmyPixels.Entities;

public class Action
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PixelId { get; set; }
    public DateTime Date { get; set; }
    public int PixelSnapshotColor { get; set; }

    /// <summary>
    /// Creates a <see cref="Entities.Action"/> object from an <see cref="NpgsqlDataReader"/> reference.
    /// </summary>
    /// <param name="r"><see cref="NpgsqlDataReader"/> reference to read data from</param>
    /// <returns><see cref="Entities.Action"/> object that is read from referenced <see cref="NpgsqlDataReader"/></returns>
    public static Action FromDatabase(ref NpgsqlDataReader r)
    {
        var obj = new Action();
        obj.Id = r.GetInt32("id");
        obj.UserId = r.GetInt32("userId");
        obj.PixelId = r.GetInt32("pixelId");
        obj.Date = r.GetDateTime("actionDate");
        obj.PixelSnapshotColor = r.GetInt32("pixelSnapshot");
        return obj;
    }
    
    public class Snapshot
    {
        public Entities.Action Action { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public static Snapshot FromDatabase(ref NpgsqlDataReader r)
        {
            var obj = new Snapshot();
            obj.Action = new Action();
            obj.Action.Id = r.GetInt32("id");
            obj.Action.UserId = r.GetInt32("userId");
            obj.Action.PixelId = r.GetInt32("pixelId");
            obj.Action.Date = r.GetDateTime("actionDate");
            obj.Action.PixelSnapshotColor = r.GetInt32("pixelSnapshot");
            obj.X = r.GetInt32("xPos");
            obj.Y = r.GetInt32("yPos");
            return obj;
        }
    }
}