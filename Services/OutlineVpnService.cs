using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TelegramVPNBot.Models;

namespace TelegramVPNBot.Services;

public static class OutlineVpnService
{
    private static readonly HttpClient HttpClient;
    private static readonly string ApiUrl;

    static OutlineVpnService()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        HttpClient = new HttpClient(handler);
        ApiUrl = configuration.GetValue<string>("OutLine:Url")!.TrimEnd('/');
    }

    public static async Task<List<VpnKey>?> GetKeysAsync()
    {
        try
        {
            var response = await HttpClient.GetAsync($"{ApiUrl}/access-keys");
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

    public static async Task<VpnKey?> CreateKeyWithIncrementedPortAsync(string name)
    {
        var keys = await GetKeysAsync();

        if (keys is null || keys.Count == 0)
        {   
            const int defaultPort = 50000;
            return await CreateKeyAsync(name, defaultPort);
        }

        var lastKey = keys.LastOrDefault();
        if (lastKey is null)
        {
            return await CreateKeyAsync(name, 50000);
        }

        var newPort = lastKey.port + 1;
        Console.WriteLine($"lastkey: {lastKey.id} (port {lastKey.port}). new port: {newPort}");

        return await CreateKeyAsync(name, newPort);
    }


    public static async Task<VpnKey?> CreateKeyAsync(string name, int port)
    {
        var requestBody = new { name, port };
        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await HttpClient.PostAsync($"{ApiUrl}/access-keys", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var newKey = JsonSerializer.Deserialize<VpnKey>(json);

        return newKey;
    }

    public static async Task DeleteKeyAsync(string keyId)
    {
        var response = await HttpClient.DeleteAsync($"{ApiUrl}/access-keys/{keyId}");
        response.EnsureSuccessStatusCode();
    }

    public static async Task UpdateKeyNameAsync(string keyId, string newName)
    {
        var content = new StringContent(
            JsonSerializer.Serialize(new { name = newName }),
            Encoding.UTF8,
            "application/json");

        var response = await HttpClient.PutAsync($"{ApiUrl}/access-keys/{keyId}/name", content);
        response.EnsureSuccessStatusCode();
    }

    public static async Task<VpnServerMetrics?> GetMetricsAsync()
    {
        var response = await HttpClient.GetAsync($"{ApiUrl}/metrics/transfer");
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

        var response = await HttpClient.PutAsync($"{ApiUrl}/access-keys/{keyId}/data-limit", content);
        response.EnsureSuccessStatusCode();
    }

    public static async Task RemoveDataLimitAsync(string keyId)
    {
        var response = await HttpClient.DeleteAsync($"{ApiUrl}/access-keys/{keyId}/data-limit");
        response.EnsureSuccessStatusCode();
    }

    public static async Task<VpnKey?> GetKeyByIdAsync(string keyId)
    {
        try
        {
            var response = await HttpClient.GetAsync($"{ApiUrl}/access-keys/{keyId}");
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