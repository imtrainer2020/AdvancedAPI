using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using static Practicals.Models.Day3;

namespace Practicals.Services.Day3
{
    public static class Day3Setup
    {
        public static IServiceCollection AddDay3Security(this IServiceCollection services, IConfiguration config)
        {
            services.AddOptions<JwtSettings>()
                .Bind(config.GetSection("Jwt"))
                .ValidateDataAnnotations()
                .ValidateOnStart();                       // missing/short key = clear startup error
            services.AddSingleton<LabSecurity>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtSettings>>((o, jwt) =>
                {
                    var s = jwt.Value;
                    o.MapInboundClaims = false;           // keep claim names exactly as issued
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = s.Issuer,
                        ValidateAudience = true,
                        ValidAudience = s.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s.SigningKey)),
                        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });

            services.AddAuthorization(o => o.AddPolicy("AdminOnly",
                p => p.RequireAuthenticatedUser().RequireRole("Admin")));

            services.AddCors(o => o.AddPolicy("LabBrowser", p =>
                p.WithOrigins(config.GetSection("AllowedOrigins")
                 .Get<string[]>() ?? new[] { "http://localhost:5500" })
                 .AllowAnyHeader().AllowAnyMethod()));
            return services;
        }

        public static IServiceCollection AddDay3Production(this IServiceCollection services)
        {
            services.AddRateLimiter(o =>
            {
                o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                o.AddFixedWindowLimiter("LabLimit", p =>
                {
                    p.PermitLimit = 3; 
                    p.Window = TimeSpan.FromSeconds(30); 
                    p.QueueLimit = 0;
                });
            });
            services.AddOutputCache();
            services.AddHealthChecks().AddCheck<LabDbHealth>("sql", tags: new[] { "ready" });
            services.AddHttpClient();                       // used by 3.10
            return services;
        }

    }
}
