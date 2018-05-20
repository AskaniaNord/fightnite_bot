using Discord.WebSocket;
using fightnite_bot.Modules;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace fightnite_bot.Core
{
    internal static class MainLoop
    {
        private static Timer Loop;

        internal static Task StartLoop()
        {
            Loop = new Timer()
            {
                Interval = 900000,
                AutoReset = true,
                Enabled = true
            };
            Loop.Elapsed += OnLoopTicked;

            return Task.CompletedTask;
        }

        private static void OnLoopTicked(object sender, ElapsedEventArgs e)
        {
            // if interval is short it works, if interval is long it doesnt work?! idk why
            ulong guildId = Convert.ToUInt64(Config.bot.guildId);
            ulong creatorId = Convert.ToUInt64(Config.bot.creatorId);
            SocketGuildUser user = Global.Client.GetGuild(guildId).GetUser(creatorId);
            foreach(SocketVoiceChannel channel in user.Guild.VoiceChannels)
            {
                if (channel.Name.Contains("Scrim #"))
                {
                    if (channel.Name.Contains("e"))
                    {
                        if (channel.Users.Count == 0)
                        {
                            string chNumber = channel.Name.Substring(channel.Name.Length - 4, 3);
                            SocketRole role = functions.GetRoleGuildContains(user, chNumber);
                            channel.DeleteAsync();
                            role.DeleteAsync();
                        }
                    } else
                    {
                        if (channel.Users.Count == 0)
                        {
                            string txt = channel.Name;
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
    }
}
