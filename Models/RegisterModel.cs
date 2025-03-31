namespace TentecimApi.Models
{
    public class RegisterModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; } // "admin" veya "user"

        /// <summary>
        /// Bu fonksiyon, modeldeki zorunlu alanların dolu olup olmadığını kontrol eder.
        /// Hatalı durum varsa `errorMessage` parametresiyle döner.
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                errorMessage = "Kullanıcı adı boş olamaz.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                errorMessage = "E-posta boş olamaz.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                errorMessage = "Şifre boş olamaz.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Role))
            {
                errorMessage = "Rol seçimi zorunludur.";
                return false;
            }

            if (Role.ToLower() == "admin" && string.IsNullOrWhiteSpace(Phone))
            {
                errorMessage = "Admin kullanıcıları için telefon numarası zorunludur.";
                return false;
            }

            errorMessage = null;
            return true;
        }
    }
}
