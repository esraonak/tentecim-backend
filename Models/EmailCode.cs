using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace TentecimApi.Models
{
    [Table("email_codes")]
    public class EmailCode : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
