using MongoDB.Bson;
using TelegramVPNBot.Models;
using User = Telegram.Bot.Types.User;

namespace TelegramVPNBot.Interfaces
{
    public interface IAuthorizationService
    {
        Task<Models.User> GetAuthorizedUserAsync(User user);
        Task<List<Models.User>?> GetUsersAsync();
        Task UpdateSubscriptionEndDateAsync(ObjectId id, DateTime? newEndDate);
        Task UpdateOutlineKeyAsync(ObjectId id, string? newOutlineKey);
        Task UpdateIsFreeAvailableAsync(ObjectId id, bool isFreeAvailable);
        Task AddConnectionHistoryAsync(ObjectId id, Connection connection);
        Task<List<Connection>?> GetConnectionHistoryAsync(ObjectId userId);
    }
}
