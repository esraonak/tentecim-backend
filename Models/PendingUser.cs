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

        [Column("password")]
        public string Password { get; set; }

        [Column("company_name")]
        public string CompanyName { get; set; }

        [Column("role")]
        public string Role { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Kullanıcı bilgilerinin geçerli olup olmadığını kontrol eder.
        /// </summary>
        public bool IsValid(out string errorMessage)
        {
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
                errorMessage = "Rol boş olamaz.";
                return false;
            }

            if (Role.ToLower() == "admin")
            {
                if (string.IsNullOrWhiteSpace(Username))
                {
                    errorMessage = "Admin kullanıcıları için kullanıcı adı zorunludur.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(CompanyName))
                {
                    errorMessage = "Firma adı boş olamaz.";
                    return false;
                }
            }

            // user rolünde username boş olabilir, sonradan girilecek
            errorMessage = null;
            return true;
        }
    }
}
