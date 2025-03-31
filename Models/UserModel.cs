using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace TentecimApi.Models
{
    [Table("users")] // Supabase'deki tablo adı
    public class User : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("phone")]
        public string Phone { get; set; }

        [Column("role")]
        public string Role { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
