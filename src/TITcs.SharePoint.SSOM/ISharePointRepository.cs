using System.Collections.Generic;
using Microsoft.SharePoint;

namespace TITcs.SharePoint.SSOM
{
    public interface ISharePointRepository<TEntity> where TEntity : SharePointItem
    {
        uint RowLimit { get; set; }
        string Title { get; set; }
        ISharePointContext Context { get; }
        TEntity GetById(int id);
        SharePointPagedData<TEntity> GetAll(string pagingInfo, uint pageSize, string camlQuery = null);
        SharePointPagedData<TEntity> GetAllRecursive(string pagingInfo, uint pageSize, string camlQuery = null);
        SharePointPagedData<TEntity> GetAllFromFolderRecursive(string folder, string pagingInfo, uint pageSize, string camlQuery = null);
        SharePointPagedData<TEntity> GetAllFromFolder(string folder, string pagingInfo, uint pageSize, string camlQuery = null);
        //SharePointPagedData<TEntity> GetAllFromFolder(string folder, int pageIndex, uint pageSize, string camlQuery = null);
        ICollection<TEntity> GetAll(string camlQuery = null);
        int Count(string camlQuery = null);
        ICollection<TEntity> GetAllRecursive(string camlQuery = null);
        SPFolder FindFolder(string url);
    }
}