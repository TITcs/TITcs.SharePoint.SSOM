using Newtonsoft.Json;
using System;

namespace TITcs.SharePoint.SSOM
{
    public abstract class SharePointItem
    {
        [JsonProperty("id")]
        [SharePointField("ID")]
        public int Id { get; set; }

        [JsonProperty("title")]
        [SharePointField("Title")]
        public virtual string Title { get; set; }

        [JsonProperty("created")]
        [SharePointField("Created")]
        public virtual DateTime Created { get; set; }

        [JsonProperty("author")]
        //public int _Level { get; set; }
        [SharePointField("Author")]
        public virtual Lookup Author { get; set; }

        [JsonProperty("file")]
        [SharePointField("File")]
        public virtual File File { get; set; }

        [JsonProperty("fileRef")]
        [SharePointField("FileRef")]
        public virtual string FileRef { get; set; }

        [JsonProperty("modified")]
        [SharePointField("Modified")]
        public virtual DateTime Modified { get; set; }
    }
}
