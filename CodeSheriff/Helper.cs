using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace CodeSheriff.Helper
{
    public sealed class JsonHelper
    {
        private Data _jsondata = new Data();
        private readonly string _path = Path.Combine(AppContext.BaseDirectory,"data.json");

        public Data GetData()
        {
            if (!File.Exists(_path))
            {
                File.WriteAllText(_path, "");
                Console.WriteLine("Error: Json file not present. Json file was created.");
                Environment.Exit(1);
            }
            var text = File.ReadAllText(_path);
            _jsondata = JsonConvert.DeserializeObject<Data>(text);
            return _jsondata;
        }

        public Data SaveData(Data data)
        {
            var json = JsonConvert.SerializeObject(data, new JsonSerializerSettings { Formatting = Formatting.Indented });
            File.WriteAllText(_path, json);
            _jsondata = GetData();
            return _jsondata;
        }
    }

    public struct Data
    {
        [JsonProperty("ignoredUsers")]
        public List<IgnoredUser> IgnoredUsers { get; private set; }
        [JsonProperty("flaggedWords")]
        public List<FlaggedWord> FlaggedWords { get; private set; }
    }

    public struct IgnoredUser
    {
        [JsonProperty("guildId")]
        public ulong GuildId { get; set; }
        [JsonProperty("userId")]
        public ulong UserId { get; set; }
    }

    public struct FlaggedWord
    {
        [JsonProperty("guildId")]
        public ulong GuildId { get; set; }
        [JsonProperty("word")]
        public string Word { get; set; }
        [JsonProperty("reasons")]
        public string[] Reasons { get; set; }
    }
}
