// 📌 Gerekli kütüphaneler
using Microsoft.AspNetCore.Mvc;
using TentecimApi.Models;
using TentecimApi.Services;
using Microsoft.AspNetCore.Identity;
using static Supabase.Postgrest.Constants;

namespace TentecimApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuperAdminController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;
        private readonly AuthService _authService;

        // 🧩 DI (Dependency Injection) ile servisler enjekte edilir
        public SuperAdminController(SupabaseService supabaseService, AuthService authService)
        {
            _supabaseService = supabaseService;
            _authService = authService;
        }

        // =============================================
        // 📊 Dashboard İstatistikleri
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

        // =============================================
        // 🔑 SuperAdmin Login
        // Route: POST /api/superadmin/login
        // =============================================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _authService.LoginAsync(model.Email, model.Password, "superadmin", model.DeviceToken);

            if (!result.Success)
                return Unauthorized(result.Message);

            return Ok(new
            {
                message = result.Message,
                user = new
                {
                    id = result.User.Id,
                    email = result.User.Email,
                    username = result.User.Username,
                    role = result.User.Role
                }
            });
        }

        #region ✅ 1. Tüm Onay Bekleyen Kullanıcılar
        [HttpGet("pending-users")]
        public async Task<IActionResult> GetAllPendingUsers()
        {
            try
            {
                var users = await _supabaseService.GetAllPendingUsersAsync();

                var cleanedUsers = users.Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Phone,
                    u.Role,
                    u.Country,
                    u.City,
                    u.Currency,
                    u.FirmId,
                    u.ParentAdminId,
                    u.CompanyName,
                    u.CreatedAt
                });

                return Ok(cleanedUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Veri alınamadı", detail = ex.Message });
            }
        }
        #endregion

        #region ❌ 2. Onay Bekleyen Kullanıcıyı Sil
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
                return StatusCode(500, new { error = "Silme başarısız", detail = ex.Message });
            }
        }
        #endregion

        #region ✅ 3. Kullanıcıyı Onayla
        [HttpPost("approve-user/{id}")]
        public async Task<IActionResult> ApprovePendingUser(Guid id)
        {
            try
            {
                var pendingUser = await _supabaseService.GetPendingUserByIdAsync(id);
                if (pendingUser == null)
                    return NotFound(new { message = "Kullanıcı bulunamadı." });

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

                await _supabaseService.InsertApprovedUserAsync(newUser);
                await _supabaseService.DeletePendingUserAsync(id);

                return Ok(new { message = "Kullanıcı onaylandı ve sisteme eklendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Onaylama hatası", detail = ex.Message });
            }
        }
        #endregion
    }
}