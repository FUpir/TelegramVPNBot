using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using TelegramVPNBot.Helpers;
using TelegramVPNBot.Interfaces;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Exceptions;

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

            try
            {
                if (update.CallbackQuery?.Message != null)
                {
                    var media = new InputMediaPhoto(new InputFileUrl(startImg))
                    {
                        Caption = startMessage,
                        ParseMode = ParseMode.Html
                    };

                    await botClient.EditMessageMedia(
                        chatId: update.CallbackQuery.Message.Chat.Id,
                        messageId: update.CallbackQuery.Message.MessageId,
                        media: media,
                        replyMarkup: inlineKeyboard
                    );
                }
                else
                {
                    await botClient.SendPhoto(
                        chatId: user.TelegramId,
                        photo: InputFile.FromUri(startImg),
                        caption: startMessage,
                        replyMarkup: inlineKeyboard,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                    );
                }
            }
            catch (ApiRequestException ex)
            {
                Console.WriteLine($"Telegram API Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error: {ex.Message}");
            }
        }
    }
}