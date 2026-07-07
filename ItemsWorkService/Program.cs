using ItemsWorkService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
// Registrar el cliente HTTP de .NET
builder.Services.AddHttpClient();
builder.Services.AddScoped<IDistributionService, DistributionService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();