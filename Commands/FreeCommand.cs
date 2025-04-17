using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramVPNBot.Helpers;
using TelegramVPNBot.Interfaces;
using TelegramVPNBot.Services;

namespace TelegramVPNBot.Commands
{
    public class FreeCommand(IAuthorizationService authorizationService) : ICommand
    {
        public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
        {
            if (update.CallbackQuery == null)
                return;

            var userData = update.CallbackQuery.From;
            var user = await authorizationService.GetAuthorizedUserAsync(userData);

            if (!user.IsFreeAvailable)
                return;

            var startMessage = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "FreeMessage");
            var startImg = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "AccessImg");
            var menuKeys = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "KeyboardFree").Split('|');

            await authorizationService.UpdateIsFreeAvailableAsync(user.Id, false);

            await authorizationService.UpdateSubscriptionEndDateAsync(
                user.Id,
                (user.SubscriptionEndDateUtc == null || user.SubscriptionEndDateUtc <= DateTime.UtcNow)
                    ? DateTime.UtcNow.AddDays(1)
                    : user.SubscriptionEndDateUtc.Value.AddDays(1)
            );

            if (user.OutlineKey == null)
            {
                var key = await OutlineVpnService.CreateKeyWithIncrementedPortAsync(user.Id.ToString());
                await authorizationService.UpdateOutlineKeyAsync(user.Id, key.id);
            }

            var inlineKeyboard = new InlineKeyboardMarkup([
                [
                    new InlineKeyboardButton(menuKeys[0]) { CallbackData = "subscription" }
                ],
                [
                    new InlineKeyboardButton(menuKeys[1]) { CallbackData = "start" }
                ]
            ]);

            try
            {
                var media = new InputMediaPhoto(new InputFileUrl(startImg))
                {
                    Caption = startMessage,
                    ParseMode = Telegram.Bot.Types.Enums.ParseMode.Html
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
                    caption: startMessage,
                    replyMarkup: inlineKeyboard,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                );
            }
        }
    }
}
