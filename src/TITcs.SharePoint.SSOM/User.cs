using System.Collections.Generic;
using Newtonsoft.Json;

namespace TITcs.SharePoint.SSOM
{
    public class User
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("login")]
        public string Login { get; set; }
        [JsonIgnore]
        public string Claims { get; set; }
        [JsonIgnore]
        public ICollection<Group> Groups { get; set; }
    }
}
