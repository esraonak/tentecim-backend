// 📁 SuperAdminController.cs
// ✅ Amaç: SuperAdmin kullanıcısının bekleyen kayıtları görmesi ve silmesi için endpointler oluşturur.

using Microsoft.AspNetCore.Mvc;
using TentecimApi.Models;
using TentecimApi.Services;

namespace TentecimApi.Controllers
{
    // ✅ Controller niteliği ve route bilgisi
    [ApiController]
    [Route("api/[controller]")]
    public class SuperAdminController : ControllerBase
    {
        // 🔧 Supabase servisine erişim
        private readonly SupabaseService _supabaseService;

        // ✅ Constructor - Bağlı servisi içeri alır
        public SuperAdminController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        // ==============================
        // 📌 1. Bekleyen tüm kullanıcıları getir
        // Endpoint: GET /api/superadmin/pending-users
        // ==============================
        [HttpGet("pending-users")]
        public async Task<IActionResult> GetAllPendingUsers()
        {
            try
            {
                // ✅ Servisten veriyi çek
                var users = await _supabaseService.GetAllPendingUsersAsync();
                return Ok(users); // ∆ 200 OK
            }
            catch (Exception ex)
            {
                // ❌ Hata durumunda 500 dön
                return StatusCode(500, new { error = "Veri alınamadı", detail = ex.Message });
            }
        }

        // ==============================
        // 📌 2. Kullanıcıyı sil (reddet)
        // Endpoint: DELETE /api/superadmin/pending-users/{id}
        // ==============================
        [HttpDelete("pending-users/{id}")]
        public async Task<IActionResult> DeletePendingUser(Guid id)
        {
            try
            {
                // ✅ Servis üzerinden silme işlemi
                await _supabaseService.DeletePendingUserAsync(id);
                return Ok(new { message = "Kullanıcı silindi." }); // ∆ 200 OK
            }
            catch (Exception ex)
            {
                // ❌ Hata durumunda bilgiyle birlikte 500 dön
                return StatusCode(500, new { error = "Silme işlemi başarısız", detail = ex.Message });
            }
        }
    }
}
