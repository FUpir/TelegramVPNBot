using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using TelegramVPNBot.Helpers;
using TelegramVPNBot.Interfaces;
using TelegramVPNBot.Services;
using Telegram.Bot.Exceptions;

namespace TelegramVPNBot.Commands
{
    public class SubscriptionCommand(IAuthorizationService authorizationService) : ICommand
    {
        public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
        {
            if (update.CallbackQuery == null)
                return;

            var userData = update.CallbackQuery.From;
            var user = await authorizationService.GetAuthorizedUserAsync(userData);

            var startMessage = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "SubscriptionMessage");
            var startImg = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "AccessImg");
            var menuKeys = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "KeyboardSubscription").Split('|');

            var status = SubscriptionStatusHelper.GetSubscriptionStatusMessage(user.SubscriptionEndDateUtc, user.Settings.Language);
            var activeTutorialUrl = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "ActiveTutorial");

            var keyInfo = await OutlineVpnService.GetKeyByIdAsync(user.OutlineKey);
            var accessUrl = keyInfo.accessUrl;

            var usageKey = await OutlineVpnService.GetUsageByKeyIdAsync(keyInfo.id);
            var accessUrlSpoiler = $"`{accessUrl}`";

            var message = string.Format(
                startMessage,
                status,
                user.SubscriptionEndDateUtc,
                string.Format("{0:F2}", usageKey / (double)(1024 * 1024 * 1024)) + " GB",
                accessUrlSpoiler
            );

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    new InlineKeyboardButton(menuKeys[0]) { Url = activeTutorialUrl }
                },
                new[]
                {
                    new InlineKeyboardButton(menuKeys[1]) { CallbackData = "start" }
                }
            });

            try
            {
                var media = new InputMediaPhoto(new InputFileUrl(startImg))
                {
                    Caption = message,
                    ParseMode = ParseMode.Markdown
                };

                await botClient.EditMessageMedia(
                    chatId: userData.Id,
                    messageId: update.CallbackQuery.Message.MessageId,
                    media: media,
                    replyMarkup: inlineKeyboard
                );
            }
            catch (ApiRequestException ex)
            {
                await botClient.SendPhoto(
                    chatId: user.TelegramId,
                    photo: InputFile.FromUri(startImg),
                    caption: message,
                    replyMarkup: inlineKeyboard,
                    parseMode: ParseMode.MarkdownV2
                );
            }
        }
    }
}
