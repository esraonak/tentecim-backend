using Microsoft.AspNetCore.Mvc;
using TentecimApi.Services;
using TentecimApi.Models;

namespace TentecimApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PendingUsersController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;

        // 🧩 Supabase servisini constructor ile içeri alıyoruz
        public PendingUsersController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        // ✅ Tüm onay bekleyen kullanıcıları getir
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PendingUser>>> GetAll()
        {
            var users = await _supabaseService.GetAllPendingUsersAsync();
            return Ok(users);
        }

        // ❌ Belirli bir kullanıcıyı sil (örneğin reddedildiğinde)
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _supabaseService.DeletePendingUserAsync(id);
            return NoContent();
        }
    }
}
