using System.Text;
using System.Text.Json;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using TelegramVPNBot.Models;

namespace TelegramVPNBot.Services
{
    public static class OutlineVpnService
    {
        private static readonly HttpClient _httpClient;
        private static readonly string _apiUrl;

        static OutlineVpnService()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            _httpClient = new HttpClient(handler);
            _apiUrl = configuration.GetValue<string>("OutLine:Url")!.TrimEnd('/');
        }

        public static async Task<List<VpnKey>?> GetKeysAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/access-keys");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var keysResponse = JsonSerializer.Deserialize<GetKeysResponse>(json);
                return keysResponse?.accessKeys;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                throw;
            }
        }

        public static async Task<VpnKey?> CreateKeyAsync(string name)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { name }),
                Encoding.UTF8,
                "application/json");
            var response = await _httpClient.PostAsync($"{_apiUrl}/access-keys", content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var newKey = JsonSerializer.Deserialize<VpnKey>(json);
            return newKey;
        }


        public static async Task DeleteKeyAsync(string keyId)
        {
            var response = await _httpClient.DeleteAsync($"{_apiUrl}/access-keys/{keyId}");
            response.EnsureSuccessStatusCode();
        }

        public static async Task UpdateKeyNameAsync(string keyId, string newName)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { name = newName }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync($"{_apiUrl}/access-keys/{keyId}/name", content);
            response.EnsureSuccessStatusCode();
        }

        public static async Task<VpnServerMetrics?> GetMetricsAsync()
        {
            var response = await _httpClient.GetAsync($"{_apiUrl}/metrics/transfer");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<VpnServerMetrics>(json);
        }

        public static async Task UpdateDataLimitAsync(string keyId, long limitBytes)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(new { dataLimit = new { bytes = limitBytes } }),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync($"{_apiUrl}/access-keys/{keyId}/data-limit", content);
            response.EnsureSuccessStatusCode();
        }

        public static async Task RemoveDataLimitAsync(string keyId)
        {
            var response = await _httpClient.DeleteAsync($"{_apiUrl}/access-keys/{keyId}/data-limit");
            response.EnsureSuccessStatusCode();
        }

        public static async Task<VpnKey?> GetKeyByIdAsync(string keyId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiUrl}/access-keys/{keyId}");
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<VpnKey>(json);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                throw;
            }
        }

        public static async Task<long?> GetUsageByKeyIdAsync(string keyId)
        {
            try
            {
                var metrics = await GetMetricsAsync();

                return metrics?.bytesTransferredByUserId.ContainsKey(keyId) == true
                    ? metrics.bytesTransferredByUserId[keyId]
                    : null;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Error: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                throw;
            }
        }
    }
}
