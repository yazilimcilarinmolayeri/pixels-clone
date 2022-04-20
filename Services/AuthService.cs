using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using YmyPixels.Entities;
using YmyPixels.Entities.Configuration;
using YmyPixels.Utilities;

namespace YmyPixels.Services;

public class AuthService : IAuthService
{
    private readonly JwtOption _jwt;
    private readonly Data _data;
    
    public AuthService(JwtOption jwtOption, Data data)
    {
        _data = data;
        _jwt = jwtOption;
    }
    
    public async Task<User> Authenticate(string discordId)
    {
        var user = await _data.GetUser(discordId);
        
        // If user is a new user, add it to the database
        if (user == null)
        {
            user = new User()
            {
                DiscordId = ulong.Parse(discordId)
            };
            user.Id = await _data.InsertUser(user);
        }

        // If user is banned, do not authorize
        if (!user.Banned)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwt.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = _jwt.Audience,
                Issuer = _jwt.Issuer,
                IssuedAt = DateTime.UtcNow,
                Subject = new ClaimsIdentity(new Claim[] 
                {
                    new("urn:discord:id", user.DiscordId.ToString()),
                    new(ClaimTypes.NameIdentifier, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            user.Token = tokenHandler.WriteToken(token);
        }
        
        return user;
    }
}