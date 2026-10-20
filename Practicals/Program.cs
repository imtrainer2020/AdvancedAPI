using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Practicals.Services.Day1;
using Practicals.Services.Day2;
using static Practicals.Services.Day1.LifetimeDemo;
using Practicals.Services.Day3;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Polly;
using Polly.Extensions.Http;

var builder = WebApplication.CreateBuilder(args);

// 1. Plain Client (Bina retry ke)
builder.Services.AddHttpClient("PlainClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:8081");
});

// 2. Polly Client (3 Retries + 3 Second Timeout)
builder.Services.AddHttpClient("PollyClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:8081");
})
.AddPolicyHandler(HttpPolicyExtensions
    .HandleTransientHttpError() // 5xx errors aur 408 request timeout handle karta hai
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(500))) // Har retry me 500ms delay
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(3))); // 3 sec timeout

//builder.Services.AddControllers();

#region "2.6 Refuse bad data and return safe errors"

builder.Services.AddControllers().ConfigureApiBehaviorOptions(o =>
{
    o.InvalidModelStateResponseFactory = c =>
    {
        var p = new ValidationProblemDetails(c.ModelState)
        {
            Status = 400,
            Title = "Check your inputs.",
            Instance = c.HttpContext.Request.Path
        };
        p.Extensions["traceId"] = c.HttpContext.TraceIdentifier;
        return new BadRequestObjectResult(p) { ContentTypes = { "application/problem+json" } };
    };
});

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<Day2Errors>();

#endregion

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Advanced API",
        Version = "v1",
        Description = "Advanced API Training Project"
    });

    #region "JWT Token for Swagger"

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Paste the raw token only (no 'Bearer ' word)."
    });
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });

    #endregion

});

builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
});

builder.Services.AddTransient<TransientStamp>();
builder.Services.AddScoped<ScopedStamp>();
builder.Services.AddSingleton<SingletonStamp>();
builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddDbContext<CampusDb>(o => o.UseSqlServer(
    builder.Configuration.GetConnectionString("Day2Db")));
builder.Services.AddScoped<StudentDbService>();

#region "JWT Token implementation"

builder.Services.AddDay3Security(builder.Configuration);

#endregion

#region "3.6 — Versions, rate limiting, output cache and health checks"

builder.Services.AddDay3Production();

#endregion

var app = builder.Build();

#region "2.6 Refuse bad data and return safe errors"

app.UseExceptionHandler();

#endregion

#region "1.3 Request Pipeline"

//app.Use(async (context, next) =>
//{
//    app.Logger.LogInformation("IN  {Method} {Path}",
//        context.Request.Method, context.Request.Path);
//    context.Response.Headers["X-Day1-Demo"] = "pipeline";
//    await next();
//    app.Logger.LogInformation("OUT {Status}", context.Response.StatusCode);
//});

#endregion

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Advanced API v1");
    options.RoutePrefix = "swagger";
});

#region "Authentication, Authorization and CORS implementation"

app.UseRouting();
app.UseCors("LabBrowser");
app.UseAuthentication();     // who are you? (must come first)
app.UseAuthorization();      // what may you do?

#endregion

#region "3.6 — Versions, rate limiting, output cache and health checks"

app.UseRateLimiter();
app.UseOutputCache();

// Not a part of Versions, rate limiting, output cache and health checks"
app.MapOpenApi("/openapi/{documentName}.json");
app.MapOpenApi("/openapi/{documentName}.yaml");

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = r => r.Tags.Contains("ready") });

#endregion

app.MapControllers();

app.Run();

public partial class Program { }