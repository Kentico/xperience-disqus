using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;

namespace Kentico.Xperience.Disqus.Models
{
    [JsonConverter(typeof(JsonPathConverter))]
    public class DisqusUserActivity
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("object.thread")]
        public DisqusThread Thread { get; set; }

        [JsonProperty("object.message")]
        public string Message { get; set; }

        [JsonProperty("object.author")]
        public DisqusUser Author { get; set; }

        [JsonProperty("object.parent")]
        public DisqusPost Parent { get; set; }

        [JsonProperty("object.url")]
        public string Url { get; set; }
    }
}
