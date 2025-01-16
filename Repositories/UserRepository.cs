using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using TelegramVPNBot.DataBase;
using TelegramVPNBot.Interfaces;
using TelegramVPNBot.Models;

namespace TelegramVPNBot.Repositories
{
    public class UserRepository(MongoDbContext context) : IUserRepository
    {
        private readonly IMongoCollection<User> _users = context.Users;

        public async Task<User> GetUserByIdAsync(ObjectId id)
        {
            return await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByTelegramIdAsync(long telegramId)
        {
            return await _users.Find(user => user.TelegramId == telegramId).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(User user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task UpdateUserAsync(ObjectId id, User user)
        {
            await _users.ReplaceOneAsync(u => u.Id == id, user);
        }

        public async Task DeleteUserAsync(ObjectId id)
        {
            await _users.DeleteOneAsync(user => user.Id == id);
        }

        public async Task UpdateSubscriptionEndDateAsync(ObjectId id, DateTime? newEndDate)
        {
            var update = Builders<User>.Update.Set(user => user.SubscriptionEndDateUtc, newEndDate);
            await _users.UpdateOneAsync(user => user.Id == id, update);
        }

        public async Task UpdateOutlineKeyAsync(ObjectId id, string? newOutlineKey)
        {
            var update = Builders<User>.Update.Set(user => user.OutlineKey, newOutlineKey);
            await _users.UpdateOneAsync(user => user.Id == id, update);
        }

        public async Task<List<User>> GetExpiredUsersAsync(DateTime currentDateUtc)
        {
            return await _users.Find(user =>
                user.SubscriptionEndDateUtc != null && user.SubscriptionEndDateUtc < currentDateUtc&& user.OutlineKey!=null).ToListAsync();
        }
    }
}