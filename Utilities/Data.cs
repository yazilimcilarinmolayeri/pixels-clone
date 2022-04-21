using Npgsql;
using YmyPixels.Entities;

namespace YmyPixels.Utilities;

public class Data
{
    private readonly NpgsqlConnection _connection;

    public Data(IConfiguration config)
    {
        var connectionString = config.GetValue<string>("ConnectionStrings:local");
        _connection = new NpgsqlConnection(connectionString);
    }

    #region Action Operations

    /// <summary>
    /// Inserts a new <see cref="Entities.Action"/> object to the database
    /// </summary>
    /// <param name="obj"><see cref="Entities.Action"/> object to insert</param>
    /// <returns>Id number of the inserted <see cref="Entities.Action"/></returns>
    public async Task<int> InsertAction(Entities.Action obj)
    {
        NpgsqlCommand cmd =
            new NpgsqlCommand(@"INSERT INTO ""actions"" VALUES(DEFAULT,@uid,@pid,@date) RETURNING ""id""",
                _connection);
        cmd.Parameters.AddWithValue("uid", obj.UserId);
        cmd.Parameters.AddWithValue("pid", obj.PixelId);
        cmd.Parameters.AddWithValue("date", DateTime.Now);
        
        _connection.Open();
        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        await _connection.CloseAsync();
        await cmd.DisposeAsync();

        return id;
    }
    
    /// <summary>
    /// Returns a user's last action date as <see cref="DateTime"/>
    /// </summary>
    /// <param name="userId">User's id number</param>
    /// <returns><see cref="Entities.Action"/>'s <see cref="DateTime"/></returns>
    public async Task<DateTime> GetLastActionDate(int userId)
    {
        NpgsqlCommand cmd =
            new NpgsqlCommand(@"SELECT COALESCE(""actionDate"", (CURRENT_TIMESTAMP(0) + INTERVAL '-1 YEAR')) FROM ""actions"" WHERE ""userId""=@id ORDER BY ""actionDate"" DESC LIMIT 1",
                _connection);
        cmd.Parameters.AddWithValue("id", userId);
        
        _connection.Open();
        var result = Convert.ToDateTime(await cmd.ExecuteScalarAsync() ?? DateTime.Now.AddYears(-1));
        
        await _connection.CloseAsync();
        await cmd.DisposeAsync();

        return result;
    }

    #endregion
    
    #region User Operations

    /// <summary>
    /// Inserts a new <see cref="User"/> to the database
    /// </summary>
    /// <param name="obj"><see cref="User"/> object to insert</param>
    /// <returns>Id number of the inserted <see cref="User"/></returns>
    public async Task<int> InsertUser(User obj)
    {
        NpgsqlCommand cmd =
            new NpgsqlCommand(@"INSERT INTO ""users"" VALUES(DEFAULT,@did,DEFAULT,@ban,@mod) RETURNING ""id""",
                _connection);
        cmd.Parameters.AddWithValue("did", obj.DiscordId.ToString());
        cmd.Parameters.AddWithValue("ban", obj.Banned);
        cmd.Parameters.AddWithValue("mod", obj.Moderator);
        
        _connection.Open();
        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        await _connection.CloseAsync();
        await cmd.DisposeAsync();

        return id;
    }
    
    public async Task<bool> UpdateUser(User obj)
    {
        NpgsqlCommand cmd =
            new NpgsqlCommand(@"UPDATE ""users"" SET ""isBanned""=@ban, ""isModerator""=@mod WHERE ""id""=@id",
                _connection);
        cmd.Parameters.AddWithValue("ban", obj.Banned);
        cmd.Parameters.AddWithValue("mod", obj.Moderator);
        cmd.Parameters.AddWithValue("id", obj.Id);
        
        _connection.Open();
        var id = await cmd.ExecuteNonQueryAsync();
        await _connection.CloseAsync();
        await cmd.DisposeAsync();

        return id > 0;
    }

    /// <summary>
    /// Returns the <see cref="User"/> with the given <paramref name="discordId"/>
    /// </summary>
    /// <param name="discordId"><see cref="User"/>'s Discord id number</param>
    /// <returns><see cref="User"/> object that matches with given <paramref name="discordId"/></returns>
    public async Task<User?> GetUser(string discordId)
    {
        NpgsqlCommand cmd =
            new NpgsqlCommand(@"SELECT * FROM ""users"" WHERE ""discordId""=@id",
                _connection);
        cmd.Parameters.AddWithValue("id", discordId);
        
        _connection.Open();
        var r = cmd.ExecuteReader();
        User? obj = null;
        if (r.Read())
            obj = User.FromDatabase(ref r);
        
        await r.CloseAsync();
        await _connection.CloseAsync();
        await cmd.DisposeAsync();
        await r.DisposeAsync();

        return obj;
    }

    #endregion
    
    #region Pixel Operations

    /// <summary>
    /// Sets a specific <see cref="Pixel"/> on the provided <see cref="Canvas"/> and returns its id number
    /// </summary>
    /// <param name="obj"><see cref="Pixel"/> data to insert (or update)</param>
    /// <returns>Id number of the inserted/updated <see cref="Pixel"/></returns>
    public async Task<int> SetPixel(Pixel obj)
    {
        NpgsqlCommand cmd =
            new NpgsqlCommand(@"SELECT ""SET_PIXEL""(@cid,@xpos,@ypos,@color)",
                _connection);
        cmd.Parameters.AddWithValue("cid", obj.CanvasId);
        cmd.Parameters.AddWithValue("xpos", obj.X);
        cmd.Parameters.AddWithValue("ypos", obj.Y);
        cmd.Parameters.AddWithValue("color", obj.Color);
        
        _connection.Open();
        var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        await _connection.CloseAsync();
        await cmd.DisposeAsync();

        return id;
    }

    /// <summary>
    /// Get a specific <see cref="Pixel"/> from given canvas and position.
    /// </summary>
    /// <param name="canvasId">Canvas' id to search on</param>
    /// <param name="x">X position on <paramref name="canvasId"/></param>
    /// <param name="y">Y position on <paramref name="canvasId"/></param>
    /// <returns><see cref="Pixel"/> matching with given <paramref name="canvasId"/>, <paramref name="x"/> and <paramref name="y"/></returns>
    public async Task<Pixel?> GetPixel(int canvasId, int x, int y)
    {
        NpgsqlCommand cmd =
            new NpgsqlCommand(@"SELECT * FROM ""pixel"" WHERE ""canvasId""=@id AND ""xPos""=@xpos AND ""yPos""=@ypos",
                _connection);
        cmd.Parameters.AddWithValue("id", canvasId);
        cmd.Parameters.AddWithValue("xpos", x);
        cmd.Parameters.AddWithValue("ypos", y);
        
        _connection.Open();
        var r = cmd.ExecuteReader();
        Pixel? obj = null;
        if (r.Read())
            obj = Pixel.FromDatabase(ref r);
        
        await r.CloseAsync();
        await _connection.CloseAsync();
        await cmd.DisposeAsync();
        await r.DisposeAsync();

        return obj;
    }
    
    /// <summary>
    /// Get all pixels on specified <see cref="Canvas"/>
    /// </summary>
    /// <param name="canvasId"><see cref="Canvas"/> id to get pixels from</param>
    /// <returns>List of pixels from specified canvas</returns>
    public async Task<List<Pixel>> GetPixels(int canvasId)
    {
        NpgsqlCommand cmd = new NpgsqlCommand(@"SELECT * FROM ""pixel"" WHERE ""canvasId""=@id", _connection);
        cmd.Parameters.AddWithValue("id", canvasId);
        
        _connection.Open();
        var r = cmd.ExecuteReader();
        List<Pixel> list = new List<Pixel>();
        while (r.Read())
            list.Add(Pixel.FromDatabase(ref r));
        
        await r.CloseAsync();
        await _connection.CloseAsync();
        await cmd.DisposeAsync();
        await r.DisposeAsync();

        return list;
    }

    #endregion
    
    #region Canvas Operations

    /// <summary>
    /// Inserts a new <see cref="Canvas"/> to database
    /// </summary>
    /// <param name="obj"><see cref="Canvas"/> object to insert</param>
    public async Task<int> NewCanvas(Canvas obj)
    {
        NpgsqlCommand cmd =
            new NpgsqlCommand(@"INSERT INTO ""canvas"" VALUES(DEFAULT,@sizex,@sizey,@dcreated,NULL,@dexpire) RETURNING ""id""",
                _connection);
        cmd.Parameters.AddWithValue("sizex", obj.Size.X);
        cmd.Parameters.AddWithValue("sizey", obj.Size.Y);
        cmd.Parameters.AddWithValue("dcreated", obj.DateCreated);
        cmd.Parameters.AddWithValue("dexpire", obj.DateExpire);
        
        _connection.Open();
        var result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        await _connection.CloseAsync();
        await cmd.DisposeAsync();

        return result;
    }
    
    public async Task<bool> UpdateCanvas(Canvas obj)
    {
        NpgsqlCommand cmd =
            new NpgsqlCommand(@$"UPDATE ""canvas"" SET ""sizeX"" = @sizex,""sizeY"" = @sizey,{(obj.DateClosed == null ? "" : @"""dateClosed"" = @dclose,")}""dateExpire"" = @dexpire WHERE ""id"" = @id",
                _connection);
        cmd.Parameters.AddWithValue("sizex", obj.Size.X);
        cmd.Parameters.AddWithValue("sizey", obj.Size.Y);
        if (obj.DateClosed != null) cmd.Parameters.AddWithValue("dclose", obj.DateClosed);
        cmd.Parameters.AddWithValue("dexpire", obj.DateExpire);
        cmd.Parameters.AddWithValue("id", obj.Id);
        
        _connection.Open();
        var result = await cmd.ExecuteNonQueryAsync();
        await _connection.CloseAsync();
        await cmd.DisposeAsync();

        return result > 0;
    }
    
    /// <summary>
    /// Fetches a specific <see cref="Canvas"/> matches with <paramref name="id"/>
    /// </summary>
    /// <param name="id">Id number of the <see cref="Canvas"/></param>
    /// <returns><see cref="Canvas"/> that matches with the given <paramref name="id"/></returns>
    public async Task<Canvas?> GetCanvas(int id)
    {
        NpgsqlCommand cmd = new NpgsqlCommand(@"SELECT * FROM ""canvas"" WHERE ""id""=@id", _connection);
        cmd.Parameters.AddWithValue("id", id);
        
        _connection.Open();
        var r = cmd.ExecuteReader();
        Canvas? obj = null;
        if (r.Read())
            obj = Canvas.FromDatabase(ref r);
        
        await r.CloseAsync();
        await _connection.CloseAsync();
        await cmd.DisposeAsync();
        await r.DisposeAsync();
        
        return obj;
    }
    
    /// <summary>
    /// Gets the latest active <see cref="Canvas"/> that is not expired.
    /// </summary>
    /// <returns>The latest active <see cref="Canvas"/> that is not expired.</returns>
    public async Task<Canvas?> GetCurrentCanvas()
    {
        NpgsqlCommand cmd = new NpgsqlCommand(@"SELECT * FROM ""GET_CURRENT_CANVAS""", _connection);
        
        _connection.Open();
        var r = cmd.ExecuteReader();
        Canvas? obj = null;
        
        if (r.Read())
            obj = Canvas.FromDatabase(ref r);

        await r.CloseAsync();
        await _connection.CloseAsync();
        await cmd.DisposeAsync();
        await r.DisposeAsync();

        return obj;
    }

    #endregion
}