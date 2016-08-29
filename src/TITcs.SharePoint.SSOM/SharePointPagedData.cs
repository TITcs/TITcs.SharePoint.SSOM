using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TITcs.SharePoint.SSOM
{
    public class SharePointPagedData<TEntity>
    {
        public SharePointPagedData(string previousPage, string nextPage, ICollection<TEntity> data)
        {
            PreviousPage = previousPage;
            NextPage = nextPage;
            Data = data;
        }
        public string PreviousPage { get; set; }

        public string NextPage { get; set; }

        public ICollection<TEntity> Data { get; set; }
    }
}
