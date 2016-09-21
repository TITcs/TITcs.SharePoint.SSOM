using System.Collections.Generic;

namespace TITcs.SharePoint.SSOM
{
    public class SharePointPagedData<TEntity>
    {
        #region properties and fields

        public string LastPosition { get; set; }
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public ICollection<TEntity> Data { get; set; }
        public string PreviousPageQuery { get; set; }
        public string NextPageQuery { get; set; }
        public string CurrentPageQuery { get; set; }

        #endregion

        public SharePointPagedData(ICollection<TEntity> data, string lastPosition, int totalItens)
        {
            LastPosition = lastPosition;
            Data = data;
            TotalItems = totalItens;
        }
        
    }
}
