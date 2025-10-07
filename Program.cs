using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using TelegramVPNBot.DataBase;
using TelegramVPNBot.Handlers;
using TelegramVPNBot.Helpers;
using TelegramVPNBot.Interfaces;
using TelegramVPNBot.Models;
using TelegramVPNBot.Repositories;
using TelegramVPNBot.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(AppContext.BaseDirectory)
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<BotConfiguration>(context.Configuration.GetSection("BotConfiguration"));

        services.AddHttpClient("telegram_bot_client").RemoveAllLoggers()
                .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
                {
                    var botConfiguration = sp.GetService<IOptions<BotConfiguration>>()?.Value;
                    ArgumentNullException.ThrowIfNull(botConfiguration);
                    TelegramBotClientOptions options = new(botConfiguration.BotToken);
                    return new TelegramBotClient(options, httpClient);
                });

        services.AddMemoryCache();

        services.AddSingleton<MongoDbContext>();
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IAuthorizationService, AuthorizationService>();
        services.AddSingleton<UpdateHandler>();
        services.AddSingleton<SubscriptionCleanupHelper>();
        services.AddSingleton<ServerConnectionMonitor>();

        services.AddHostedService<TelegramBotHostedService>();
    })
    .Build();

await host.RunAsync();


public class TelegramBotHostedService(
    ITelegramBotClient botClient,
    UpdateHandler updateHandler,
    SubscriptionCleanupHelper cleanupService,
    ServerConnectionMonitor serverMonitor)
    : IHostedService
{
    private CancellationTokenSource? _cts;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Bot is starting...");

        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        botClient.StartReceiving(
            updateHandler: async (bot, update, token) =>
            {
                await updateHandler.HandleUpdateAsync(bot, update, token);
            },
            errorHandler: (_, exception, _) =>
            {
                Console.WriteLine($"Error: {exception.Message}");
                return Task.CompletedTask;
            },
            cancellationToken: _cts.Token
        );

        Console.WriteLine("Bot is running...");

        _ = Task.Run(() => cleanupService.StartAsync(_cts.Token), _cts.Token);
        _ = Task.Run(() => serverMonitor.StartAsync(_cts.Token), _cts.Token);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Stopping bot...");
        _cts?.Cancel();
        Console.WriteLine("Application stopped.");
        return Task.CompletedTask;
    }
}