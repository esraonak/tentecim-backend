using DotNetEnv;
using Supabase;
using Supabase.Gotrue;
using Supabase.Postgrest;
using Supabase.Postgrest.Models;
using Supabase.Realtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TentecimApi.Models; // ✅ PendingUser modelin burada tanımlı

namespace TentecimApi.Services
{
    public class SupabaseService
    {
        // 🌐 Supabase Client örneği (tek noktadan erişim için private saklanır)
        private readonly Supabase.Client _client;

        #region Constructor
        // 🧩 Constructor: Supabase bağlantısını başlatır
        public SupabaseService(IConfiguration configuration)
        {
            // .env.local dosyasından Supabase URL ve Key bilgisini alır
            var url = Env.GetString("SUPABASE_URL");
            var key = Env.GetString("SUPABASE_ANON_KEY");

            Console.WriteLine("SUPABASE_URL: " + url);
            Console.WriteLine("SUPABASE_ANON_KEY: " + key);

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true
            };

            // Supabase bağlantısını kurar ve başlatır
            _client = new Supabase.Client(url, key, options);
            _client.InitializeAsync().Wait(); // ⛔ Senkron başlatılır (geliştirici ortamı için uygundur)
        }
        #endregion

        #region Client Erişimi
        // 💾 Supabase Client'a dışarıdan erişim sağlamak için kullanılır
        public Supabase.Client GetClient()
        {
            return _client;
        }
        #endregion

        #region INSERT → pending_users tablosuna yeni kullanıcı ekleme
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
        #endregion

        #region GET → Tüm pending_users verilerini listele
        public async Task<List<PendingUser>> GetAllPendingUsersAsync()
        {
            var response = await _client.From<PendingUser>().Get();
            return response.Models;
        }
        #endregion

        #region DELETE → ID'ye göre pending user sil
        public async Task DeletePendingUserAsync(Guid id)
        {
            await _client
                .From<PendingUser>()
                .Where(p => p.Id == id)
                .Delete();
        }

        #endregion
    }
}
