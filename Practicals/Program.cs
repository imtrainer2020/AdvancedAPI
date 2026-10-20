using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Practicals.Services.Day1;
using Practicals.Services.Day2;
using static Practicals.Services.Day1.LifetimeDemo;


var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();

#region "2.6 Refuse bad data and return safe errors"

builder.Services.AddControllers().ConfigureApiBehaviorOptions(o =>
{
    o.InvalidModelStateResponseFactory = c => {
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

var app = builder.Build();

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

#region "2.6 Refuse bad data and return safe errors"

app.UseExceptionHandler();

#endregion

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Advanced API v1");
    options.RoutePrefix = "swagger";
});

app.MapOpenApi("/openapi/{documentName}.json");
app.MapOpenApi("/openapi/{documentName}.yaml");

app.MapControllers();

app.Run();