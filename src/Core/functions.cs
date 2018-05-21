using Discord.WebSocket;
using System.Linq;

namespace fightnite_bot.Core
{
    internal class Functions
    {
        internal static SocketRole GetRoleWithName(SocketGuildUser user, string name)
        {
            var targetRoleName = name;
            var result = from r in user.Guild.Roles
                         where r.Name == targetRoleName
                         select r.Id;
            var roleId = result.FirstOrDefault();
            if (roleId == 0) return null;
            return roleId.Equals(null) ? null : user.Guild.GetRole(roleId);
        }

        internal static string GetUserTier(SocketGuildUser user)
        {
            string[] tiers = { "Bronze", "Silver", "Gold", "Platinum", "Diamond" };
            return (from tier in tiers
                let targetRoleName = tier
                let result = (from r in user.Guild.Roles where r.Name == targetRoleName select r.Id)
                let roleId = result.FirstOrDefault()
                where roleId != 0
                let targetRole = user.Guild.GetRole(roleId)
                where user.Roles.Contains(targetRole)
                select tier).FirstOrDefault();
        }

        internal static string GetUserPlatform(SocketGuildUser user)
        {
            string[] platforms = { "PS4", "PC", "Xbox" };
            return (from platform in platforms
                let targetRoleName = platform
                let result = (from r in user.Guild.Roles where r.Name == targetRoleName select r.Id)
                let roleId = result.FirstOrDefault()
                where roleId != 0
                let targetRole = user.Guild.GetRole(roleId)
                where user.Roles.Contains(targetRole)
                select platform).FirstOrDefault();
        }

        internal static SocketVoiceChannel GetChannelThatContains(SocketGuildUser user, string name)
        {
            var targetRoleName = name;
            var result = from r in user.Guild.Channels
                         where r.Name.Contains(targetRoleName)
                         select r.Id;
            var roleId = result.FirstOrDefault();
            return roleId == 0 ? null : user.Guild.GetVoiceChannel(roleId);
        }

        internal static SocketRole GetRoleThatContains(SocketGuildUser user, string name)
        {
            var targetRoleName = name;
            var result = from r in user.Roles
                         where r.Name.Contains(targetRoleName)
                         select r.Id;
            var roleId = result.FirstOrDefault();
            if (roleId == 0) return null;
            return roleId.Equals(null) ? null : user.Guild.GetRole(roleId);
        }

        internal static SocketRole GetRoleFromGuildThatContains(SocketGuildUser user, string name)
        {
            var targetRoleName = name;
            var result = from r in user.Guild.Roles
                         where r.Name.Contains(targetRoleName)
                         select r.Id;
            var roleId = result.FirstOrDefault();
            if (roleId == 0) return null;
            return roleId.Equals(null) ? null : user.Guild.GetRole(roleId);
        }

        internal static SocketGuildUser GetSocketGuildUser(SocketGuildUser user)
        {
            return user;
        }
    }
}
