using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramVPNBot.Commands;
using TelegramVPNBot.Interfaces;

namespace TelegramVPNBot.Handlers
{
    public class UpdateHandler(IAuthorizationService authorizationService)
    {
        private readonly Dictionary<string, ICommand> _commands = new()
        {
            { "/start", new StartCommand(authorizationService) }
        };

        private readonly Dictionary<string, ICommand> _callbackQueryList = new()
        {
            { "access", new AccessCommand(authorizationService) },
            { "start", new StartCommand(authorizationService) },
            { "profile", new ProfileCommand(authorizationService) },
            { "month", new PaymentCommand(authorizationService) },
            { "SuccessPayment", new SuccessPaymentCommand(authorizationService) },
        };

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message)
            {
                Console.WriteLine($"Processing message from chat: {update.Message.Chat.Id}");
                await HandleMessageAsync(botClient, update, cancellationToken);
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                Console.WriteLine($"Processing callback query from user: {update.CallbackQuery.From.Id}");
                await HandleCallbackQueryAsync(botClient, update, cancellationToken);
            }
            else if (update is { Type: UpdateType.PreCheckoutQuery, PreCheckoutQuery.Id: not null })
            {
                await botClient.AnswerPreCheckoutQuery(update.PreCheckoutQuery.Id, cancellationToken: cancellationToken);

            }
            else
            {
                Console.WriteLine($"Update type {update.Type} is not supported.");
            }
        }

        private async Task HandleMessageAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message?.SuccessfulPayment != null && _callbackQueryList.TryGetValue("SuccessPayment", out var successPayment))
            {
                await successPayment.ExecuteAsync(update, botClient);
            }
            if (update.Message?.Text != null && _commands.TryGetValue(update.Message.Text, out var command))
            {
                await command.ExecuteAsync(update, botClient);
            }
        }

        private async Task HandleCallbackQueryAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.CallbackQuery?.Data != null)
            {
                var command = _callbackQueryList
                    .FirstOrDefault(cmd => update.CallbackQuery.Data.StartsWith(cmd.Key)).Value;

                if (command != null)
                {
                    await command.ExecuteAsync(update, botClient);
                }
                else
                {
                    Console.WriteLine($"Callback data '{update.CallbackQuery.Data}' did not match any command.");
                }

                await botClient.AnswerCallbackQuery(
                    callbackQueryId: update.CallbackQuery.Id,
                    cancellationToken: cancellationToken);
            }
        }

    }
}
