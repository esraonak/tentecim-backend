﻿#region USING
using Microsoft.AspNetCore.Mvc;
using Supabase;
using TentecimApi.Models;
using TentecimApi.Services;
using System.Net;
using System.Net.Mail;


#endregion

namespace TentecimApi.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly SupabaseService _supabaseService;

        public EmailController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }

        // 📩 E-posta adresine doğrulama kodu gönder ve veritabanına kaydet
        [HttpPost("sendcode")]
        public async Task<IActionResult> SendCode([FromBody] EmailRequest request)
        {
            var client = _supabaseService.GetClient();
            var code = new Random().Next(100000, 999999).ToString();
            var createdAt = DateTime.UtcNow;

            // Supabase'e kodu kaydet
            var result = await client
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

            // ✅ SMTP AYARLARI
            var smtpClient = new SmtpClient("smtp.gmail.com") // Örneğin Gmail
            {
                Port = 587,
                Credentials = new NetworkCredential("524esrasahin@gmail.com", "tbtdfeftkvmzihyy"),
                EnableSsl = true,
            };
            // ✅ Marka adı tanımlı bir gönderim
       
           
            // ✅ E-POSTA GÖNDER
            var mailMessage = new MailMessage
            {
                From = new MailAddress("524esrasahin@gmail.com", "TENTECIMAPP"),
                Subject = "Doğrulama Kodunuz",
                Body = $"Merhaba,\n\nTENTECIMAPP Doğrulama kodunuz: {code}\n\nBu kod 5 dakika içinde geçerlidir.",
                IsBodyHtml = false,
            };

            mailMessage.To.Add(request.Email);

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                return Ok("Kod e-posta ile gönderildi.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"E-posta gönderilemedi: {ex.Message}");
            }
        }


        // ✅ Kod Doğrulama (email + code ile)
        [HttpPost("verify")]
        public async Task<IActionResult> VerifyCode([FromBody] VerifyRequest request)
        {
            var client = _supabaseService.GetClient();

            // Kodun doğru ve güncel olup olmadığını kontrol et
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
