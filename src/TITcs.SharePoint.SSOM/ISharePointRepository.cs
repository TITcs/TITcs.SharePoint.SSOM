using System.Collections.Generic;

namespace TITcs.SharePoint.SSOM
{
    public interface ISharePointRepository<TEntity> where TEntity : class
    {
        uint RowLimit { get; set; }
        string Title { get; set; }
        TEntity GetById(int id);
        SharePointPagedData<TEntity> GetAll(string lastPosition, string camlQuery = null);
        ICollection<TEntity> GetAll(string camlQuery = null);
    }
}