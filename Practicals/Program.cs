using Microsoft.OpenApi;
using Practicals.Services.Day1;
using static Practicals.Services.Day1.LifetimeDemo;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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

var app = builder.Build();

//app.Use(async (context, next) =>
//{
//    app.Logger.LogInformation("IN  {Method} {Path}",
//        context.Request.Method, context.Request.Path);
//    context.Response.Headers["X-Day1-Demo"] = "pipeline";
//    await next();
//    app.Logger.LogInformation("OUT {Status}", context.Response.StatusCode);
//});

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