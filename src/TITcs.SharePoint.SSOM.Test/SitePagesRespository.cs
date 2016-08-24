using Microsoft.SharePoint;
using System.Linq;

namespace TITcs.SharePoint.SSOM.Test
{
    [SharePointList("Teste")]
    public class SitePagesRepository : SharePointRepository<Item>
    {
        public SitePagesRepository(SPWeb rootWeb)
            : base(rootWeb)
        {
        }

        public Item GetByTitle(string title)
        {
            var query = string.Format(@"<Where><Eq><FieldRef Name='Title' /><Value Type='Text'>{0}</Value></Eq></Where>", title);

            var result = GetAll(query).FirstOrDefault();

            return result;
        }
    }
}
