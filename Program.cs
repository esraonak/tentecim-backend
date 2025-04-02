using TentecimApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DotNetEnv;
using System.IO;
using Microsoft.OpenApi.Models;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env.local");
Env.Load(envPath); // .env.local dosyasını yükle

var builder = WebApplication.CreateBuilder(args);

// ✅ SMTP bilgilerini buradan okuyabilmek için appsettings.json yükleniyor
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// 🌐 Servisleri ekle
builder.Services.AddSingleton<SupabaseService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // ✅ Swagger için gerekli

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tentecim API",
        Version = "v1"
    });
});

// ✅ CORS Politikası (Canlı ortam için Vercel domain tanımlı)
var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins, policy =>
    {
        policy
            .WithOrigins(
                "https://tentecim-frontend.vercel.app",  // 🌍 Canlı frontend domainin
                "http://localhost:3000"                  // 🧪 Lokal geliştirme
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// ✅ Swagger sadece geliştirme ortamında çalışır
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tentecim API v1");
        options.RoutePrefix = "swagger";
    });
}

// ✅ Middleware sıralaması önemli
app.UseCors(MyAllowSpecificOrigins); // İlk sırada olmalı
app.UseAuthorization();
app.MapControllers(); // Tüm Controller'ları aktif et

// ✅ Örnek endpoint (geliştirme/test amaçlı)
app.MapGet("/weatherforecast", () =>
{
    var summaries = new[] { "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching" };
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        )).ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// ✅ Supabase test endpoint
app.MapGet("/supabase-check", async (SupabaseService supabaseService) =>
{
    try
    {
        var client = supabaseService.GetClient();
        var user = client.Auth.CurrentUser;

        return Results.Ok(new
        {
            Message = "Supabase bağlantısı başarılı!",
            UserId = user?.Id ?? "Henüz giriş yapılmamış"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem("Supabase bağlantısı başarısız: " + ex.Message);
    }
});

app.Run();

// ✅ WeatherForecast record tanımı
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
