using Microsoft.AspNetCore.Mvc;
using TentecimApi.Models;
using TentecimApi.Services;

namespace TentecimApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FirmsController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;

        public FirmsController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        /// <summary>
        /// 📦 Tüm aktif firmaları listeler (Dropdown için).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var client = _supabaseService.GetClient();

                var result = await client
                    .From<Firm>()
                    .Filter("is_active", Supabase.Postgrest.Constants.Operator.Equals, true)
                    .Get();

                return Ok(result.Models);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Firmalar alınırken hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// 📌 Belirli bir firmanın detaylarını döner (ülke, şehir, para birimi).
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFirmById(Guid id)
        {
            try
            {
                var client = _supabaseService.GetClient();
                var result = await client
                    .From<Firm>()
                    .Filter("id", Supabase.Postgrest.Constants.Operator.Equals, id)
                    .Get();

                var firm = result.Models.FirstOrDefault();
                if (firm == null)
                    return NotFound("Firma bulunamadı.");

                return Ok(new
                {
                    supportedCountries = firm.SupportedCountries,
                    supportedCurrencies = firm.SupportedCurrencies,
                    supportedCities = firm.SupportedCities
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Firma detayları alınırken hata oluştu: {ex.Message}");
            }
        }

        /// <summary>
        /// 👥 Belirli bir firmaya bağlı tüm adminleri döner.
        /// </summary>
        [HttpGet("{firmId}/admins")]
        public async Task<IActionResult> GetAdminsByFirm(Guid firmId)
        {
            try
            {
                var client = _supabaseService.GetClient();

                var result = await client
                    .From<User>()
                    .Filter("role", Supabase.Postgrest.Constants.Operator.Equals, "admin")
                    .Filter("firm_id", Supabase.Postgrest.Constants.Operator.Equals, firmId.ToString())
                    .Get();

                return Ok(result.Models);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Admin listesi alınırken hata: {ex.Message}");
            }
        }
    }
}
