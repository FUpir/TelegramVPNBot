using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramVPNBot.Interfaces;
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

                    var startMessage = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "ExpiredMessage");
                    var menuKeys = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "KeyboardExpired").Split('|');

                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[]
                        {
                            new InlineKeyboardButton($"{menuKeys[0]}")
                            {
                                CallbackData = "access"
                            }
                        },
                        new[]
                        {
                            new InlineKeyboardButton($"{menuKeys[1]}")
                            {
                                CallbackData = "profile"
                            }
                        }
                    });

                    await botClient.SendMessage(user.TelegramId, startMessage, replyMarkup: inlineKeyboard);
                }
            }
        }
    }
}
