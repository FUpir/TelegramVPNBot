using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramVPNBot.Helpers;
using TelegramVPNBot.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramVPNBot.Commands
{
    public class StartCommand(IAuthorizationService authorizationService) : ICommand
    {
        public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
        {
            User userChat;

            if (update.Message?.From != null)
                userChat = update.Message.From;
            else if (update.CallbackQuery != null)
                userChat = update.CallbackQuery.From;
            else
                return;

            var user = await authorizationService.GetAuthorizedUserAsync(userChat);

            var startMessage = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "StartMessage");
            var startImg = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "StartImg");
            var menuKeys = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "KeyboardStart").Split('|');

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    new InlineKeyboardButton(menuKeys[0]) { CallbackData = "access" }
                },
                new[]
                {
                    new InlineKeyboardButton(menuKeys[1]) { CallbackData = "profile" },
                    new InlineKeyboardButton(menuKeys[2]) { CallbackData = "settings" }
                },
                new[]
                {
                    new InlineKeyboardButton(menuKeys[3]) { CallbackData = "support" }
                }
            });

            await botClient.SendPhoto(userChat.Id, InputFile.FromUri(startImg), caption: startMessage,
                replyMarkup: inlineKeyboard, parseMode:ParseMode.Markdown);
        }
    }
}