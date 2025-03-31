namespace TentecimApi.Models
{
    public class RegisterModel
    {
        public string Username { get; set; }      // Ad soyad (user için isteğe bağlı)
        public string Email { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }          // "admin" veya "user"
        public string CompanyName { get; set; }   // admin için zorunlu olabilir

        /// <summary>
        /// Formdan gelen verilerin doğruluğunu kontrol eder
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                errorMessage = "E-posta adresi boş olamaz.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                errorMessage = "Şifre boş olamaz.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Role))
            {
                errorMessage = "Rol seçimi yapılmalı.";
                return false;
            }

            if (Role.ToLower() == "admin")
            {
                if (string.IsNullOrWhiteSpace(CompanyName))
                {
                    errorMessage = "Firma adı boş olamaz.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(Username))
                {
                    errorMessage = "Admin için kullanıcı adı boş olamaz.";
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }
    }
}
