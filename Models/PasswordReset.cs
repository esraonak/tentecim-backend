using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace TentecimApi.Models
{
    [Table("password_resets")]
    public class PasswordReset : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("role")]
        public string Role { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
