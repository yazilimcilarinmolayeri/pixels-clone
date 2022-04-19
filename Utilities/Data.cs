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
    
    #region Pixel Operations

    /// <summary>
    /// Sets a specific <see cref="Pixel"/> on the provided <see cref="Canvas"/>
    /// </summary>
    /// <param name="obj"><see cref="Pixel"/> data to insert (or update)</param>
    public async Task SetPixel(Pixel obj)
    {
        NpgsqlCommand cmd =
            new NpgsqlCommand(@"CALL ""SET_PIXEL""(@cid,@xpos,@ypos,@color)",
                _connection);
        cmd.Parameters.AddWithValue("cid", obj.CanvasId);
        cmd.Parameters.AddWithValue("xpos", obj.X);
        cmd.Parameters.AddWithValue("ypos", obj.Y);
        cmd.Parameters.AddWithValue("color", obj.Color);
        
        _connection.Open();
        await cmd.ExecuteNonQueryAsync();
        await _connection.CloseAsync();
        await cmd.DisposeAsync();
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
    public async Task NewCanvas(Canvas obj)
    {
        NpgsqlCommand cmd =
            new NpgsqlCommand("INSERT INTO `canvas` VALUES(DEFAULT,@sizex,@sizey,@active,@dcreated,@dclosed,@dexpire)",
                _connection);
        cmd.Parameters.AddWithValue("sizex", obj.Size.X);
        cmd.Parameters.AddWithValue("sizey", obj.Size.Y);
        cmd.Parameters.AddWithValue("active", obj.Active);
        cmd.Parameters.AddWithValue("dcreated", obj.DateCreated);
        cmd.Parameters.AddWithValue("dclosed", obj.DateClosed!);
        cmd.Parameters.AddWithValue("dexpire", obj.DateExpire!);
        
        _connection.Open();
        await cmd.ExecuteNonQueryAsync();
        await _connection.CloseAsync();
        await cmd.DisposeAsync();
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