#region GEREKLİ USING TANIMLARI
using Microsoft.AspNetCore.Mvc; // ASP.NET Core MVC özelliklerini kullanmak için
using TentecimApi.Services;     // SupabaseService'e erişim sağlamak için
using TentecimApi.Models;       // User modeline erişim sağlamak için
#endregion

namespace TentecimApi.Controllers
{
    #region CONTROLLER TANIMI VE ROUTE
    [ApiController] // Bu sınıfın bir API controller olduğunu belirtir
    [Route("api/[controller]")] // Bu controller'a 'api/auth' üzerinden erişim sağlanır
    public class AuthController : ControllerBase
    #endregion
    {
        #region DEPENDENCY INJECTION - SERVİS ALANI
        private readonly SupabaseService _supabaseService; // Supabase işlemleri için servis

        // Constructor: DI (Dependency Injection) ile SupabaseService enjekte edilir
        public AuthController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }
        #endregion

        #region Register Metodu - Admin Kayıt + E-Posta Doğrulama
        [HttpPost("register")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] PendingUser user)
        {
            try
            {
                var client = _supabaseService.GetClient();

                // 1. E-posta daha önce kayıtlı mı? (pending_users tablosunda kontrol)
                var existingPending = await client
                    .From<PendingUser>()
                    .Where(p => p.Email == user.Email)
                    .Get();

                if (existingPending.Models.Count > 0)
                {
                    return BadRequest("Bu e-posta zaten onay bekleyenler listesinde var.");
                }

                // 2. E-posta daha önce onaylanmış mı? (users tablosunda kontrol)
                var existingUser = await client
                    .From<User>()
                    .Where(p => p.Email == user.Email)
                    .Get();

                if (existingUser.Models.Count > 0)
                {
                    return BadRequest("Bu e-posta ile zaten kayıt yapılmış.");
                }

                // 3. Supabase Auth ile e-posta doğrulamalı hesap oluştur
                var signUpResponse = await client.Auth.SignUp(user.Email, user.Password);

                if (signUpResponse.User == null)
                {
                    return BadRequest("Kayıt sırasında bir hata oluştu (Auth).");
                }

                // 4. Supabase’in otomatik UUID üretmesini sağla
                user.Id = default;
                user.CreatedAt = DateTime.UtcNow;

                var insertResponse = await client
                    .From<PendingUser>()
                    .Insert(user);

                if (insertResponse.Models != null)
                {
                    return Ok(new
                    {
                        message = "Kayıt başarılı! 📧 E-postanı doğrula ve yönetici/superadmin onayını bekle."
                    });
                }

                return BadRequest("Kayıt veritabanına eklenemedi.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Sunucu hatası: {ex.Message}");
            }
        }

        #endregion

    }
}
