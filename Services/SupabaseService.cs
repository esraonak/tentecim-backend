using DotNetEnv;
using Supabase;
using Supabase.Gotrue;
using Supabase.Postgrest;
using Supabase.Postgrest.Models;
using Supabase.Realtime;
using System;
using System.Threading.Tasks;
using TentecimApi.Models; // PendingUser modelin burada

namespace TentecimApi.Services
{
    public class SupabaseService
    {
        private readonly Supabase.Client _client;

        // 🧩 Constructor: Bağlantıyı başlatıyor
        public SupabaseService(IConfiguration configuration)
        {
            // .env.local dosyasından bilgileri al
            var url = Env.GetString("SUPABASE_URL");
            var key = Env.GetString("SUPABASE_ANON_KEY");

            Console.WriteLine("SUPABASE_URL: " + url);
            Console.WriteLine("SUPABASE_ANON_KEY: " + key);

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true
            };

            // Supabase bağlantısını kur
            _client = new Supabase.Client(url, key, options);
            _client.InitializeAsync().Wait(); // Bağlantıyı senkron başlat
        }

        // 💾 Supabase Client erişimi
        public Supabase.Client GetClient()
        {
            return _client;
        }

        // ✅ pending_users tablosuna veri ekleme
        public async Task InsertPendingUserAsync(PendingUser user)
        {
            try
            {
                var response = await _client.From<PendingUser>().Insert(user);
                Console.WriteLine("✅ Kullanıcı pending_users tablosuna eklendi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ HATA: Kullanıcı eklenemedi → " + ex.Message);
                throw;
            }
        }
    }
}
