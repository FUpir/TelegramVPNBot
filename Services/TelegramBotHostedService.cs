using Microsoft.Extensions.Hosting;
using Telegram.Bot;
using TelegramVPNBot.Handlers;
using TelegramVPNBot.Helpers;

namespace TelegramVPNBot.Services
{
    public class TelegramBotHostedService(
        ITelegramBotClient botClient,
        UpdateHandler updateHandler,
        SubscriptionCleanupHelper cleanupHelper,
        ServerConnectionMonitor serverMonitor)
        : IHostedService
    {
        private CancellationTokenSource? _cts;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Bot is starting...");
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            botClient.StartReceiving(
                updateHandler: async (bot, update, token) =>
                {
                    await updateHandler.HandleUpdateAsync(bot, update, token);
                },
                errorHandler: (_, exception, token) =>
                {
                    Console.WriteLine($"Telegram error: {exception.Message}");
                    return Task.CompletedTask;
                },
                cancellationToken: _cts.Token
            );

            _ = cleanupHelper.StartAsync(_cts.Token);
            _ = serverMonitor.StartAsync(_cts.Token);

            Console.WriteLine("Bot is running.");
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Bot is stopping...");
            await _cts?.CancelAsync()!;

            await Task.Delay(1000, cancellationToken);
            Console.WriteLine("Bot stopped.");
        }
    }
}
