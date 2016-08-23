using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TITcs.SharePoint.SOM;

namespace TITcs.SharePoint.SOM.Test
{
    public class Item : SharePointItem
    {
        [SharePointField("WikiField")]
        public string Content { get; set; }

    }
}
