using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramVPNBot.Helpers
{
    public class Worker : IHostedService
    {
        private readonly IHostApplicationLifetime _appLifetime;

        public Worker(IHostApplicationLifetime appLifetime)
        {
            _appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Сервис запущен.");
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            return Task.CompletedTask;
        }

        private void OnStarted()
        {
            Console.WriteLine("Сервис начал работу.");
        }

        private void OnStopping()
        {
            Console.WriteLine("Сервис завершает работу.");
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Сервис остановлен.");
            return Task.CompletedTask;
        }
    }
}
