using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace fightnite_bot.Core
{
    class q
    {
        public static System.Threading.Tasks.Task<Discord.Rest.RestRole> CreateQueueAsync(SocketGuildUser user, string tier, string platform, int maxPlayers)
        {
            tier = tier.Substring(0, 1);
            return user.Guild.CreateRoleAsync($"queue_{maxPlayers}_{tier}_{platform}");
        }

        public static bool CheckQueue(SocketGuildUser user, string tier, string platform, int maxPlayers)
        {
            tier = tier.Substring(0, 1);
            // check if there is a queue for that platform and tier
            if (platform == "PC")
            {
                foreach(SocketRole r in user.Guild.Roles)
                {
                    if(r.Name.Contains("queue") && !r.Name.Contains("full") && r.Name.Contains(tier))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (platform == "PS4")
            {
                foreach(SocketRole r in user.Guild.Roles)
                {
                    if(r.Name.Contains("queue") && !r.Name.Contains("full") && !r.Name.Contains("Xbox") && r.Name.Contains(tier))
                    {
                        return true;
                    }
                }
                return false;
            }
            else if (platform == "Xbox")
            {
                foreach (SocketRole r in user.Guild.Roles)
                {
                    if (r.Name.Contains("queue") && !r.Name.Contains("full") && !r.Name.Contains("PS4") && r.Name.Contains(tier))
                    {
                        return true;
                    }
                }
                return false;
            }
            else return false;
        }

        public static async System.Threading.Tasks.Task JoinQueueAsync(SocketGuildUser user, Discord.IRole role, SocketRole srole, string platform, IMessageChannel _channel, string tier, int maxP = 0)
        {
            Random rnd = new Random();
            //give the user the role
            await user.AddRoleAsync(role);

            int maxPlayers = 3; // this is a removed feature
            string txt = role.Name;

            bool rolePC = txt.Contains("PC");
            bool rolePS4 = txt.Contains("PS4");
            bool roleXbox = txt.Contains("Xbox");

            // add the platform to rolename if needed
            switch (platform)
            {
                case "PC":
                    if ((rolePS4 || roleXbox) && !rolePC)
                    {
                        txt = txt + "_PC";
                    }
                    break;
                case "PS4":
                    if (rolePC && !roleXbox && !rolePS4)
                    {
                        txt = txt + "_PS4";
                    }
                    break;
                case "Xbox":
                    if(rolePC && !roleXbox && !rolePS4)
                    {
                        txt = txt + "_Xbox";
                    }
                    break;
                default:
                    break;
            }
            if(maxP != 0)
            {
                maxPlayers = maxP - 1; // this is for !c
            }
            int rndinname = rnd.Next(999);
            string name = $"{txt}_full{rndinname}";
            if (srole != null)
            {
                // if queue is full or !c is typed
                if (srole.Members.Count() >= maxPlayers)
                {
                    await role.ModifyAsync(x =>
                    {
                        x.Name = name;
                    });
                    string channelname = $"Scrim #{rndinname}";
                    Discord.Rest.RestVoiceChannel channel = await user.Guild.CreateVoiceChannelAsync(channelname);
                    OverwritePermissions a = new OverwritePermissions(connect: PermValue.Allow);
                    OverwritePermissions d = new OverwritePermissions(connect: PermValue.Deny);
                    await channel.AddPermissionOverwriteAsync(role, a);
                    await channel.AddPermissionOverwriteAsync(functions.GetRole(user, "@everyone"), d);
                    await channel.ModifyAsync(x =>
                    {
                        x.CategoryId = 447191118715682818;
                        x.UserLimit = maxPlayers+1;
                    });
                    string message = $"{user.Mention} The {tier} queue is now full! Created voice channel {channelname}. You are teamed with:";
                    foreach (SocketUser u in srole.Members)
                    {
                        message = message + $" {u.Mention}";
                        await u.SendMessageAsync($"Your queue on {user.Guild.Name} just finished. The voice channel {channelname} got created for you and your team!");
                    }
                    await _channel.SendMessageAsync(message);
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
                    await _channel.SendMessageAsync($"{user.Mention} Succesfully joined the {tier} queue.");
                }
            } else
            {
                await _channel.SendMessageAsync($"{user.Mention} Succesfully joined the {tier} queue.");
            }
        }
    }
}
