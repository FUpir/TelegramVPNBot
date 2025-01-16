using Telegram.Bot.Types;
using Telegram.Bot;

namespace TelegramVPNBot.Interfaces
{
    interface ICommand
    {
        Task ExecuteAsync(Update update, ITelegramBotClient botClient);
    }
}
