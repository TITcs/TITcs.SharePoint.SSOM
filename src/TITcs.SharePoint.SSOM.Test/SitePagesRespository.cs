using System;
using System.IO;
using Microsoft.SharePoint;
using System.Linq;

namespace TITcs.SharePoint.SSOM.Test
{
    [SharePointList("Teste")]
    public class SitePagesRepository : SharePointRepository<Item>
    {
        public SitePagesRepository(SPWeb spWeb)
            : base(spWeb)
        {
        }

        public SitePagesRepository(ISharePointContext context)
            : base(context)
        {
        }

        public Item GetByTitle(string title)
        {
            var query = string.Format(@"<Where><Eq><FieldRef Name='Title' /><Value Type='Text'>{0}</Value></Eq></Where>", title);

            var result = GetAll(query).FirstOrDefault();

            return result;
        }

        public int Add(string title)
        {
            var fields = new Fields<Item>();

            fields.Add(i => i.Title, title);

            var id = Insert(fields);

            return id;
        }

        public void Update(int id, string title)
        {
            var fields = new Fields<Item>();

            fields.Add(i => i.Id, 21);
            fields.Add(i => i.Title, title);

            Update(fields);
        }
    }
}
