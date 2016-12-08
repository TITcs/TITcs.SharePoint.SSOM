using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TITcs.SharePoint.SSOM.Test.Models
{
    public class PGCSCItem : SharePointItem
    {
        #region fields and properties

        [JsonProperty("pgcscFilePath")]
        [SharePointField("PGCSCFilePath")]
        public string PGCSCFilePath { get; set; }

        [JsonProperty("publicosAlvo")]
        [SharePointField("PGCSCPublicoAlvoCentralArquivos")]
        public ICollection<Lookup> PublicosAlvo { get; set; }

        [JsonProperty("FileType")]
        [SharePointField("FileSystemObjectType")]
        public string FileType { get; set; }

        #endregion
    }
}
