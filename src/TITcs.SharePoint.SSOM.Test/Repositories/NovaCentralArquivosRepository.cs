using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TITcs.SharePoint.SSOM.Test.Models;

namespace TITcs.SharePoint.SSOM.Test.Repositories
{
    [SharePointList("Nova Central de Arquivos")]
    public class NovaCentralArquivosRepository : SharePointRepository<PGCSCItem>
    {
        public NovaCentralArquivosRepository(SPWeb web) : base(web)
        {

        }
    }
}