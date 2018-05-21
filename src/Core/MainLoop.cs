using System;
using System.Threading.Tasks;
using System.Timers;

namespace fightnite_bot.Core
{
    internal static class MainLoop
    {
        private static Timer _loop;

        internal static Task StartLoop()
        {
            _loop = new Timer()
            {
                Interval = 900000,
                AutoReset = true,
                Enabled = true
            };
            _loop.Elapsed += OnLoopTicked;

            return Task.CompletedTask;
        }

        private static void OnLoopTicked(object sender, ElapsedEventArgs e)
        {
            // if interval is short it works, if interval is long it doesnt work?! idk why
            var guildId = Convert.ToUInt64(Config.Bot.guildId);
            var creatorId = Convert.ToUInt64(Config.Bot.creatorId);
            var user = Global.Client.GetGuild(guildId).GetUser(creatorId);
            foreach(var channel in user.Guild.VoiceChannels)
            {
                if (!channel.Name.Contains("Scrim #")) continue;
                if (channel.Name.Contains("e"))
                {
                    if (channel.Users.Count != 0) continue;
                    var chNumber = channel.Name.Substring(channel.Name.Length - 4, 3);
                    var role = Functions.GetRoleFromGuildThatContains(user, chNumber);
                    channel.DeleteAsync();
                    role.DeleteAsync();
                } else
                {
                    if (channel.Users.Count != 0) continue;
                    var txt = channel.Name;
                    txt = txt + "e";
                    channel.ModifyAsync(x =>
                    {
                        x.Name = txt;
                    });
                }
            }
        }
    }
}
