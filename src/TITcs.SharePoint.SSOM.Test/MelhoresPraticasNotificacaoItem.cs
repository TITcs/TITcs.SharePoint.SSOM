using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TITcs.SharePoint.SSOM;

namespace TITcs.SharePoint.SSOM.Test
{
    public class MelhoresPraticasNotificacaoItem : SharePointItem
    {
        #region properties

        public const string CURTIDA = "Curtida";
        public const string COMENTARIO = "Comentário";

        [JsonProperty("criador")]
        [SharePointField("Criador")]
        public string Criador { get; set; }

        [JsonProperty("historicoCurtidas")]
        [SharePointField("HistoricoCurtidas")]
        public ICollection<Lookup> HistoricoCurtidas { get; set; }

        [JsonProperty("tipo")]
        [SharePointField("Tipo")]
        public string Tipo { get; set; }

        [JsonProperty("idPost")]
        [SharePointField("IdPost")]
        public int IdPost { get; set; }

        [JsonProperty("contador")]
        [SharePointField("Contador")]
        public int Contador { get; set; }

        [JsonProperty("visualizado")]
        [SharePointField("Visualizado")]
        public bool Visualizado { get; set; }

        #endregion
    }
}
