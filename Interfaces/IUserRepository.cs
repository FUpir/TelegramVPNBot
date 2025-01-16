using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using TelegramVPNBot.Models;

namespace TelegramVPNBot.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetUserByIdAsync(ObjectId id);
        Task<User?> GetUserByTelegramIdAsync(long telegramId);
        Task CreateUserAsync(User user);
        Task UpdateUserAsync(ObjectId id ,User user);
        Task DeleteUserAsync(ObjectId id);
        Task UpdateSubscriptionEndDateAsync(ObjectId id, DateTime? newEndDate);
        Task UpdateOutlineKeyAsync(ObjectId id, string? newOutlineKey);
        Task<List<User>> GetExpiredUsersAsync(DateTime currentDateUtc);
    }
}