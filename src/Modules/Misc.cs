using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using fightnite_bot.Core;

namespace fightnite_bot.Modules
{
    public class Misc : ModuleBase<SocketCommandContext>
    {
        /* 
        !q to queue
        !c to complete the queue at it's current status
        !status to see queue status
        !leave to leave queue 
        !cag to close all games
        */
        [Command("q")]
        public async Task QueueUp([Remainder]string queuetype = "")
        {
            //Error Handler
            if (Context.Channel.Name != "scrims")
            {
                ulong channelId = Convert.ToUInt64(Config.bot.channelId);
                SocketTextChannel ch = Context.Guild.GetTextChannel(channelId);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Please use {ch.Mention}");
                return;
            }
            if (!(queuetype == "a") && !(queuetype == "o"))
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Please use !q <a/o> (a: queues with all tiers | o: queues only with your tier).");
                return;
            }
            // Get Information if they have a tier or not
            bool isTierThere = false;
            if (queuetype == "a")
            {
                isTierThere = true;
            }
            bool isPlatformThere = false;
            foreach (SocketRole r in Context.Guild.GetUser(Context.User.Id).Roles)
            {
                if (r.Name.Contains("Bronze") || r.Name.Contains("Silver") || r.Name.Contains("Gold") || r.Name.Contains("Platinum") || r.Name.Contains("Diamond"))
                {
                    isTierThere = true;
                }
                if (r.Name.Contains("PC") || r.Name.Contains("PS4") || r.Name.Contains("Xbox"))
                {
                    isPlatformThere = true;
                }
                if (isPlatformThere && isTierThere) break;
            }
            int arg = 4;
            //Error Handler
            if (!isPlatformThere)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} You dont have a platform role.");
                return;
            }
            if (!isTierThere)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} You dont have a tier role.");
                return;
            }
            //Check if they are in queue/lobby or not
            if (functions.GetRoleContains((SocketGuildUser)Context.User, "queue") == null)
            {
                SocketRole sRole;
                //Get user tier
                string tier = "";
                if (queuetype == "a")
                {
                    tier = "AllTier";
                }
                else
                {
                    tier = functions.GetUserTier((SocketGuildUser)Context.User);
                }
                string platform = functions.GetUserPlatform((SocketGuildUser)Context.User);
                string[] rolenames = null;
                // Get the tier substring bc there is only 1 letter in the rolename
                string tierA = tier.Substring(0, 1);
                switch (platform)
                {
                    case "PC":
                        rolenames = new string[]{ $"queue_{arg}_{tierA}_PC", $"queue_{arg}_{ tierA }_PS4", $"queue_{arg}_{ tierA }_Xbox" ,
                                        $"queue_{arg}_{tierA}_PC_Xbox", $"queue_{arg}_{ tierA }_PC_PS4", $"queue_{arg}_{ tierA }_PS4_PC",
                                        $"queue_{arg}_{ tierA }_Xbox_PC" };
                        break;
                    case "PS4":
                        rolenames = new string[] { $"queue_{arg}_{tierA}_PC", $"queue_{arg}_{ tierA }_PS4", $"queue_{arg}_{ tierA }_PC_PS4", $"queue_{arg}_{ tierA }_PS4_PC" };
                        break;
                    case "Xbox":
                        rolenames = new string[] { $"queue_{arg}_{tierA}_PC", $"queue_{arg}_{ tierA }_Xbox", $"queue_{arg}_{ tierA }_PC_Xbox", $"queue_{arg}_{ tierA }_Xbox_PC" };
                        break;
                    default:
                        break;
                }
                // Check if there is already a queue for that type
                if (q.CheckQueue((SocketGuildUser)Context.User, tier, platform, arg))
                {
                    //You get here if already a queue exists
                    sRole = null;
                    // Get the role object
                    foreach (string rolename in rolenames)
                    {
                        if (functions.GetRole((SocketGuildUser)Context.User, rolename) != null)
                        {
                            sRole = functions.GetRole((SocketGuildUser)Context.User, rolename);
                            break;
                        }
                    }
                    IRole role = sRole;
                    //join the queue
                    await q.JoinQueueAsync((SocketGuildUser)Context.User, role, sRole, platform, Context.Channel, tier);
                }
                else
                {
                    //you get here if no queue exists
                    // Create a role and get role object from it
                    Discord.Rest.RestRole a = await q.CreateQueueAsync((SocketGuildUser)Context.User, tier, platform, arg);
                    sRole = null;
                    // idk why i do this
                    foreach (string rolename in rolenames)
                    {
                        if (functions.GetRole((SocketGuildUser)Context.User, rolename) != null)
                        {
                            sRole = functions.GetRole((SocketGuildUser)Context.User, rolename);
                            break;
                        }
                    }
                    Discord.IRole role = a;
                    // join the created queue
                    await q.JoinQueueAsync((SocketGuildUser)Context.User, role, sRole, platform, Context.Channel, tier);
                }
            }
            else
            {
                // respond to user that he is already in the queue
                SocketRole role = functions.GetRoleContains((SocketGuildUser)Context.User, "queue");
                if (role.Name.Contains("full"))
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} You are still in a non expired lobby! Use !leave to leave.");
                }
                else
                {
                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} You are already in a queue! Use !leave to leave.");
                }
            }
        }
        [Command("leave")]
        public async Task destroyQueue()
        {
            // Error Handler
            if (Context.Channel.Name != "scrims")
            {
                ulong channelId = Convert.ToUInt64(Config.bot.channelId);
                SocketTextChannel ch = Context.Guild.GetTextChannel(channelId);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Please use {ch.Mention}");
                return;
            }
            SocketRole role = functions.GetRoleContains((SocketGuildUser)Context.User, "queue");
            if (role == null || role.Equals(null))
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} You are not in a queue.");
                return;
            }

            if (role.Members.Count() == 1)
            {
                // if he is alone delete the role and channel
                string channellast = role.Name.Substring(role.Name.Length - 3, 3);
                SocketVoiceChannel channel = functions.GetChannelContains((SocketGuildUser)Context.User, channellast);
                await role.DeleteAsync();
                if (channel != null)
                {
                    await channel.DeleteAsync();
                }
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Succesfully left your existing queue.");
            }
            else
            {
                // removing the role
                SocketGuildUser user = functions.GetSocketGuildUser((SocketGuildUser)Context.User);
                await user.RemoveRoleAsync(role);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Succesfully left your existing queue.");
            }
        }
        [Command("status")]
        public async Task status()
        {
            //Error Handler
            if (Context.Channel.Name != "scrims")
            {
                ulong channelId = Convert.ToUInt64(Config.bot.channelId);
                SocketTextChannel ch = Context.Guild.GetTextChannel(channelId);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Please use {ch.Mention}");
                return;
            }
            SocketRole role = functions.GetRoleContains((SocketGuildUser)Context.User, "queue");
            if (role == null)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} You are currently not in a queue.");
                return;
            }
            if (role.Name.Contains("full"))
            {
                string message1 = $"{Context.User.Mention} Your lobby has already started! You are matched with:";
                foreach (SocketGuildUser u in role.Members)
                {
                    message1 = message1 + $" {u.Mention}";
                }
                await Context.Channel.SendMessageAsync(message1);
                return;
            }
            
            string message = $"{Context.User.Mention} Here is the queue status: \n```";
            foreach (SocketRole r in Context.Guild.Roles)
            {
                //rotate through each role to check if it is a queue
                if (r.Name.Contains("queue") && !r.Name.Contains("full"))
                {
                    string sResult = r.Name.Substring(8, 1);
                    string tier;
                    bool isPC = false;
                    bool isPS4 = false;
                    bool isXbox = false;
                    // Get Tier
                    switch (sResult)
                    {
                        default:
                            tier = "Error";
                            break;
                        case "A":
                            tier = "AllTier";
                            break;
                        case "B":
                            tier = "Bronze";
                            break;
                        case "S":
                            tier = "Silver";
                            break;
                        case "G":
                            tier = "Gold";
                            break;
                        case "P":
                            tier = "Platinum";
                            break;
                        case "D":
                            tier = "Diamond";
                            break;
                    }
                    //Get Platform(s)
                    if (r.Name.Contains("PC"))
                    {
                        isPC = true;
                    }
                    if (r.Name.Contains("Xbox"))
                    {
                        isXbox = true;
                    }
                    if (r.Name.Contains("PS4"))
                    {
                        isPS4 = true;
                    }
                    // Get response for platforms, dont know a way of doing it better
                    if (isPC && !isPS4 && !isXbox)
                    {
                        message = message + $"{tier} queue status for PC: {r.Members.Count()}/4. Members:";
                    }
                    if (!isPC && isPS4 && !isXbox)
                    {
                        message = message + $"{tier} queue status for PS4: {r.Members.Count()}/4. Members:";
                    }
                    if (!isPC && !isPS4 && isXbox)
                    {
                        message = message + $"{tier} queue status for Xbox: {r.Members.Count()}/4. Members:";
                    }
                    if (isPC && isPS4 && !isXbox)
                    {
                        message = message + $"{tier} queue status for PC and PS4: {r.Members.Count()}/4. Members:";
                    }
                    if (isPC && !isPS4 && isXbox)
                    {
                        message = message + $"{tier} queue status for PC and Xbox: {r.Members.Count()}/4. Members:";
                    }
                    foreach (SocketGuildUser u in r.Members)
                    {
                        message = message + $" {u.Username}";
                    }
                    message = message + "\n";
                }
            }
            message = message + "```";
            await Context.Channel.SendMessageAsync(message);
        }
        [Command("c")]
        public async Task completeQueue()
        {
            //Error handler
            if (Context.Channel.Name != "scrims")
            {
                ulong channelId = Convert.ToUInt64(Config.bot.channelId);
                SocketTextChannel ch = Context.Guild.GetTextChannel(channelId);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Please use {ch.Mention}");
                return;
            }
            string platform = functions.GetUserPlatform((SocketGuildUser)Context.User);
            SocketRole srole = functions.GetRoleContains((SocketGuildUser)Context.User, "queue");
            if (srole == null)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} You are currently not in a queue.");
                return;
            }
            Discord.IRole role = srole;
            string tier = functions.GetUserTier((SocketGuildUser)Context.User);
            await q.JoinQueueAsync((SocketGuildUser)Context.User, role, srole, platform, Context.Channel, tier, srole.Members.Count());
        }
        [Command("cag")]
        public async Task closeAllGames()
        {
            //Error handler
            if (Context.Channel.Name != "scrims")
            {
                ulong channelId = Convert.ToUInt64(Config.bot.channelId);
                SocketTextChannel ch = Context.Guild.GetTextChannel(channelId);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Please use {ch.Mention}");
                return;
            }
            //only i can use this command kinda buggy tho
            ulong creatorId = Convert.ToUInt64(Config.bot.creatorId);
            if (Context.User.Id == creatorId)
            {
                foreach (SocketRole role in Context.Guild.Roles)
                {
                    if (role.Name.Contains("queue_"))
                    {
                        await role.DeleteAsync();
                    }
                }
                foreach (SocketVoiceChannel channel in Context.Guild.VoiceChannels)
                {
                    if (channel.Name.Contains("Scrim #"))
                    {
                        if (channel.Name.Contains("e"))
                        {
                            string chNumber = channel.Name.Substring(channel.Name.Length - 4, 3);
                            SocketRole role = functions.GetRoleGuildContains((SocketGuildUser)Context.User, chNumber);
                            await channel.DeleteAsync();
                            if (!(role == null))
                            {
                                await role.DeleteAsync();
                            }
                        }
                        else
                        {
                            string chNumber = channel.Name.Substring(channel.Name.Length - 3, 3);
                            SocketRole role = functions.GetRoleGuildContains((SocketGuildUser)Context.User, chNumber);
                            await channel.DeleteAsync();
                            if (!(role == null))
                            {
                                await role.DeleteAsync();
                            }
                        }
                    }
                }

                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Successfully deleted all lobbys and queues.");
            }
            else
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} You are not allowed to do that.");
            }
        }
    }
}
