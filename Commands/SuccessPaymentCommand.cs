using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramVPNBot.Helpers;
using TelegramVPNBot.Interfaces;
using TelegramVPNBot.Services;

namespace TelegramVPNBot.Commands
{
    public class SuccessPaymentCommand(IAuthorizationService authorizationService) : ICommand
    {
        public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
        {
            if (update.Message?.From == null)
                return;

            var userData = update.Message.From;
            var user = await authorizationService.GetAuthorizedUserAsync(userData);

            var subMonths = int.Parse(update.Message.SuccessfulPayment.InvoicePayload);

            var messageTxt = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "SuccessMessage");
            var startImg = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "AccessImg");
            var menuKeys = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "KeyboardSuccess").Split('|');

            var message = string.Format(messageTxt, subMonths, user.SubscriptionEndDateUtc);

            await authorizationService.UpdateSubscriptionEndDateAsync(
                user.Id,
                (user.SubscriptionEndDateUtc == null || user.SubscriptionEndDateUtc <= DateTime.UtcNow)
                    ? DateTime.UtcNow.AddMonths(subMonths) 
                    : user.SubscriptionEndDateUtc.Value.AddMonths(subMonths) 
            );

            if (user.OutlineKey == null)
            {
                var key = await OutlineVpnService.CreateKeyWithIncrementedPortAsync(user.Id.ToString());
                await authorizationService.UpdateOutlineKeyAsync(user.Id, key.id);
            }

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    new InlineKeyboardButton(menuKeys[0]) { CallbackData = "subscription" }
                },
                new[]
                {
                    new InlineKeyboardButton(menuKeys[1]) { CallbackData = "start" }
                }
            });

            await botClient.SendPhoto(user.TelegramId, InputFile.FromUri(startImg), caption: message,
                replyMarkup: inlineKeyboard,parseMode:ParseMode.Markdown);
        }
    }
}
