using System.Data;
using Npgsql;

namespace YmyPixels.Entities;

public class User
{
    public int Id { get; set; }
    public DateTime DateRegister { get; set; }
    public ulong DiscordId { get; set; }
    public bool Banned { get; set; }
    public bool Moderator { get; set; }
    
    /// <summary>
    /// User's jwt token
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Creates a <see cref="User"/> object from an <see cref="NpgsqlDataReader"/> reference.
    /// </summary>
    /// <param name="r"><see cref="NpgsqlDataReader"/> reference to read data from</param>
    /// <returns><see cref="User"/> object that is read from referenced <see cref="NpgsqlDataReader"/></returns>
    public static User FromDatabase(ref NpgsqlDataReader r)
    {
        var obj = new User();
        obj.Id = r.GetInt32("id");
        obj.DiscordId = ulong.Parse(r.GetString("discordId"));
        obj.Moderator = r.GetBoolean("isModerator");
        obj.Banned = r.GetBoolean("isBanned");
        obj.DateRegister = r.GetDateTime("registerDate");
        return obj;
    }
}