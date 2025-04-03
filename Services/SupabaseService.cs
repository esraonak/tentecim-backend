using DotNetEnv;
using Supabase;
using Supabase.Postgrest;
using Supabase.Postgrest.Models;
using Supabase.Realtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TentecimApi.Models; // ✅ Modeller burada tanımlı

namespace TentecimApi.Services
{
    public class SupabaseService
    {
        // 🌐 Supabase Client örneği (tek noktadan erişim için private saklanır)
        private readonly Supabase.Client _client;

        #region 🚀 Constructor: Supabase bağlantısını başlatır
        public SupabaseService(IConfiguration configuration)
        {
            // .env.local dosyasından Supabase URL ve KEY al
            var url = Env.GetString("SUPABASE_URL");
            var key = Env.GetString("SUPABASE_ANON_KEY");

            Console.WriteLine("SUPABASE_URL: " + url);
            Console.WriteLine("SUPABASE_ANON_KEY: " + key);

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true
            };

            _client = new Supabase.Client(url, key, options);
            _client.InitializeAsync().Wait(); // ⛔ Geliştirme ortamı için senkron başlatma
        }
        #endregion

        #region 💾 Supabase Client erişimi
        public Supabase.Client GetClient()
        {
            return _client;
        }
        #endregion

        #region ✅ Tüm users tablosunu getir
        public async Task<List<User>> GetAllUsersAsync()
        {
            var response = await _client.From<User>().Get();
            return response.Models;
        }
        #endregion


        #region ➕ INSERT: pending_users tablosuna kullanıcı ekle
        public async Task InsertPendingUserAsync(PendingUser user)
        {
            try
            {
                await _client.From<PendingUser>().Insert(user);
                Console.WriteLine("✅ Kullanıcı pending_users tablosuna eklendi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ HATA: Kullanıcı eklenemedi → " + ex.Message);
                throw;
            }
        }
        #endregion

        #region 📥 GET: Tüm pending_users verilerini listele
        public async Task<List<PendingUser>> GetAllPendingUsersAsync()
        {
            try
            {
                Console.WriteLine("⏳ pending_users tablosu çekiliyor...");
                var response = await _client.From<PendingUser>().Get();
                Console.WriteLine($"✅ {response.Models.Count} kayıt bulundu.");
                return response.Models;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ pending_users verisi alınamadı: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region ❌ DELETE: Belirli ID ile pending_user sil
        public async Task DeletePendingUserAsync(Guid id)
        {
            await _client
                .From<PendingUser>()
                .Where(p => p.Id == id)
                .Delete();
        }
        #endregion

        #region 🔍 GET: Belirli bir pending_user'ı ID ile getir
        public async Task<PendingUser?> GetPendingUserByIdAsync(Guid id)
        {
            try
            {
                var response = await _client
                    .From<PendingUser>()
                    .Where(u => u.Id == id)
                    .Get();

                return response.Models.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Kullanıcı getirilemedi: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region ✅ INSERT: pending_user'dan users tablosuna kayıt oluştur
        public async Task InsertApprovedUserAsync(User newUser)
        {
            try
            {
                await _client.From<User>().Insert(newUser);
                Console.WriteLine("✅ Kullanıcı users tablosuna eklendi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Kullanıcı eklenemedi: {ex.Message}");
                throw;
            }
        }
        #endregion
    }
}
