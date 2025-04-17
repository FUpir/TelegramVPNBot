using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using TelegramVPNBot.DataBase;
using TelegramVPNBot.Handlers;
using TelegramVPNBot.Helpers;
using TelegramVPNBot.Interfaces;
using TelegramVPNBot.Repositories;
using TelegramVPNBot.Services;

namespace TelegramVPNBot
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<MongoDbContext>()
                .AddSingleton<IUserRepository, UserRepository>()
                .AddSingleton<IAuthorizationService, AuthorizationService>()
                .AddSingleton<UpdateHandler>()
                .AddSingleton<ITelegramBotClient>(_ =>
                {
                    var botToken = configuration.GetValue<string>("Telegram:Token") ?? throw new Exception();
                    return new TelegramBotClient(botToken);
                })
                .AddSingleton<SubscriptionCleanupHelper>()
                .AddSingleton<ServerConnectionMonitor>()
                .BuildServiceProvider();

            var botClient = serviceProvider.GetRequiredService<ITelegramBotClient>();
            var updateHandler = serviceProvider.GetRequiredService<UpdateHandler>();
            var cleanupService = serviceProvider.GetRequiredService<SubscriptionCleanupHelper>();
            var severMonitor = serviceProvider.GetRequiredService<ServerConnectionMonitor>();

            var cts = new CancellationTokenSource();

            botClient.StartReceiving(
                updateHandler: async (bot, update, token) =>
                {
                    await updateHandler.HandleUpdateAsync(bot, update, token);
                },
                errorHandler: (_, exception, token) =>
                {
                    Console.WriteLine($"Error: {exception.Message}");
                    return Task.CompletedTask;
                },
                cancellationToken: cts.Token
            );

            Console.WriteLine("Bot is running...");

            var cleanupTask = cleanupService.StartAsync(cts.Token);
            var monitorTask = severMonitor.StartAsync(cts.Token);

            var waitForShutdown = new TaskCompletionSource();
            AppDomain.CurrentDomain.ProcessExit += (_, _) => waitForShutdown.TrySetResult();
            Console.CancelKeyPress += (_, _) => waitForShutdown.TrySetResult();

            await waitForShutdown.Task;

            await cts.CancelAsync();
            await cleanupTask;
            await monitorTask;

            Console.WriteLine("Application stopped.");
        }
    }
}