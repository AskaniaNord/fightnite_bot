using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace fightnite_bot
{
    class CommandHandler
    {
        DiscordSocketClient _client;
        CommandService _service;

        public async Task InitializeAsync(DiscordSocketClient client)
        {
            _client = client;
            _service = new CommandService();
            await _service.AddModulesAsync(Assembly.GetEntryAssembly());
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage s)
        {
            if (!(s is SocketUserMessage msg)) return;
            var context = new SocketCommandContext(_client, msg);
            var argPos = 0;
            if (msg.HasStringPrefix(Config.Bot.cmdPrefix, ref argPos)
                || msg.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _service.ExecuteAsync(context, argPos);
                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    Console.WriteLine(result.ErrorReason);
                }

                if (result.Error == CommandError.UnknownCommand)
                {
                    if (msg.Author.Id != 447190702863155220)
                    {
                        if (msg.Channel.Id == 447530226272960524)
                        {
                            await msg.DeleteAsync();
                        }
                    }
                }
            }
            else
            {
                if (msg.Author.Id != 447190702863155220)
                {
                    if (msg.Channel.Id == 447530226272960524)
                    {
                        await msg.DeleteAsync();
                    }
                }
            }
        }
    }
}
