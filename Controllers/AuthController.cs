#region USING
using Microsoft.AspNetCore.Mvc; // ASP.NET Core MVC özelliklerini kullanmak için
using TentecimApi.Services;     // SupabaseService'e erişim sağlamak için
using TentecimApi.Models;       // User ve RegisterModel'e erişim sağlamak için
using Supabase.Gotrue;          // Supabase Auth işlemleri için
using static Supabase.Postgrest.Constants; // Supabase filtre sabitleri
using Microsoft.AspNetCore.Identity;
using System.Net.Mail;
using System.Net;

#endregion

namespace TentecimApi.Controllers
{
    #region CONTROLLER TANIMI VE ROUTE
    [ApiController] // Bu sınıfın bir API controller olduğunu belirtir
    [Route("api/[controller]")] // Bu controller'a 'api/auth' üzerinden erişim sağlanır
    public class AuthController : ControllerBase
    #endregion
    {
        private readonly IConfiguration _configuration;
        #region DEPENDENCY INJECTION - SERVİS ALANI
        private readonly SupabaseService _supabaseService; // Supabase işlemleri için servis

        public AuthController(SupabaseService supabaseService)
        {
            _supabaseService = supabaseService;
        }
        #endregion

        #region REGISTER METODU - Yeni kullanıcı kaydı başlatılır (Admin/User)

        /// <summary>
        /// Yeni kullanıcı kaydı alır. Supabase Auth üzerinden kullanıcı oluşturur,
        /// ardından pending_users tablosuna kayıt atar. (Admin veya User için)
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            try
            {
                // 🛡️ 1. Validasyon (görsel akış adımlarına uygun kontrol)
                if (!model.IsValid(out var validationMessage, step: 4))
                    return BadRequest(validationMessage);

                var client = _supabaseService.GetClient();

                // 🔍 2. pending_users tablosunda e-posta kontrolü
                try
                {
                    var existingPending = await client
                        .From<PendingUser>()
                        .Filter("email", Operator.Equals, model.Email)
                        .Get();

                    if (existingPending.Models.Count > 0)
                        return BadRequest("Bu e-posta zaten onay bekleyenler listesinde var.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"pending_users kontrolü sırasında hata oluştu: {ex.Message}");
                }

                // 🔍 3. users tablosunda e-posta kontrolü
                try
                {
                    var existingUser = await client
                        .From<TentecimApi.Models.User>()
                        .Filter("email", Operator.Equals, model.Email)
                        .Get();

                    if (existingUser.Models.Count > 0)
                        return BadRequest("Bu e-posta ile zaten kayıt yapılmış.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"users kontrolü sırasında hata oluştu: {ex.Message}");
                }

                // 🔐 4. Supabase Auth ile kullanıcı oluştur
                Session signUpResponse;
                try
                {
                    signUpResponse = await client.Auth.SignUp(model.Email, model.Password);

                    if (signUpResponse.User == null)
                        return BadRequest("Kayıt sırasında bir hata oluştu (Auth). Kullanıcı oluşturulamadı.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Supabase Auth kayıt hatası: {ex.Message}");
                }
                // 🧠 Şifreyi hashleyelim
                var hasher = new PasswordHasher<string>();
                string hashedPassword = hasher.HashPassword(null, model.Password);
                // 🧾 5. pending_users tablosuna ekleme yapılır
                var newUser = new PendingUser
                {
                    Id = default,
                    Username = model.Username,
                    Email = model.Email,
                    Password = hashedPassword,
                    Role = model.Role,
                    FirmId = model.FirmId,
                    ParentAdminId = model.ParentAdminId,
                    City = model.City,
                    Country = model.Country,
                    Currency = model.Currency,
                    CreatedAt = DateTime.UtcNow
                };

                try
                {
                    var insertResponse = await client
                        .From<PendingUser>()
                        .Insert(newUser);

                    if (insertResponse.Models != null)
                    {
                        return Ok(new
                        {
                            message = "Kayıt başarılı! 📧 Lütfen e-postanı doğrula ve SuperAdmin onayını bekle."
                        });
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Veritabanına kayıt sırasında hata oluştu: {ex.Message}");
                }

                // ❌ Normalde bu noktaya gelinmemeli
                return BadRequest("Kayıt veritabanına eklenemedi.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Genel kayıt hatası: {ex.Message}");
            }
        }

        #endregion

       


        #region FORGOT PASSWORD - Şifre sıfırlama kodu gönderimi

        /// <summary>
        /// Kullanıcının e-posta ve rol bilgisine göre şifre sıfırlama kodu gönderilir.
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest model)
        {
            try
            {
                var client = _supabaseService.GetClient();

                // 🔍 Kullanıcıyı kontrol et
                var userResponse = await client
                    .From<TentecimApi.Models.User>()
                    .Filter("email", Operator.Equals, model.Email)
                    .Filter("role", Operator.Equals, model.Role)
                    .Get();

                var user = userResponse.Models.FirstOrDefault();
                if (user == null)
                    return NotFound("Bu bilgilere ait kullanıcı bulunamadı.");

                // 🔐 Kod üret
                var code = new Random().Next(100000, 999999).ToString(); // 6 haneli kod
                var expiresAt = DateTime.UtcNow.AddMinutes(10);

                // 💾 password_resets tablosuna kayıt
                var resetRecord = new PasswordReset
                {
                    Id = Guid.NewGuid(),
                    Email = model.Email,
                    Role = model.Role,
                    Code = code,
                    ExpiresAt = expiresAt,
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<PasswordReset>().Insert(resetRecord);

                // 📝 Geliştirme süreci için log'a yaz
                Console.WriteLine($"[Şifre Sıfırla] {model.Email} ({model.Role}) → Kod: {code} (geçerlilik: 10dk)");

                return Ok(new
                {
                    message = "Şifre sıfırlama kodu e-posta adresinize gönderildi.",
                    // code = code // test aşamasında gösterilebilir, canlıda gösterilmez
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Şifre sıfırlama isteği başarısız: {ex.Message}");
            }
        }


        #endregion

        #region RESET PASSWORD - Şifre sıfırlama kodunu doğrula ve şifreyi güncelle

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
        {
            try
            {
                var client = _supabaseService.GetClient();

                // 🔍 1. Kodun geçerli olup olmadığını kontrol et
                var codeCheck = await client
                    .From<PasswordReset>()
                    .Filter("email", Operator.Equals, model.Email)
                    .Filter("role", Operator.Equals, model.Role)
                    .Filter("code", Operator.Equals, model.Code)
                    .Get();

                var codeRecord = codeCheck.Models.FirstOrDefault();

                if (codeRecord == null)
                    return BadRequest("Kod geçersiz veya bulunamadı.");

                if (codeRecord.ExpiresAt < DateTime.UtcNow)
                    return BadRequest("Kodun süresi dolmuş.");

                // 🔐 2. Şifreyi hashle
                var hasher = new PasswordHasher<string>();
                var hashedPassword = hasher.HashPassword(null, model.NewPassword);

                // 🔄 3. Kullanıcının şifresini güncelle
                var userResponse = await client
                    .From<TentecimApi.Models.User>()
                    .Filter("email", Operator.Equals, model.Email)
                    .Filter("role", Operator.Equals, model.Role)
                    .Get();

                var user = userResponse.Models.FirstOrDefault();
                if (user == null)
                    return NotFound("Kullanıcı bulunamadı.");

                user.hashedPassword = hashedPassword;
                await client.From<TentecimApi.Models.User>().Update(user);
                // ✅ Şifre sıfırlama logunu yaz
                await LogPasswordReset(
                    email: model.Email,
                    role: model.Role,
                    firmId: user.FirmId, // null olabilir, sorun değil
                    action: "reset_success",
                    status: "success",
                    message: "Şifre başarıyla güncellendi."
                );
                // 🧹 4. Kod kaydını temizle (isteğe bağlı)
                await client.From<PasswordReset>().Delete(codeRecord);

                return Ok(new
                {
                    message = "Şifreniz başarıyla güncellendi. Giriş yapabilirsiniz."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Şifre sıfırlama başarısız: {ex.Message}");
            }
        }

        #endregion

        #region logın loglar ıcın yardımcı 
        private async Task LogLogin(string email, string status, string message)
        {
            try
            {
                var client = _supabaseService.GetClient();
                var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault()
                         ?? HttpContext.Connection.RemoteIpAddress?.ToString();
                var agent = Request.Headers["User-Agent"].ToString();

                var log = new LoginLog
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Status = status,
                    Message = message,
                    IpAddress = ip,
                    UserAgent = agent,
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<LoginLog>().Insert(log);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login log yazılamadı: {ex.Message}");
            }
        }

        #endregion

        #region SIFRE SIFIRLAMA LOG
        private async Task LogPasswordReset(string email, string role, Guid? firmId, string action, string status, string message)
        {
            try
            {
                var client = _supabaseService.GetClient();
                var ip = Request.Headers["X-Forwarded-For"].FirstOrDefault()
                         ?? HttpContext.Connection.RemoteIpAddress?.ToString();
                var agent = Request.Headers["User-Agent"].ToString();

                var log = new PasswordResetLog
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    Role = role,
                    FirmId = firmId,
                    Action = action,
                    Status = status,
                    Message = message,
                    IpAddress = ip,
                    UserAgent = agent,
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<PasswordResetLog>().Insert(log);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Şifre sıfırlama logu yazılamadı: {ex.Message}");
            }
        }

        #endregion

        [HttpPost("send-reset-code")]
        public async Task<IActionResult> SendResetCode([FromBody] EmailRequest request)
        {
            var client = _supabaseService.GetClient();

            // 🧠 Kullanıcı kayıtlı mı?
            var userResponse = await client
                .From<TentecimApi.Models.User>()
                .Filter("email", Operator.Equals, request.Email)
                .Filter("role", Operator.Equals, request.Role)
                .Get();

            var user = userResponse.Models.FirstOrDefault();
            if (user == null)
                return BadRequest("Bu e-posta ile kayıtlı bir kullanıcı bulunamadı.");

            // 🔁 Rate limit kontrolü
            var lastCode = await client
                .From<PasswordReset>()
                .Filter("email", Operator.Equals, request.Email)
                .Order("created_at", Ordering.Descending)
                .Limit(1)
                .Get();

            if (lastCode.Models.Count > 0 && (DateTime.UtcNow - lastCode.Models[0].CreatedAt).TotalMinutes < 1)
                return BadRequest("Lütfen 1 dakika sonra tekrar deneyin.");

            // ✅ Kod üret
            var code = new Random().Next(100000, 999999).ToString();
            var reset = new PasswordReset
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                Code = code,
                Role = request.Role,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5)
            };

            await client.From<PasswordReset>().Insert(reset);

            // ✅ Kod e-posta gönderimi
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
                    Subject = "Şifre Sıfırlama Kodunuz",
                    Body = $"Şifre sıfırlama kodunuz: {code}\nBu kod 5 dakika geçerlidir.",
                    IsBodyHtml = false,
                };

                mailMessage.To.Add(request.Email);
                await smtpClient.SendMailAsync(mailMessage);

                // ✅ Log ekle
                await LogPasswordReset(
                    email: request.Email,
                    role: request.Role,
                    firmId: user.FirmId,
                    action: "code_sent",
                    status: "success",
                    message: "Şifre sıfırlama kodu gönderildi"
                );

                return Ok("Kod gönderildi.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Kod gönderildi ama e-posta başarısız: {ex.Message}");
            }
        }



    }
}