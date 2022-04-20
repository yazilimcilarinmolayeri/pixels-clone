namespace YmyPixels.Entities.Configuration;

#pragma warning disable CS8618
public class Oauth
{
    public ulong ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string RedirectUri { get; set; }
    public string[] Scopes { get; set; }
}
#pragma warning restore CS8618