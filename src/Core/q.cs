using Discord;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Rest;

namespace fightnite_bot.Core
{
    internal class Q
    {
        public static Task<RestRole> CreateQueueAsync(SocketGuildUser user, string tier, string platform, int maxPlayers)
        {
            tier = tier.Substring(0, 1);
            return user.Guild.CreateRoleAsync($"queue_{maxPlayers}_{tier}_{platform}");
        }

        public static bool CheckIfThereIsAnExistingQueue(SocketGuildUser user, string tier, string platform, int maxPlayers)
        {
            tier = tier.Substring(0, 1);
            switch (platform)
            {
                // check if there is a queue for that platform and tier
                case "PC":
                    if (user.Guild.Roles.Any(r => r.Name.Contains("queue") && !r.Name.Contains("full") && r.Name.Contains(tier)))
                    {
                        return true;
                    }
                    return false;
                case "PS4":
                    if (user.Guild.Roles.Any(r => r.Name.Contains("queue") && !r.Name.Contains("full") && !r.Name.Contains("Xbox") && r.Name.Contains(tier)))
                    {
                        return true;
                    }
                    return false;
                case "Xbox":
                    if (user.Guild.Roles.Any(r => r.Name.Contains("queue") && !r.Name.Contains("full") && !r.Name.Contains("PS4") && r.Name.Contains(tier)))
                    {
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }

        public static async Task JoinQueueAsync(SocketGuildUser user, IRole role, SocketRole srole, string platform, IMessageChannel channel, string tier, int maxP = 0)
        {
            var rnd = new Random();
            //give the user the role
            await user.AddRoleAsync(role);

            var maxPlayers = 3; // this is a removed feature
            var txt = role.Name;

            txt = AddThePlatformToRolenameIfNeeded(txt, platform);
            
            if (maxP != 0)
            {
                maxPlayers = maxP - 1; // this is for !c
            }
            var rndinname = rnd.Next(999);
            var name = $"{txt}_full{rndinname}";
            if (srole != null)
            {
                // if queue is full or !c is typed
                if (srole.Members.Count() >= maxPlayers)
                {
                    await role.ModifyAsync(x =>
                    {
                        x.Name = name;
                    });
                    var channelname = $"Scrim #{rndinname}";
                    // ReSharper disable once InconsistentNaming
                    var _channel = await user.Guild.CreateVoiceChannelAsync(channelname);
                    await EditVoiceChannelPermissionsAsync(user, _channel, role);
                    //move channel to category & give it a userlimit
                    var categoryId = Convert.ToUInt64(Config.Bot.categoryId);
                    await _channel.ModifyAsync(x =>
                    {
                        x.CategoryId = categoryId;
                        x.UserLimit = maxPlayers+1;
                    });
                    var message = $"{user.Mention} The {tier} queue is now full! Created voice channel {channelname}. You are teamed with:";
                    foreach (var u in srole.Members)
                    {
                        message = message + $" {u.Mention}";
                        await u.SendMessageAsync($"Your queue on {user.Guild.Name} just finished. The voice channel {channelname} got created for you and your team!");
                    }
                    await channel.SendMessageAsync(message);
                }
                else
                {
                    if (role.Name != txt)
                    {
                        await role.ModifyAsync(x =>
                        {
                            x.Name = txt;
                        });
                    }
                    await channel.SendMessageAsync($"{user.Mention} Succesfully joined the {tier} queue.");
                }
            } else
            {
                await channel.SendMessageAsync($"{user.Mention} Succesfully joined the {tier} queue.");
            }
        }

        private static string AddThePlatformToRolenameIfNeeded(string txt, string platform)
        {
            var rolePc = txt.Contains("PC");
            var rolePs4 = txt.Contains("PS4");
            var roleXbox = txt.Contains("Xbox");

            switch (platform)
            {
                case "PC":
                    if ((rolePs4 || roleXbox) && !rolePc)
                    {
                        txt = txt + "_PC";
                    }
                    break;
                case "PS4":
                    if (rolePc && !roleXbox && !rolePs4)
                    {
                        txt = txt + "_PS4";
                    }
                    break;
                case "Xbox":
                    if (rolePc && !roleXbox && !rolePs4)
                    {
                        txt = txt + "_Xbox";
                    }
                    break;
            }
            return txt;
        }
        private static async Task EditVoiceChannelPermissionsAsync(SocketGuildUser user, RestGuildChannel channel, IRole role)
        {
            //only given role can join the given voicechannel
            var a = new OverwritePermissions(connect: PermValue.Allow);
            var d = new OverwritePermissions(connect: PermValue.Deny);
            await channel.AddPermissionOverwriteAsync(role, a);
            await channel.AddPermissionOverwriteAsync(Functions.GetRoleWithName(user, "@everyone"), d);
        }
    }
}
