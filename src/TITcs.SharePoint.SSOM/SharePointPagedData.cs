using System.Collections.Generic;

namespace TITcs.SharePoint.SSOM
{
    public class SharePointPagedData<TEntity>
    {
        public SharePointPagedData(ICollection<TEntity> data, string lastPosition)
        {
            LastPosition = lastPosition;
            Data = data;  
        }
        public string LastPosition { get; set; }

        public ICollection<TEntity> Data { get; set; }
    }
}
