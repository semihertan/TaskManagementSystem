using Microsoft.EntityFrameworkCore;
using TaskManagement.API.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var databaseProvider = builder.Configuration["DatabaseProvider"];

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (databaseProvider == "Oracle")
    {
        options.UseOracle(builder.Configuration.GetConnectionString("Oracle"));
    }
    else
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSql"));
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
