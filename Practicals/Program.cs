using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Practicals.Services.Day1;
using Practicals.Services.Day2;
using System.Text;
using static Practicals.Services.Day2.DILifespans;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<Day2DbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Day2Db"))
           .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));

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

builder.Services.AddSingleton<ProductStore>();

builder.Services.AddTransient<TransientValue>();
builder.Services.AddScoped<ScopedValue>();
builder.Services.AddSingleton<SingletonValue>();




var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Advanced API v1");
    options.RoutePrefix = "swagger";
});


app.MapControllers();

app.Run();