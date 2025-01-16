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
                    var botToken = configuration.GetValue<string>("Telegram:Token");
                    return new TelegramBotClient(botToken);
                })
                .AddSingleton<SubscriptionCleanupHelper>()
                .BuildServiceProvider();

            var botClient = serviceProvider.GetRequiredService<ITelegramBotClient>();
            var updateHandler = serviceProvider.GetRequiredService<UpdateHandler>();
            var cleanupService = serviceProvider.GetRequiredService<SubscriptionCleanupHelper>();

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

            Console.ReadKey();
            cts.Cancel();

            await cleanupTask;

            Console.WriteLine("Application stopped.");
        }
    }
}
