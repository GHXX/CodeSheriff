using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CodeSheriff.Helper
{
    public sealed class JsonHelper
    {
        private Data _jsondata = new Data();
        private string fileName = Path.Combine("db","data.json");

        public Data GetData()
        {
            if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName)))
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName), "");
                (new Log()).WriteLogMessage("Error: Database file not present. DB file was created.", DSharpPlus.LogLevel.Critical);
                Environment.Exit(1);
            }
            var text = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
            _jsondata = JsonConvert.DeserializeObject<Data>(text);
            return _jsondata;
        }
        public Data SaveData(Data data)
        {
            var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings { Formatting = Formatting.Indented });
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName), json);
            _jsondata = GetData();
            return _jsondata;
        }
    }
    public sealed class Data
    {
        [JsonProperty("ignoredUsers")]
        public List<IgnoredUser> IgnoredUsers { get; private set; }
        [JsonProperty("flaggedWords")]
        public List<FlaggedWord> FlaggedWords { get; private set; }
    }
    public sealed class IgnoredUser
    {
        [JsonProperty("guildId")]
        public ulong GuildId { get; set; }
        [JsonProperty("userId")]
        public ulong UserId { get; set; }
    }
    public sealed class FlaggedWord
    {
        [JsonProperty("guildId")]
        public ulong GuildId { get; set; }
        [JsonProperty("word")]
        public string Word { get; set; }
        [JsonProperty("reasons")]
        public string[] Reasons { get; set; }
    }
}
