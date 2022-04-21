using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using YmyPixels.Entities;
using YmyPixels.Middleware.Models;

namespace YmyPixels.Middleware;

public class WebSocketConnectionManager
{
    private readonly ConcurrentDictionary<WebSocketUser, WebSocket> _sockets = new();

    public ConcurrentDictionary<WebSocketUser, WebSocket> GetAllSockets()
    {
        return _sockets;
    }

    public bool AddSocket(WebSocket ws, WebSocketUser user)
    {
        var result = _sockets.TryAdd(user, ws);
        if (result) Console.WriteLine($"Websocket connection added: {user.GUID}");
        return result;
    }

    public void RemoveSocket(WebSocketUser user)
    {
        var result = _sockets.TryRemove(user, out _);
        if(result) Console.WriteLine($"Websocket connection removed: {user.GUID}");
    }

    public async Task BroadcastPixelUpdate(PixelUpdateData data)
    {
        foreach (var socket in _sockets)
        {
            string msg;

            // If user is a moderator, send discord id too
            if (socket.Key.isModerator)
                msg = JsonConvert.SerializeObject(new
                {
                    op = WebSocketMiddleware.OperationCode.PixelUpdate,
                    data.pixel,
                    data.discordUser
                });
            else
                msg = JsonConvert.SerializeObject(new
                {
                    op = WebSocketMiddleware.OperationCode.PixelUpdate,
                    data.pixel
                });
            
            await socket.Value.SendAsync(
                new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg), 0, msg.Length),
                WebSocketMessageType.Text,
                true, CancellationToken.None);
        }
    }
}