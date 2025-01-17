using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramVPNBot.Helpers;
using TelegramVPNBot.Interfaces;

namespace TelegramVPNBot.Commands
{
    public class AccessCommand(IAuthorizationService authorizationService): ICommand
    {
        public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
        {
            if (update.CallbackQuery == null)
                return;

            var userData = update.CallbackQuery.From;
            var user = await authorizationService.GetAuthorizedUserAsync(userData);

            var startMessage = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "AccessMessage");
            var startImg = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "AccessImg");
            var menuKeys = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "KeyboardAccess").Split('|');

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    new InlineKeyboardButton(menuKeys[3]) { CallbackData = "free" }
                },
                new[]
                {
                    new InlineKeyboardButton(menuKeys[0]) { CallbackData = "month1" }
                },
                new[]
                {
                    new InlineKeyboardButton(menuKeys[1]) { CallbackData = "month6" }
                },
                new[]
                {
                    new InlineKeyboardButton(menuKeys[2]) { CallbackData = "month12" }
                },
                new[]
                {
                    new InlineKeyboardButton(menuKeys[4]) { CallbackData = "start" }
                }
            });

            await botClient.SendPhoto(user.TelegramId, InputFile.FromUri(startImg), caption: startMessage,
                replyMarkup: inlineKeyboard);
        }
    }
}
