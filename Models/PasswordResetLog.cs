using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace TentecimApi.Models
{
    [Table("password_reset_logs")]
    public class PasswordResetLog : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("role")]
        public string Role { get; set; }

        [Column("firm_id")]
        public Guid? FirmId { get; set; }

        [Column("action")]
        public string Action { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("message")]
        public string Message { get; set; }

        [Column("ip_address")]
        public string IpAddress { get; set; }

        [Column("user_agent")]
        public string UserAgent { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
