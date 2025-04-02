using Supabase.Postgrest.Models;
using Supabase.Postgrest.Attributes;
using System;

namespace TentecimApi.Models
{
    [Table("trusted_devices")]
    public class TrustedDevice : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("user_id")]
        public Guid UserId { get; set; }

        [Column("device_token")]
        public string DeviceToken { get; set; }

        [Column("ip_address")]
        public string IpAddress { get; set; }

        [Column("user_agent")]
        public string UserAgent { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }
    }
}
