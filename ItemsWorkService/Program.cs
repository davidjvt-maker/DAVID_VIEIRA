using ItemsWorkService.Services;
using ItemsWorkService.Interfaces;
using System.Reflection;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Configurar XML Comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    if(System.IO.File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Habilitar CORS con orígenes restringidos
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var userServiceBase = builder.Configuration["UserService:BaseUrl"] ?? "https://localhost:7021";
builder.Services.AddHttpClient<IDistributionService, DistributionService>(client =>
{
    client.BaseAddress = new Uri(userServiceBase);
});

builder.Services.AddSingleton<IWorkItemRepository, InMemoryWorkItemRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ItemsWorkService API V1");
});

app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program { }