using MongoDB.Bson;
using TelegramVPNBot.Helpers;
using TelegramVPNBot.Interfaces;
using TelegramVPNBot.Models;
using User = Telegram.Bot.Types.User;

namespace TelegramVPNBot.Services
{
    public class AuthorizationService(IUserRepository userRepository) : IAuthorizationService
    {
        public async Task<List<Models.User>?> GetUsersAsync()
        {
            return await userRepository.GetUsersAsync();
        }

        public async Task<Models.User> GetAuthorizedUserAsync(User userChat)
        {
            var user = await userRepository.GetUserByTelegramIdAsync(userChat.Id);

            if (user == null)
            {
                user = new Models.User()
                {
                    TelegramId = userChat.Id,
                    Username = userChat.Username,
                    CreatedAtUtc = DateTime.UtcNow,
                    FullName = userChat.FirstName + (userChat.LastName != null ? " " + userChat.LastName : ""),
                    Settings = new Settings
                    {
                        Language = LanguageHelper.GetLanguage(userChat.LanguageCode)
                    }
                };

                await userRepository.CreateUserAsync(user);
            }

            return user;
        }

        public async Task UpdateSubscriptionEndDateAsync(ObjectId id, DateTime? newEndDate)
        {
              await userRepository.UpdateSubscriptionEndDateAsync(id, newEndDate);
        }

        public async Task UpdateOutlineKeyAsync(ObjectId id, string? newOutlineKey)
        {
            await userRepository.UpdateOutlineKeyAsync(id, newOutlineKey);
        }

        public async Task UpdateIsFreeAvailableAsync(ObjectId id, bool isFreeAvailable)
        {
            await userRepository.UpdateIsFreeAvailableAsync(id, isFreeAvailable);
        }

        public async Task AddConnectionHistoryAsync(ObjectId id, Connection connection)
        {
            await userRepository.AddConnectionHistoryAsync(id, connection);
        }

        public async Task<List<Connection>?> GetConnectionHistoryAsync(ObjectId userId)
        {
            var user = await userRepository.GetUserByIdAsync(userId);

            return user?.ConnectionHistory;
        }
    }
}