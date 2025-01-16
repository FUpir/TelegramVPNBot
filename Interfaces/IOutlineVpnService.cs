using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramVPNBot.Models;

namespace TelegramVPNBot.Interfaces
{
    public interface IOutlineVpnService
    {
        Task<List<VpnKey>?> GetKeysAsync();
        Task<VpnKey?> CreateKeyAsync();
        Task DeleteKeyAsync(string keyId);
        Task UpdateKeyNameAsync(string keyId, string newName);
        Task<VpnServerMetrics?> GetMetricsAsync();
        Task UpdateDataLimitAsync(string keyId, long limitBytes);
        Task RemoveDataLimitAsync(string keyId);
    }
}
