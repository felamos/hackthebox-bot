using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using LiteDB;
using HackTheBox.Entities;
using HackTheBox.Helpers;
using HackTheBox.Services;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MollaAbdulkerim
{
    class Program
    {
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private Config _config;

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                ExclusiveBulkDelete = false
            });
            _config = Extensions.GetConfig();

            var services = ConfigureServices();
            services.GetRequiredService<LogService>();

            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);

            await _client.LoginAsync(TokenType.Bot, _config.Token);
            await _client.SetGameAsync(_config.GameStatus, type: ActivityType.Playing);
            await _client.StartAsync();
            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton(new CommandService(new CommandServiceConfig()
                {
                    DefaultRunMode = RunMode.Async
                }))
                .AddSingleton<CommandHandlingService>()
                // Logging
                // .AddLogging(x => x.AddConsole())
                .AddSingleton<LogService>()
                .AddSingleton<AdminService>()
                .AddSingleton<ConfigurationService>()
                // Extra
                .AddSingleton(_config)
                .AddSingleton(new LiteRepository("molla.db"))
                // Add additional services here...
                .BuildServiceProvider();
        }
    }
}
