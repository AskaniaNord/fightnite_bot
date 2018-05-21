using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using fightnite_bot.Core;
// ReSharper disable UnusedMember.Global

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
                var channelId = Convert.ToUInt64(Config.Bot.channelId);
                var ch = Context.Guild.GetTextChannel(channelId);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Please use {ch.Mention}");
                return;
            }

            if (queuetype == "a" || queuetype == "o")
            {
                var isTierThere = queuetype == "a";

                var isPlatformThere = false;
                foreach (var r in Context.Guild.GetUser(Context.User.Id).Roles)
                {
                    if (r.Name.Contains("Bronze") || r.Name.Contains("Silver") || r.Name.Contains("Gold") ||
                        r.Name.Contains("Platinum") || r.Name.Contains("Diamond"))
                    {
                        isTierThere = true;
                    }

                    if (r.Name.Contains("PC") || r.Name.Contains("PS4") || r.Name.Contains("Xbox"))
                    {
                        isPlatformThere = true;
                    }

                    if (isPlatformThere && isTierThere) break;
                }

                const int arg = 4;
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
                if (Functions.GetRoleThatContains((SocketGuildUser) Context.User, "queue") == null)
                {
                    SocketRole sRole;
                    //Get user tier
                    var tier = queuetype == "a" ? "AllTier" : Functions.GetUserTier((SocketGuildUser) Context.User);

                    var platform = Functions.GetUserPlatform((SocketGuildUser) Context.User);
                    string[] rolenames = null;
                    // Get the tier substring bc there is only 1 letter in the rolename
                    var tierA = tier.Substring(0, 1);
                    switch (platform)
                    {
                        case "PC":
                            rolenames = new[]
                            {
                                $"queue_{arg}_{tierA}_PC", $"queue_{arg}_{tierA}_PS4", $"queue_{arg}_{tierA}_Xbox",
                                $"queue_{arg}_{tierA}_PC_Xbox", $"queue_{arg}_{tierA}_PC_PS4",
                                $"queue_{arg}_{tierA}_PS4_PC",
                                $"queue_{arg}_{tierA}_Xbox_PC"
                            };
                            break;
                        case "PS4":
                            rolenames = new[]
                            {
                                $"queue_{arg}_{tierA}_PC", $"queue_{arg}_{tierA}_PS4", $"queue_{arg}_{tierA}_PC_PS4",
                                $"queue_{arg}_{tierA}_PS4_PC"
                            };
                            break;
                        case "Xbox":
                            rolenames = new[]
                            {
                                $"queue_{arg}_{tierA}_PC", $"queue_{arg}_{tierA}_Xbox", $"queue_{arg}_{tierA}_PC_Xbox",
                                $"queue_{arg}_{tierA}_Xbox_PC"
                            };
                            break;
                    }

                    // Check if there is already a queue for that type
                    if (Q.CheckIfThereIsAnExistingQueue((SocketGuildUser) Context.User, tier, platform, arg))
                    {
                        //You get here if already a queue exists
                        // Get the role object
                        sRole = (from rolename in rolenames
                                where Functions.GetRoleWithName((SocketGuildUser) Context.User, rolename) != null
                                select Functions.GetRoleWithName((SocketGuildUser) Context.User, rolename))
                            .FirstOrDefault();

                        IRole role = sRole;
                        //join the queue
                        await Q.JoinQueueAsync((SocketGuildUser) Context.User, role, sRole, platform, Context.Channel,
                            tier);
                    }
                    else
                    {
                        //you get here if no queue exists
                        // Create a role and get role object from it
                        var a =
                            await Q.CreateQueueAsync((SocketGuildUser) Context.User, tier, platform, arg);
                        // idk why i do this
                        sRole = (from rolename in rolenames
                                where Functions.GetRoleWithName((SocketGuildUser) Context.User, rolename) != null
                                select Functions.GetRoleWithName((SocketGuildUser) Context.User, rolename))
                            .FirstOrDefault();

                        IRole role = a;
                        // join the created queue
                        await Q.JoinQueueAsync((SocketGuildUser) Context.User, role, sRole, platform, Context.Channel,
                            tier);
                    }
                }
                else
                {
                    // respond to user that he is already in the queue
                    var role = Functions.GetRoleThatContains((SocketGuildUser) Context.User, "queue");
                    if (role.Name.Contains("full"))
                        await Context.Channel.SendMessageAsync(
                            $"{Context.User.Mention} You are still in a non expired lobby! Use !leave to leave.");
                    else
                        await Context.Channel.SendMessageAsync(
                            $"{Context.User.Mention} You are already in a queue! Use !leave to leave.");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync(
                    $"{Context.User.Mention} Please use !q <a/o> (a: queues with all tiers | o: queues only with your tier).");
            }

            // Get Information if they have a tier or not
        }
        [Command("leave")]
        public async Task DestroyQueue()
        {
            // Error Handler
            if (Context.Channel.Name != "scrims")
            {
                var channelId = Convert.ToUInt64(Config.Bot.channelId);
                var ch = Context.Guild.GetTextChannel(channelId);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Please use {ch.Mention}");
                return;
            }
            var role = Functions.GetRoleThatContains((SocketGuildUser)Context.User, "queue");
            if (role == null || role.Equals(null))
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} You are not in a queue.");
                return;
            }

            if (role.Members.Count() == 1)
            {
                // if he is alone delete the role and channel
                var channellast = role.Name.Substring(role.Name.Length - 3, 3);
                var channel = Functions.GetChannelThatContains((SocketGuildUser)Context.User, channellast);
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
                var user = Functions.GetSocketGuildUser((SocketGuildUser)Context.User);
                await user.RemoveRoleAsync(role);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Succesfully left your existing queue.");
            }
        }
        [Command("status")]
        public async Task Status()
        {
            //Error Handler
            if (Context.Channel.Name != "scrims")
            {
                var channelId = Convert.ToUInt64(Config.Bot.channelId);
                var ch = Context.Guild.GetTextChannel(channelId);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Please use {ch.Mention}");
                return;
            }
            var role = Functions.GetRoleThatContains((SocketGuildUser)Context.User, "queue");
            if (role == null)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} You are currently not in a queue.");
                return;
            }
            if (role.Name.Contains("full"))
            {
                var message1 = $"{Context.User.Mention} Your lobby has already started! You are matched with:";
                message1 = role.Members.Aggregate(message1, (current, u) => current + $" {u.Mention}");
                await Context.Channel.SendMessageAsync(message1);
                return;
            }
            
            var message = $"{Context.User.Mention} Here is the queue status: \n```";
            foreach (var r in Context.Guild.Roles)
            {
                //rotate through each role to check if it is a queue
                if (!r.Name.Contains("queue") || r.Name.Contains("full")) continue;
                var sResult = r.Name.Substring(8, 1);
                string tier;
                var isPc = false;
                var isPs4 = false;
                var isXbox = false;
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
                    isPc = true;
                }
                if (r.Name.Contains("Xbox"))
                {
                    isXbox = true;
                }
                if (r.Name.Contains("PS4"))
                {
                    isPs4 = true;
                }
                // Get response for platforms, dont know a way of doing it better
                if (isPc && !isPs4 && !isXbox)
                {
                    message = message + $"{tier} queue status for PC: {r.Members.Count()}/4. Members:";
                }
                if (!isPc && isPs4 && !isXbox)
                {
                    message = message + $"{tier} queue status for PS4: {r.Members.Count()}/4. Members:";
                }
                if (!isPc && !isPs4 && isXbox)
                {
                    message = message + $"{tier} queue status for Xbox: {r.Members.Count()}/4. Members:";
                }
                if (isPc && isPs4 && !isXbox)
                {
                    message = message + $"{tier} queue status for PC and PS4: {r.Members.Count()}/4. Members:";
                }
                if (isPc && !isPs4 && isXbox)
                {
                    message = message + $"{tier} queue status for PC and Xbox: {r.Members.Count()}/4. Members:";
                }

                message = r.Members.Aggregate(message, (current, u) => current + $" {u.Username}");
                message = message + "\n";
            }
            message = message + "```";
            await Context.Channel.SendMessageAsync(message);
        }
        [Command("c")]
        public async Task CompleteQueue()
        {
            //Error handler
            if (Context.Channel.Name != "scrims")
            {
                var channelId = Convert.ToUInt64(Config.Bot.channelId);
                var ch = Context.Guild.GetTextChannel(channelId);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Please use {ch.Mention}");
                return;
            }
            var platform = Functions.GetUserPlatform((SocketGuildUser)Context.User);
            var srole = Functions.GetRoleThatContains((SocketGuildUser)Context.User, "queue");
            if (srole == null)
            {
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} You are currently not in a queue.");
                return;
            }
            IRole role = srole;
            var tier = Functions.GetUserTier((SocketGuildUser)Context.User);
            await Q.JoinQueueAsync((SocketGuildUser)Context.User, role, srole, platform, Context.Channel, tier, srole.Members.Count());
        }
        [Command("cag")]
        public async Task CloseAllGames()
        {
            //Error handler
            if (Context.Channel.Name != "scrims")
            {
                var channelId = Convert.ToUInt64(Config.Bot.channelId);
                var ch = Context.Guild.GetTextChannel(channelId);
                await Context.Channel.SendMessageAsync($"{Context.User.Mention} Please use {ch.Mention}");
                return;
            }
            //only i can use this command kinda buggy tho
            var creatorId = Convert.ToUInt64(Config.Bot.creatorId);
            if (Context.User.Id == creatorId)
            {
                foreach (var role in Context.Guild.Roles)
                {
                    if (role.Name.Contains("queue_"))
                    {
                        await role.DeleteAsync();
                    }
                }
                foreach (var channel in Context.Guild.VoiceChannels)
                {
                    if (!channel.Name.Contains("Scrim #")) continue;
                    if (channel.Name.Contains("e"))
                    {
                        var chNumber = channel.Name.Substring(channel.Name.Length - 4, 3);
                        var role = Functions.GetRoleFromGuildThatContains((SocketGuildUser)Context.User, chNumber);
                        await channel.DeleteAsync();
                        if (role != null)
                        {
                            await role.DeleteAsync();
                        }
                    }
                    else
                    {
                        var chNumber = channel.Name.Substring(channel.Name.Length - 3, 3);
                        var role = Functions.GetRoleFromGuildThatContains((SocketGuildUser)Context.User, chNumber);
                        await channel.DeleteAsync();
                        if (role != null)
                        {
                            await role.DeleteAsync();
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
