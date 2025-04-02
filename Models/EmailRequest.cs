namespace TentecimApi.Models
{
    public class EmailRequest
    {
        // Kullanıcının e-posta adresi
        public string Email { get; set; }

        // Hangi firmaya kayıt olmaya çalıştığı (zorunlu hale getirildi)
        public Guid? FirmId { get; set; }
        public string Role { get; set; }
        // Validasyon metodu – hem e-posta hem firma kontrolü yapılır
        public bool IsValid(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(Email))
            {
                errorMessage = "E-posta boş olamaz.";
                return false;
            }

            if (FirmId == null)
            {
                errorMessage = "Firma seçimi yapılmadı.";
                return false;
            }

            errorMessage = "";
            return true;
        }
    }
}
