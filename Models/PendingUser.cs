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
    }
}
