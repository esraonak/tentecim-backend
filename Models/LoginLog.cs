using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace TentecimApi.Models
{
    [Table("login_logs")]
    public class LoginLog : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("status")]
        public string Status { get; set; } // "success" | "failed"

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
