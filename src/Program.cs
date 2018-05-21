using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;
using fightnite_bot.Core;

namespace fightnite_bot
{
    class Program
    {
        DiscordSocketClient _client;
        CommandHandler _handler;

        private static void Main()
        => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            if (string.IsNullOrEmpty(Config.Bot.token)) return;
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose
            });
            _client.Log += Log;
            _client.Ready += MainLoop.StartLoop;
            await _client.LoginAsync(TokenType.Bot, Config.Bot.token);
            await _client.StartAsync();
            Global.Client = _client;
            _handler = new CommandHandler();
            await _handler.InitializeAsync(_client);
            await Task.Delay(-1);
        }

       
#pragma warning disable 1998
        private static async Task Log(LogMessage msg) => Console.WriteLine(msg.Message);
#pragma warning restore 1998
    }
}
