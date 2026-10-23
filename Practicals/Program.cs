using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Practicals.Services.Day1;
using Practicals.Services.Day2;
using Practicals.Services.Day3;
using System.Text;
using static Practicals.Services.Day2.DILifespans;
using Microsoft.OpenApi;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCaching(); //For caching the response of the API for a specific duration
builder.Services.AddControllers();

#region CORS Policy Making

// 1. Add CORS Services (Define Policies)
builder.Services.AddCors(options =>
{
    // Policy 1: Allow any origin (Best for Dev environment)
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    // Policy 2: Allow specific frontend (Best for Production)
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://my-real-frontend.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

#endregion

builder.Services.AddDbContext<Day2DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Day2Db"))
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));

builder.Services.AddDbContext<Day3DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Day3Db"))
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));

#region Simple Authentication Implementation (Microsoft)

// 1. Add Authentication & Authorization Services
//builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//    .AddCookie(options =>
//    {
//        // Prevent redirecting to a login page; return 401 for APIs instead
//        options.Events.OnRedirectToLogin = context =>
//        {
//            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
//            return Task.CompletedTask;
//        };
//        options.Events.OnRedirectToAccessDenied = context =>
//        {
//            context.Response.StatusCode = StatusCodes.Status403Forbidden;
//            return Task.CompletedTask;
//        };
//    });

#endregion

#region JWT Token Based Authentication Implementation

// 1. JWT Authentication Setup
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Advanced API",
        Version = "v1",
        Description = "Advanced API Training Project"
    });

    // Configure JWT Bearer authentication
    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token"
        });

    // Apply the Bearer security scheme globally
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "Bearer", document)] = new List<string>()
        });
});

#endregion

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSingleton<ProductStore>();
builder.Services.AddTransient<TransientValue>();
builder.Services.AddScoped<ScopedValue>();
builder.Services.AddSingleton<SingletonValue>();

// 2. Rate Limiter Configuration
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("FixedPolicy", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(10); // 10 seconds ki time window
        opt.PermitLimit = 5;                   // Maximum 5 requests allowed
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;                    // reject extra requests
    });

    // Custom status code when limit cross ho jaye (429 - Too Many Requests)
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

// Enable CORS Middleware (Apply Policies)
app.UseCors("AllowSpecificOrigins");

app.UseResponseCaching();
app.UseRateLimiter();

// 2. Add Middlewares (Order is strictly important)
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Advanced API v1");
    options.RoutePrefix = "swagger";
});

app.MapControllers();

app.Run();