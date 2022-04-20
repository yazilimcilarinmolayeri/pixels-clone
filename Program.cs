using System.Text;
using System.Text.Json;
using YmyPixels.Services;
using YmyPixels.Utilities;
using System.Net.Http.Headers;
using AspNet.Security.OAuth.Discord;
using Microsoft.IdentityModel.Tokens;
using YmyPixels.Entities.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;

#region Configure the application and services

var builder = WebApplication.CreateBuilder(args);

// Get Discord configuration from configuration
var discordConfig = builder.Configuration.GetSection("Discord").Get<DiscordConfiguration>();
// Get json web token configuration from configuration
var jwtConfig = builder.Configuration.GetSection("Jwt").Get<JwtOption>();

builder.Services
    .AddSingleton(jwtConfig)
    .AddSingleton(discordConfig)
    .AddTransient<Data>()
    .AddScoped<IAuthService, AuthService>();

builder.Services
    .AddAuthentication(DiscordAuthenticationDefaults.AuthenticationScheme)
    .AddDiscord(DiscordAuthenticationDefaults.AuthenticationScheme, opt =>
    {
        opt.ClientId = discordConfig.Oauth.ClientId.ToString();
        opt.ClientSecret = discordConfig.Oauth.ClientSecret;
        foreach (var s in discordConfig.Oauth.Scopes)
            opt.Scope.Add(s);

        opt.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        opt.AuthorizationEndpoint = DiscordAuthenticationDefaults.AuthorizationEndpoint;
        opt.TokenEndpoint = DiscordAuthenticationDefaults.TokenEndpoint;
        opt.UserInformationEndpoint = DiscordAuthenticationDefaults.UserInformationEndpoint;

        opt.ClaimActions.MapJsonKey("urn:discord:id", "id");
        opt.ClaimActions.MapJsonKey("urn:discord:username", "username");
        opt.ClaimActions.MapJsonKey("urn:discord:discriminator", "discriminator");

        opt.Events = new OAuthEvents()
        {
            OnCreatingTicket = async ctx =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, ctx.Options.UserInformationEndpoint);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ctx.AccessToken);

                var response = await ctx.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead,
                    ctx.HttpContext.RequestAborted);
                response.EnsureSuccessStatusCode();

                var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

                ctx.RunClaimActions(user);
            }
        };
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opt =>
    {
        opt.Cookie = new CookieBuilder()
        {
            Name = "YmyPixels",
            MaxAge = TimeSpan.FromDays(1),
        };
        opt.ExpireTimeSpan = TimeSpan.FromDays(1);
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig.Issuer,
            ValidAudience = jwtConfig.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.Secret))
        };
        opt.Events = new JwtBearerEvents()
        {
            OnChallenge = context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";

                // Ensure we always have an error and error description.
                if (string.IsNullOrEmpty(context.Error))
                    context.Error = "invalid_token";
                if (string.IsNullOrEmpty(context.ErrorDescription))
                    context.ErrorDescription = "This request requires a valid JWT access token to be provided";

                // Add some extra context for expired tokens.
                if (context.AuthenticateFailure != null && context.AuthenticateFailure.GetType() == typeof(SecurityTokenExpiredException))
                {
                    var authenticationException = context.AuthenticateFailure as SecurityTokenExpiredException;
                    context.Response.Headers.Add("x-token-expired", authenticationException!.Expires.ToString("o"));
                    context.ErrorDescription = $"The token expired on {authenticationException.Expires:o}";
                }

                return context.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    error = context.Error,
                    error_message = context.ErrorDescription
                }));
            }
        };
        opt.ForwardSignIn = DiscordAuthenticationDefaults.AuthenticationScheme;
        opt.ForwardSignOut = DiscordAuthenticationDefaults.AuthenticationScheme;
    });

builder.Services.AddControllers();

#endregion

#region Configuration of the app

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCookiePolicy();

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

#endregion