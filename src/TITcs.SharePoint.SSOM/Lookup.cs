
using Newtonsoft.Json;

namespace TITcs.SharePoint.SSOM
{
    public class Lookup
    {
        public Lookup(int id)
        {
            Id = id;
        }

        public Lookup(int id, string text)
        {
            Id = id;
            Text = text;
        }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}
