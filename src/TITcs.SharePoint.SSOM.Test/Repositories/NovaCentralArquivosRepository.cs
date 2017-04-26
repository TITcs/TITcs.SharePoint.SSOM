using Microsoft.SharePoint;
using TITcs.SharePoint.SSOM.Test.Models;

namespace TITcs.SharePoint.SSOM.Test.Repositories
{
    [SharePointList("Central de Arquivos")]
    public class NovaCentralArquivosRepository : SharePointRepository<PGCSCItem>
    {
        public NovaCentralArquivosRepository(SPWeb web) : base(web)
        {

        }
    }
}