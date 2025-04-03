using TentecimApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DotNetEnv;
using System.IO;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json; // Eklemeye gerek yok ama bilmen adına yazıldı

#region 🌱 Ortam Değişkenlerini Yükle (.env.local)
var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env.local");
Env.Load(envPath);
#endregion

var builder = WebApplication.CreateBuilder(args);

#region 🔧 Yapılandırma (appsettings.json)
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
#endregion

#region 🌐 Servisleri Tanımla
builder.Services.AddSingleton<SupabaseService>();
builder.Services.AddScoped<AuthService>();
// 🔧 System.Text.Json yerine Newtonsoft.Json kullan
builder.Services.AddControllers().AddNewtonsoftJson();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tentecim API",
        Version = "v1"
    });
});
#endregion

#region 🔓 CORS Politikası
var corsPolicyName = "AllowFrontendOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName, policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "https://tentecim-frontend.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
#endregion

var app = builder.Build();

#region 🧪 Swagger (Sadece Geliştirme Ortamı)
/*
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Tentecim API v1");
        options.RoutePrefix = "swagger";
    });
}*/

#endregion

#region 🧱 Middleware Sıralaması Önemli
app.UseHttpsRedirection();       // ✅ bu eklendi
app.UseCors(corsPolicyName);     // ✅ üstte
app.UseAuthorization();
app.MapControllers();

#endregion

#region 🔁 Test Endpoint (Opsiyonel)
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
#endregion

#region ✅ Supabase Bağlantı Testi (Opsiyonel)
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
#endregion

app.Run();

#region 🌤 WeatherForecast record (Demo amaçlı)
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
#endregion
