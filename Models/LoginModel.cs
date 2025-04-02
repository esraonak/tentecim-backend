namespace TentecimApi.Models
{
    public class LoginModel
    {
        public string Email { get; set; }            // Kullanıcının e-posta adresi
        public string Password { get; set; }         // Düz metin şifre
        public string Role { get; set; }             // superadmin / admin / user

        public bool RememberMe { get; set; }         // Beni hatırla kutucuğu (true/false)
        public string DeviceToken { get; set; }      // Cihazın UUID/Fingerprint değeri
    }
}
