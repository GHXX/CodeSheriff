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
        public Data GetData()
        {
            var text = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json"));
            _jsondata = JsonConvert.DeserializeObject<Data>(text);
            return _jsondata;
        }
        public Data SaveData(Data data)
        {
            var json = JsonConvert.SerializeObject(data);
            File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json"), json);
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
