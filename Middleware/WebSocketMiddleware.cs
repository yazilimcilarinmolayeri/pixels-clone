using System.Net;
using System.Net.WebSockets;
using System.Text;
using Microsoft.Extensions.Primitives;
using YmyPixels.Entities;
using YmyPixels.Services;
using YmyPixels.Utilities;

namespace YmyPixels.Middleware;

public class WebSocketMiddleware
{
    private readonly Data _data;
    private readonly IAuthService _auth;
    private readonly RequestDelegate _next;
    private readonly WebSocketConnectionManager _manager;
    
    public enum OperationCode
    {
        PixelUpdate
    }

    public WebSocketMiddleware(Data data, RequestDelegate next, WebSocketConnectionManager manager, IAuthService auth)
    {
        _data = data;
        _auth = auth;
        _next = next;
        _manager = manager;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (ctx.Request.Path == "/websocket")
        {
            if (ctx.WebSockets.IsWebSocketRequest)
            {
                // If authorization header is present
                if (ctx.Request.Headers.Authorization.Count == 0)
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                // And a bearer token is provided
                if (!ctx.Request.Headers.Authorization[0].StartsWith("Bearer "))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                
                // And this token is not empty
                string token = ctx.Request.Headers.Authorization[0][7..];
                if (string.IsNullOrWhiteSpace(token))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                
                // Now we can validate the provided token
                string discordId = _auth.ValidateToken(token, "urn:discord:id");
                if (discordId == String.Empty)
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                
                // If user exists, we can finally continue
                var dbUser = await _data.GetUser(discordId);
                if (dbUser == null || dbUser.Banned)
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }
                
                // This is for storing the websocket user information in session
                WebSocketUser user = new WebSocketUser()
                {
                    isModerator = dbUser.Moderator,
                    GUID = Guid.NewGuid().ToString()
                };

                // Accept the websocket and add it to the manager
                WebSocket ws = await ctx.WebSockets.AcceptWebSocketAsync();
                if (!_manager.AddSocket(ws, user))
                {
                    // I guess this is the right response code?
                    ctx.Response.StatusCode = StatusCodes.Status507InsufficientStorage;
                    return;
                }

                // Handling received messages
                await ReceiveMessage(ws, async (result, bytes) =>
                {
                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            // Just echo the message for now
                            Console.WriteLine($"{Encoding.UTF8.GetString(bytes)}");
                            break;
                        case WebSocketMessageType.Close:
                            Console.WriteLine($"Received close message from {user.GUID}");
                            _manager.RemoveSocket(user);
                            return;
                    }
                });
            }
            else ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
        else await _next(ctx);
    }

    private async Task ReceiveMessage(WebSocket ws, Action<WebSocketReceiveResult, byte[]> handle)
    {
        var buffer = new byte[1024 * 4];

        while (ws.State == WebSocketState.Open)
        {
            var receiveResult = await ws.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            handle(receiveResult, buffer);
        }
    }
}