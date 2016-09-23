using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TITcs.SharePoint.SSOM.Test
{
    [SharePointList("Projetos")]
    public class ProjetosRepository : SharePointRepository<Item>
    {
        public ProjetosRepository(SPWeb rootWeb)
            : base(rootWeb)
        {
        }

        public Item Add(Item item)
        {
            var fields = new Fields<Item>();

            fields.Add(i => i.Title, item.Title);
            fields.Add(i => i.Created, item.Created);

            var id = Insert(fields);

            return GetById(id);
        }
    }
}
