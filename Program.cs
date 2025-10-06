using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
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
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;

                    services.AddSingleton<IConfiguration>(configuration);
                    services.AddSingleton<MongoDbContext>();
                    services.AddSingleton<IUserRepository, UserRepository>();
                    services.AddSingleton<IAuthorizationService, AuthorizationService>();
                    services.AddSingleton<UpdateHandler>();
                    services.AddSingleton<SubscriptionCleanupHelper>();
                    services.AddSingleton<ServerConnectionMonitor>();

                    services.AddSingleton<ITelegramBotClient>(_ =>
                    {
                        var botToken = configuration.GetValue<string>("Telegram:Token")
                            ?? throw new Exception("Telegram token not found in configuration.");
                        return new TelegramBotClient(botToken);
                    });

                    services.AddHostedService<TelegramBotHostedService>();
                })
                .Build();

            await host.RunAsync();
        }
    }
}
