using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramVPNBot.Commands;
using TelegramVPNBot.Interfaces;
using TelegramVPNBot.Services;

namespace TelegramVPNBot.Handlers
{
    public class UpdateHandler
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly OutlineVpnService _outlineVpnService;
        private readonly Dictionary<string, ICommand> _commands;
        private readonly Dictionary<string, ICommand> _callbackQueryList;

        public UpdateHandler(IAuthorizationService authorizationService, OutlineVpnService outlineVpnService)
        {
            _authorizationService = authorizationService;
            _outlineVpnService = outlineVpnService;

            _commands = new Dictionary<string, ICommand>
            {
                { "/start", new StartCommand(_authorizationService) },
                { "/user", new MonitorLogsCommand(_authorizationService) },
                { "/announcement", new AnnouncementCommand(_authorizationService) }
            };

            _callbackQueryList = new Dictionary<string, ICommand>
            {
                { "access", new AccessCommand(_authorizationService) },
                { "start", new StartCommand(_authorizationService) },
                { "profile", new ProfileCommand(_authorizationService) },
                { "month", new PaymentCommand(_authorizationService) },
                { "SuccessPayment", new SuccessPaymentCommand(_authorizationService, _outlineVpnService) },
                { "subscription", new SubscriptionCommand(_authorizationService, _outlineVpnService) },
                { "free", new FreeCommand(_authorizationService, _outlineVpnService) }
            };
        }

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
                return;
            }

            if (update.Message?.Text is not { } messageText)
                return;

            var commandEntry = _commands.FirstOrDefault(c => messageText.StartsWith(c.Key));

            if (commandEntry.Key != null)
            {
                await commandEntry.Value.ExecuteAsync(update, botClient);
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
