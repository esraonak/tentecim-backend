using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Text.Json;
namespace TentecimApi.Models
{
    [Table("firms")]
    public class Firm : BaseModel
    {
        [PrimaryKey("id", false)]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("active_until")]
        public DateTime ActiveUntil { get; set; }

        [Column("max_admins")]
        public int MaxAdmins { get; set; }

        [Column("max_users_per_admin")]
        public int MaxUsersPerAdmin { get; set; }

        [Column("contact_name")]
        public string ContactName { get; set; }

        [Column("contact_email")]
        public string ContactEmail { get; set; }

        [Column("contact_phone")]
        public string ContactPhone { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("supported_countries")]
        public string[] SupportedCountries { get; set; }

        [Column("supported_currencies")]
        public string[] SupportedCurrencies { get; set; }

        [Column("supported_cities")]
        public string SupportedCitiesJson { get; set; }

        public Dictionary<string, List<string>> SupportedCities
        {
            get => string.IsNullOrWhiteSpace(SupportedCitiesJson)
                ? new()
                : JsonSerializer.Deserialize<Dictionary<string, List<string>>>(SupportedCitiesJson);

            set => SupportedCitiesJson = JsonSerializer.Serialize(value);
        }

    }
}
