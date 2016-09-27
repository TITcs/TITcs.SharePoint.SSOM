using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TITcs.SharePoint.SSOM.Test
{
    public class MelhoresPraticasComentariosRepository : SharePointRepository<Item>
    {
        #region constructors

        public MelhoresPraticasComentariosRepository(Microsoft.SharePoint.SPWeb web) : base(web)
        {

        }

        #endregion
    }
}
