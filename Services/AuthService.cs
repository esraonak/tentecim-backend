// Services/AuthService.cs
using TentecimApi.Models;
using Supabase.Gotrue;
using Supabase;
using Microsoft.AspNetCore.Identity;
using static Supabase.Postgrest.Constants;
using TentecimApi.Services;

public class AuthService
{
    private readonly SupabaseService _supabaseService;

    public AuthService(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    public async Task<(bool Success, string Message, TentecimApi.Models.User User)> LoginAsync(string email, string password, string role, string deviceToken)
    {
        var client = _supabaseService.GetClient();

        // Kullanıcıyı bul
        var existingUserResponse = await client
            .From<TentecimApi.Models.User>()
            .Filter("email", Operator.Equals, email)
            .Filter("role", Operator.Equals, role)
            .Get();

        var user = existingUserResponse.Models.FirstOrDefault();

        if (user == null)
            return (false, "E-posta ya da rol hatalı.", null);

        // Şifre kontrolü
        var hasher = new PasswordHasher<string>();
        var result = hasher.VerifyHashedPassword(null, user.hashedPassword, password);

        if (result == PasswordVerificationResult.Failed)
            return (false, "Şifre hatalı.", null);

        // TrustedDevice kaydı (opsiyonel)
        if (!string.IsNullOrWhiteSpace(deviceToken))
        {
            var existingDevice = await client
                .From<TrustedDevice>()
                .Filter("device_token", Operator.Equals, deviceToken)
                .Filter("user_id", Operator.Equals, user.Id.ToString())
                .Get();

            if (existingDevice.Models.Count == 0)
            {
                var trustedDevice = new TrustedDevice
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    DeviceToken = deviceToken,
                    IpAddress = "", // IP log'u kontrol edilecek
                    UserAgent = "",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(30),
                    IsActive = true
                };

                await client.From<TrustedDevice>().Insert(trustedDevice);
            }
        }

        return (true, "Giriş başarılı", user);
    }
}
