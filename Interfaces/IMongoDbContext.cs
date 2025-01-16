using MongoDB.Driver;
using TelegramVPNBot.Models;

namespace TelegramVPNBot.Interfaces
{
    public interface IMongoDbContext
    {
        IMongoCollection<User> Users { get; }
    }
}