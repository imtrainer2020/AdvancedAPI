using Microsoft.OpenApi;

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

var app = builder.Build();

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