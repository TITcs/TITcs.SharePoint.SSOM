using System.Linq;
using System.Collections.Generic;
using System.Text;
using Microsoft.SharePoint;
using System.Text.RegularExpressions;
using System;

namespace TITcs.SharePoint.SSOM
{
    public class SharePointPagedData<TEntity> where TEntity : SharePointItem
    {
        #region properties and fields

        private const string REGEX_PID = "p_ID=[0-9]*";
        private const string REGEX_BACK_PAGING = "PagedPrev=TRUE";
        public string PagingInfo { get; set; }
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public uint PageSize { get; set; }
        public int NextPageIndex { get; set; }
        public ICollection<TEntity> Data { get; set; }
        public SPListItemCollection OriginalData { get; set; }
        public string PreviousPageQuery { get; set; }
        public string NextPageQuery { get; set; }
        public string CurrentPageSubtitle { get; set; }

        #endregion

        #region constructors

        public SharePointPagedData(SPListItemCollection originalData, ICollection<TEntity> data, string pagingInfo, uint pageSize)
        {
            //PagingInfo = pagingInfo;
            OriginalData = originalData;
            Data = data;
            TotalItems = originalData.Count;
            PageSize = pageSize;
            

            // build paging data
            NextPageQuery = GetNextPageQuery();
            NextPageIndex = GetNextPageIndex();
            PreviousPageQuery = GetPreviousPageQuery();
            CurrentPageSubtitle = GetCurrentPageSubtitle();
        }

        #endregion

        #region methods

        private string GetNextPageQuery()
        {
            var sb = new StringBuilder();
            var data = (Data.OfType<SharePointItem>().ToList<SharePointItem>());
            if (IsLastPage())
            {
                sb.Append(string.Empty);
            }
            else
            {
                sb.AppendFormat("Paged=TRUE&p_ID={0}", data[Data.Count - 1].Id);
            }
            return sb.ToString();
        }
        private string GetPreviousPageQuery()
        {
            var sb = new StringBuilder();
            var data = (Data.OfType<SharePointItem>().ToList<SharePointItem>());
            if (IsFirstPage())
            {
                sb.Append(string.Empty);
            }
            else
            {
                sb.AppendFormat("Paged=TRUE&PagedPrev=TRUE&p_ID={0}", data[0].Id);
            }
            return sb.ToString();
        }
        private string GetCurrentPageSubtitle()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Página {0} de {1}", GetCurrentPage(), TotalItems / PageSize);
            return sb.ToString();
        }
        private int GetNextPageIndex()
        {
            // search for id param and extract
            var nextPageIndex = 0;
            var searchPID = Regex.Match(NextPageQuery, "p_ID=[0-9]*"); // SEARCH A WAY TO RECOGNIZE THIS IS LAST PAGE
            if (searchPID.Success)
            {
                var splitPID = searchPID.Value.Split(new[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (splitPID != null && splitPID.Length >= 2)
                {
                    // find index in origin list based on id
                    var pID = Convert.ToInt32(splitPID[1]);
                    nextPageIndex = OriginalData.OfType<SPListItem>().ToList<SPListItem>().FindIndex(i => i.ID == pID);
                    nextPageIndex = nextPageIndex > (TotalItems - 1) ? TotalItems - 1 : nextPageIndex;
                }
            }

            return nextPageIndex;
        }
        private bool IsFirstPage()
        {
            var numberOfPages = TotalItems / PageSize;
            var page = 1;

            for (int i = 1; i <= numberOfPages; i++)
            {
                if (NextPageIndex <= ((i * PageSize) - 1))
                {
                    page = i;
                    break;
                }
            }

            return page == 1;
        }
        private bool IsLastPage()
        {
            var numberOfPages = TotalItems / PageSize;
            var page = 1;

            for (int i = 1; i <= numberOfPages; i++)
            {
                if (NextPageIndex <= ((i * PageSize) - 1))
                {
                    page = i;
                    break;
                }
            }

            return page == numberOfPages;
        }
        private int GetCurrentPage()
        {
            var numberOfPages = TotalItems / PageSize;
            var page = 1;

            for (int i = 1; i <= numberOfPages; i++)
            {
                if (NextPageIndex  < (i * PageSize))
                {
                    page = i;
                    break;
                }
            }

            return page;
        }

        #endregion
    }
}
