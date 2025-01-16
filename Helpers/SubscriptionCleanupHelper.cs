using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using TelegramVPNBot.DataBase;
using TelegramVPNBot.Interfaces;
using TelegramVPNBot.Repositories;
using TelegramVPNBot.Services;

namespace TelegramVPNBot.Helpers
{
    public class SubscriptionCleanupHelper(IUserRepository userRepository, ITelegramBotClient botClient)
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine("Starting subscription cleanup service...");
                    await CleanupExpiredSubscriptionsAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during cleanup: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
            }

            Console.WriteLine("Subscription cleanup service stopped.");
        }

        private async Task CleanupExpiredSubscriptionsAsync()
        {
            var now = DateTime.UtcNow;
            var expiredUsers = await userRepository.GetExpiredUsersAsync(now);

            foreach (var user in expiredUsers)
            {
                if (!string.IsNullOrEmpty(user.OutlineKey))
                {
                    await OutlineVpnService.DeleteKeyAsync(user.OutlineKey);
                    await userRepository.UpdateOutlineKeyAsync(user.Id, null);

                    await botClient.SendMessage(user.TelegramId, "Your subscription has expired");
                }
            }
        }
    }
}
