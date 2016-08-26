using Newtonsoft.Json;

namespace TITcs.SharePoint.SSOM
{
    public class Group
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
