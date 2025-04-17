using Telegram.Bot;
using Telegram.Bot.Types;
using TelegramVPNBot.Helpers;
using TelegramVPNBot.Interfaces;

namespace TelegramVPNBot.Commands
{
    public class MonitorLogsCommand(IAuthorizationService authorizationService) : ICommand
    {
        public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
        {
            if (update.Message?.From == null)
                return;

            var userChat = update.Message.From;
            var user = await authorizationService.GetAuthorizedUserAsync(userChat);

            if (!user.IsAdmin)
                return;

            var users = await authorizationService.GetUsersAsync();

            foreach (var userData in users)
            {
                var connections = userData.ConnectionHistory;
                if (connections == null)
                    continue;

                var imgPath = await GraphScot.CreateGraphAsync(connections);

                await using Stream stream = System.IO.File.OpenRead(imgPath);

                await botClient.SendPhoto(
                    chatId: update.Message.Chat.Id,
                    photo: InputFile.FromStream(stream),
                    caption: $"@{userData.Username} {userData.Id} {userData.FullName}"
                );
            }
        }
    }
}
