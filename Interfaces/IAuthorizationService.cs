using MongoDB.Bson;
using Telegram.Bot.Types;

namespace TelegramVPNBot.Interfaces
{
    public interface IAuthorizationService
    {
        Task<Models.User> GetAuthorizedUserAsync(User user);

        Task UpdateSubscriptionEndDateAsync(ObjectId id, DateTime? newEndDate);
        Task UpdateOutlineKeyAsync(ObjectId id, string? newOutlineKey);
    }
}
