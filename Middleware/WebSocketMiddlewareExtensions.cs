namespace YmyPixels.Middleware;

public static class WebSocketMiddlewareExtensions
{
    public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<WebSocketMiddleware>();
    }

    public static IServiceCollection AddWebSocketManager(this IServiceCollection services)
    {
        return services.AddSingleton<WebSocketConnectionManager>();
    }
}