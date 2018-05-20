using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fightnite_bot.Core
{
    class functions
    {
        internal static SocketRole GetRoleWithName(SocketGuildUser user, string name)
        {
            string targetRoleName = name;
            var result = from r in user.Guild.Roles
                         where r.Name == targetRoleName
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return null;
            if (roleID.Equals(null)) return null;
            return user.Guild.GetRole(roleID);
        }

        internal static bool UserIsRole(SocketGuildUser user, string name)
        {
            string targetRoleName = name;
            var result = from r in user.Guild.Roles
                         where r.Name == targetRoleName
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return false;
            var targetRole = user.Guild.GetRole(roleID);
            return user.Roles.Contains(targetRole);
        }

        internal static string GetUserTier(SocketGuildUser user)
        {
            string[] tiers = { "Bronze", "Silver", "Gold", "Platinum", "Diamond" };
            foreach (string tier in tiers)
            {
                string targetRoleName = tier;
                var result = from r in user.Guild.Roles
                             where r.Name == targetRoleName
                             select r.Id;
                ulong roleID = result.FirstOrDefault();
                if (roleID == 0) continue;
                var targetRole = user.Guild.GetRole(roleID);
                if (user.Roles.Contains(targetRole))
                {
                    return tier;
                }
            }
            return null;
        }

        internal static string GetUserPlatform(SocketGuildUser user)
        {
            string[] platforms = { "PS4", "PC", "Xbox" };
            foreach (string platform in platforms)
            {
                string targetRoleName = platform;
                var result = from r in user.Guild.Roles
                             where r.Name == targetRoleName
                             select r.Id;
                ulong roleID = result.FirstOrDefault();
                if (roleID == 0) continue;
                var targetRole = user.Guild.GetRole(roleID);
                if (user.Roles.Contains(targetRole))
                {
                    return platform;
                }
            }
            return null;
        }

        internal static SocketVoiceChannel GetChannelWithName(SocketGuildUser user, string name)
        {
            string targetRoleName = name;
            var result = from r in user.Guild.Channels
                         where r.Name == targetRoleName
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return null;
            return user.Guild.GetVoiceChannel(roleID);
        }

        internal static SocketVoiceChannel GetChannelThatContains(SocketGuildUser user, string name)
        {
            string targetRoleName = name;
            var result = from r in user.Guild.Channels
                         where r.Name.Contains(targetRoleName)
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return null;
            return user.Guild.GetVoiceChannel(roleID);
        }

        internal static SocketRole GetRoleThatContains(SocketGuildUser user, string name)
        {
            string targetRoleName = name;
            var result = from r in user.Roles
                         where r.Name.Contains(targetRoleName)
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return null;
            if (roleID.Equals(null)) return null;
            return user.Guild.GetRole(roleID);
        }

        internal static SocketRole GetRoleFromGuildThatContains(SocketGuildUser user, string name)
        {
            string targetRoleName = name;
            var result = from r in user.Guild.Roles
                         where r.Name.Contains(targetRoleName)
                         select r.Id;
            ulong roleID = result.FirstOrDefault();
            if (roleID == 0) return null;
            if (roleID.Equals(null)) return null;
            return user.Guild.GetRole(roleID);
        }

        internal static SocketGuildUser GetSocketGuildUser(SocketGuildUser user)
        {
            return user;
        }
    }
}
