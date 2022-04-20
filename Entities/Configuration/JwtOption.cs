namespace YmyPixels.Entities.Configuration;

#pragma warning disable CS8618
public class JwtOption
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Secret { get; set; }
}
#pragma warning restore CS8618