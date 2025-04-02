#region USING
using Microsoft.AspNetCore.Mvc;
using Supabase;
using TentecimApi.Models;
using TentecimApi.Services;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
#endregion

namespace TentecimApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;
        private readonly IConfiguration _configuration; // 👈  appsettings.json erişimi için

        public EmailController(SupabaseService supabaseService, IConfiguration configuration)
        {
            _supabaseService = supabaseService;
            _configuration = configuration;
        }

        [HttpPost("sendcode")]
        public async Task<IActionResult> SendCode([FromBody] EmailRequest request)
        {
            if (!request.IsValid(out var validationError))
                return BadRequest(validationError);

            var client = _supabaseService.GetClient();

            // ✅ Aynı firmaya aynı e-posta daha önce kayıtlı mı?
            try
            {
                var pending = await client
                    .From<PendingUser>()
                    .Filter("email", Supabase.Postgrest.Constants.Operator.Equals, request.Email)
                    .Filter("firm_id", Supabase.Postgrest.Constants.Operator.Equals, request.FirmId.ToString())
                    .Get();

                var users = await client
                    .From<User>()
                    .Filter("email", Supabase.Postgrest.Constants.Operator.Equals, request.Email)
                    .Filter("firm_id", Supabase.Postgrest.Constants.Operator.Equals, request.FirmId.ToString())
                    .Get();

                if (pending.Models.Count > 0 || users.Models.Count > 0)
                {
                    return BadRequest("Bu e-posta bu firmaya zaten kayıtlı.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Veritabanı kontrol hatası: {ex.Message} - {(ex.InnerException?.Message ?? "")}");
            }

            // 🔁 1 dakikalık rate limit
            var existingCode = await client
                .From<EmailCode>()
                .Filter("email", Supabase.Postgrest.Constants.Operator.Equals, request.Email)
                .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(1)
                .Get();

            if (existingCode.Models.Count > 0 && (DateTime.UtcNow - existingCode.Models[0].CreatedAt).TotalMinutes < 1)
            {
                return BadRequest("Lütfen 1 dakika sonra tekrar deneyin.");
            }

            // ✅ Kod oluştur ve kaydet
            var code = new Random().Next(100000, 999999).ToString();
            var createdAt = DateTime.UtcNow;

            try
            {
                await client
                    .From<EmailCode>()
                    .Insert(new List<EmailCode>
                    {
                new EmailCode
                {
                    Email = request.Email,
                    Code = code,
                    CreatedAt = createdAt
                }
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Kod eklenirken hata oluştu: {ex.Message}");
            }

            // ✅ E-posta gönder
            try
            {
                var smtpUser = _configuration["Smtp:User"];
                var smtpPass = _configuration["Smtp:Password"];

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUser, "TENTECIMAPP"),
                    Subject = "Doğrulama Kodunuz",
                    Body = $"Merhaba,\n\nTENTECIMAPP doğrulama kodunuz: {code}\n\nBu kod 5 dakika geçerlidir.",
                    IsBodyHtml = false,
                };

                mailMessage.To.Add(request.Email);
                await smtpClient.SendMailAsync(mailMessage);

                return Ok("Kod gönderildi.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"E-posta gönderilemedi: {ex.Message}");
            }
        }


        [HttpPost("verify")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code))
                return BadRequest("E-posta ve kod boş olamaz.");

            var client = _supabaseService.GetClient();

            var result = await client
                .From<EmailCode>()
                .Where(x => x.Email == request.Email && x.Code == request.Code)
                .Order("created_at", Supabase.Postgrest.Constants.Ordering.Descending)
                .Limit(1)
                .Get();

            if (result.Models.Count == 0)
                return BadRequest("Kod geçersiz.");

            var codeEntry = result.Models[0];
            if ((DateTime.UtcNow - codeEntry.CreatedAt).TotalMinutes > 5)
                return BadRequest("Kodun süresi dolmuş. Lütfen yeniden isteyin.");

            return Ok("Kod doğrulandı.");
        }
    }
}
