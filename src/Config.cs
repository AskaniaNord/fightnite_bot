using System.IO;
using Newtonsoft.Json;

namespace fightnite_bot
{
    internal class Config
    {
        private const string ConfigFolder = "Resources";
        private const string ConfigFile = "config.json";

        public static BotConfig Bot;

        static Config()
        {
            if (!Directory.Exists(ConfigFolder)) Directory.CreateDirectory(ConfigFolder);
            if (!File.Exists(ConfigFolder + "/" + ConfigFile))
            {
                Bot = new BotConfig();
                var json = JsonConvert.SerializeObject(Bot, Formatting.Indented);
                File.WriteAllText(ConfigFolder + "/" + ConfigFile, json);
            }
            else
            {
                string json = File.ReadAllText(ConfigFolder + "/" + ConfigFile);
                Bot = JsonConvert.DeserializeObject<BotConfig>(json);                
            }
        }

        public struct BotConfig
        {
            public string token;
            public string cmdPrefix;
            public string creatorId;
            public string channelId;
            public string guildId;
            public string categoryId;
        }
    }
}
