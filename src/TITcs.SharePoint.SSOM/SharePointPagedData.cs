using System.Collections.Generic;

namespace TITcs.SharePoint.SSOM
{
    public class SharePointPagedData<TEntity>
    {
        public SharePointPagedData(ICollection<TEntity> data, string lastPosition, int totalItens)
        {
            LastPosition = lastPosition;
            Data = data;
            TotalItems = totalItens;
        }
        public string LastPosition { get; set; }

        public int TotalItems { get; set; }

        public ICollection<TEntity> Data { get; set; }
    }
}
