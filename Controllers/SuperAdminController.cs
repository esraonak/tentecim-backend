// 📌 Gerekli kütüphaneler
using Microsoft.AspNetCore.Mvc;
using TentecimApi.Models;
using TentecimApi.Services;

namespace TentecimApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuperAdminController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;

        // 🧩 DI (Dependency Injection) ile servis enjekte edilir
        public SuperAdminController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }
        // =============================================
        // 📊 4. Dashboard için firma/admin/user sayısı
        // Route: GET /api/superadmin/stats
        // =============================================
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var allUsers = await _supabaseService.GetAllUsersAsync();

                int totalFirms = allUsers
                    .Where(u => u.Role == "admin" || u.Role == "superadmin")
                    .Select(u => u.FirmId)
                    .Distinct()
                    .Count();

                int totalAdmins = allUsers.Count(u => u.Role == "admin");
                int totalUsers = allUsers.Count(u => u.Role == "user");

                return Ok(new
                {
                    totalFirms,
                    totalAdmins,
                    totalUsers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "İstatistikler alınamadı", detail = ex.Message });
            }
        }

        #region ✅ 1. Tüm Onay Bekleyen Kullanıcıları Listele
        // Route: GET /api/superadmin/pending-users
        // Amaç: Supabase içindeki pending_users tablosundan kayıtları çekmek
        [HttpGet("pending-users")]
        public async Task<IActionResult> GetAllPendingUsers()
        {
            try
            {
                var users = await _supabaseService.GetAllPendingUsersAsync();
                return Ok(users); // 200 OK + kullanıcı listesi
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Veri alınamadı",
                    detail = ex.Message
                });
            }
        }
        #endregion

        #region ❌ 2. Kullanıcıyı Reddet (Sil)
        // Route: DELETE /api/superadmin/pending-users/{id}
        // Amaç: Belirli bir ID ile pending_users kaydını silmek
        [HttpDelete("pending-users/{id}")]
        public async Task<IActionResult> DeletePendingUser(Guid id)
        {
            try
            {
                await _supabaseService.DeletePendingUserAsync(id);
                return Ok(new { message = "Kullanıcı silindi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Silme işlemi başarısız",
                    detail = ex.Message
                });
            }
        }
        #endregion

        #region ✅ 3. Kullanıcıyı Onayla (Sisteme Aktar)
        // Route: POST /api/superadmin/approve-user/{id}
        // Amaç:
        //   1. pending_users tablosundan veriyi al
        //   2. users tablosuna ekle
        //   3. pending_users'tan sil
        [HttpPost("approve-user/{id}")]
        public async Task<IActionResult> ApprovePendingUser(Guid id)
        {
            try
            {
                // 1. ID ile pending kayıt çekilir
                var pendingUser = await _supabaseService.GetPendingUserByIdAsync(id);
                if (pendingUser == null)
                    return NotFound(new { message = "Kullanıcı bulunamadı." });

                // 2. User modeline dönüştürülür
                var newUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = pendingUser.Username,
                    Email = pendingUser.Email,
                    Phone = pendingUser.Phone,
                    hashedPassword = pendingUser.Password,
                    Role = pendingUser.Role,
                    CompanyName = pendingUser.CompanyName,
                    FirmId = pendingUser.FirmId,
                    ParentAdminId = pendingUser.ParentAdminId,
                    Country = pendingUser.Country,
                    City = pendingUser.City,
                    Currency = pendingUser.Currency,
                    CreatedAt = DateTime.UtcNow
                };

                // 3. users tablosuna eklenir
                await _supabaseService.InsertApprovedUserAsync(newUser);

                // 4. pending_users kaydı silinir
                await _supabaseService.DeletePendingUserAsync(id);

                // (Opsiyonel) 📧 Bilgilendirme e-postası gönderilebilir

                return Ok(new { message = "Kullanıcı onaylandı ve sisteme eklendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Onaylama işlemi başarısız",
                    detail = ex.Message
                });
            }
        }
        #endregion
    }
}
