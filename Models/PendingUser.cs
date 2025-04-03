using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace TentecimApi.Models
{
    [Table("pending_users")]
    public class PendingUser : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")] // ✅ Buraya dikkat
        public string Password { get; set; } // ❗ Artık PasswordHash değil

        [Column("phone")]
        public string Phone { get; set; }

        [Column("role")]
        public string Role { get; set; }

        [Column("company_name")]
        public string CompanyName { get; set; }

        [Column("firm_id")]
        public Guid? FirmId { get; set; }

        [Column("parent_admin_id")]
        public Guid? ParentAdminId { get; set; }

        [Column("city")]
        public string City { get; set; }

        [Column("country")]
        public string Country { get; set; }

        [Column("currency")]
        public string Currency { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

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
                else if (string.IsNullOrWhiteSpace(Password)) // ✅ Buraya da dikkat
                    errorMessage = "Şifre boş olamaz.";
            }

            return string.IsNullOrEmpty(errorMessage);
        }
    }
}
