namespace TentecimApi.Models
{
    public class RegisterModel
    {
        public string Username { get; set; }             // step 4 - Ad Soyad
        public string Email { get; set; }                // step 3
        public string Password { get; set; }             // step 4
        public string Role { get; set; }                 // step 1
        public Guid? FirmId { get; set; }                // step 1
        public Guid? ParentAdminId { get; set; }         // step 2

        public string City { get; set; }                 // step 4
        public string Country { get; set; }              // step 4
        public string Currency { get; set; }             // step 4

        // Step adımlarına göre validasyon
        public bool IsValid(out string errorMessage, int step)
        {
            errorMessage = "";

            if (step >= 1)
            {
                if (string.IsNullOrWhiteSpace(Role))
                    errorMessage = "Rol seçimi yapılmalı.";
                else if (Role.ToLower() == "admin" && FirmId == null)
                    errorMessage = "Admin için firma seçimi yapılmalı.";
                else if (Role.ToLower() == "user" && FirmId == null)
                    errorMessage = "Kullanıcı için firma seçimi yapılmalı.";
            }

            if (step >= 2)
            {
                if (Role.ToLower() == "user" && ParentAdminId == null)
                    errorMessage = "Kullanıcı için admin seçimi yapılmalı.";
            }

            if (step >= 3)
            {
                if (string.IsNullOrWhiteSpace(Email))
                    errorMessage = "E-posta adresi girilmeli.";
            }

            if (step >= 4)
            {
                if (string.IsNullOrWhiteSpace(Username))
                    errorMessage = "Kullanıcı adı boş olamaz.";
                else if (string.IsNullOrWhiteSpace(Password))
                    errorMessage = "Şifre boş olamaz.";
                else if (Password.Length < 6)
                    errorMessage = "Şifre en az 6 karakter olmalıdır.";
            }

            return string.IsNullOrEmpty(errorMessage);
        }
    }
}
