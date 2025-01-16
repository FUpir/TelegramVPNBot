using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramVPNBot.Helpers;
using TelegramVPNBot.Interfaces;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.Payments;

namespace TelegramVPNBot.Commands
{
    public class PaymentCommand(IAuthorizationService authorizationService) : ICommand
    {
        private const int MonthPrice = 5;
        private int _totalPrice = 0;
        public async Task ExecuteAsync(Update update, ITelegramBotClient botClient)
        {
            if (update.CallbackQuery?.Data == null)
                return;

            var userData = update.CallbackQuery.From;
            var user = await authorizationService.GetAuthorizedUserAsync(userData);

            var match = Regex.Match(update.CallbackQuery.Data, @"\d+");

            if (match.Success)
            {
                _totalPrice = int.Parse(match.Value) * MonthPrice;
            }
            else
                return;


            var startMessage = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "PaymentMessage");
            var startImg = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "AccessImg");
            var menuKeys = LanguageHelper.GetLocalizedMessage(user.Settings.Language, "KeyboardProfile").Split('|');

            var status = SubscriptionStatusHelper.GetSubscriptionStatusMessage(user.SubscriptionEndDateUtc, user.Settings.Language);

            var message = string.Format(startMessage, _totalPrice / MonthPrice, _totalPrice);

            List<LabeledPrice> labels = [new LabeledPrice("PRICE", _totalPrice)];

            var link = await botClient.CreateInvoiceLink
            (
                "Subscription",
                "PutiNet VPN",
                $"{_totalPrice/MonthPrice}",
                "XTR",
                labels);

            var inlineKeyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    new InlineKeyboardButton($"{_totalPrice}⭐")
                    {
                        Url = link
                    }
                },
                new[]
                {
                    new InlineKeyboardButton($"⬅️{menuKeys[0]}")
                    {
                        CallbackData = "access"
                    }
                }
            });

            await botClient.SendPhoto(user.TelegramId, InputFile.FromUri(startImg), caption: message,
                replyMarkup: inlineKeyboard, parseMode: ParseMode.Markdown);
        }
    }
}
