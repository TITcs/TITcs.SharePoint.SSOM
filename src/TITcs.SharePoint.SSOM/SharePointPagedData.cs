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
        private int _numbersOfPage;
        public string PagingInfo { get; set; }
        public int TotalItems { get; set; }
        public int CurrentPage { get; set; }
        public uint PageSize { get; set; }
        public int NextPageIndex { get; set; }
        public ICollection<TEntity> Data { get; set; }
        private SPListItemCollection OriginalData { get; set; }
        public string PreviousPageQuery { get; set; }
        public string NextPageQuery { get; set; }
        public int FirstPageIndex { get; set; }
        public string FirstPageQuery { get; set; }
        public int LastPageIndex { get; set; }
        public string LastPageQuery { get; set; }
        public string CurrentPageSubtitle { get; set; }
        public IDictionary<int, string> PagingInfos { get; set; }

        #endregion

        #region constructors

        public SharePointPagedData(SPListItemCollection originalData, ICollection<TEntity> data, string pagingInfo, uint pageSize)
        {
            OriginalData = originalData;
            Data = data;
            TotalItems = originalData.Count;

            if (pageSize > TotalItems)
                pageSize = (uint)TotalItems;

            PageSize = pageSize;
            _numbersOfPage = GetNumbersOfPage();


            // build paging data
            NextPageQuery = pagingInfo;
            NextPageIndex = GetNextPageIndex();
            PreviousPageQuery = GetPreviousPageQuery();
            CurrentPageSubtitle = GetCurrentPageSubtitle();

            PagingInfos = GetPagingInfos();

            FirstPageQuery = GetFirstPageQuery();
            LastPageQuery = GetLastPageQuery();
        }

        #endregion

        #region methods

        private string GetFirstPageQuery()
        {
            FirstPageIndex = 0;

            return string.Empty;
        }
        private string GetLastPageQuery()
        {
            var pagesCount = GetNumbersOfPage();

            if (pagesCount == 1)
                return string.Empty;

            int index = (int)((PageSize * pagesCount) - PageSize) - 1;

            if (index < 0)
                index = 0;

            return OriginalData.Count > 0 ? $"Paged=TRUE&p_ID={OriginalData[index].ID}" : string.Empty;
        }
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
            sb.AppendFormat("Página {0} de {1}", OriginalData.Count > 0 && Data.Count > 0 ? GetCurrentPage() : 0, _numbersOfPage);
            return sb.ToString();
        }
        private int GetNextPageIndex()
        {
            // search for id param and extract
            var nextPageIndex = 0;
            if (!string.IsNullOrEmpty(NextPageQuery))
            {
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
            }
            else
            {
                var lastItemIndex = Data.Count - 1;
                nextPageIndex = lastItemIndex >= 0 ? OriginalData.OfType<SPListItem>().ToList<SPListItem>().FindIndex(i => i.ID == Data.OfType<SharePointItem>().ToList<SharePointItem>()[lastItemIndex].Id) : 0;
            }
            return nextPageIndex;
        }
        private bool IsFirstPage()
        {
            var page = 1;
            for (int i = 1; i <= _numbersOfPage; i++)
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
            var page = 1;

            for (int i = 1; i <= _numbersOfPage; i++)
            {
                if (NextPageIndex <= ((i * PageSize) - 1))
                {
                    page = i;
                    break;
                }
            }

            return page == _numbersOfPage;
        }
        private int GetCurrentPage()
        {
            var page = 1;

            for (int i = 1; i <= _numbersOfPage; i++)
            {
                if (NextPageIndex < (i * PageSize))
                {
                    page = i;
                    break;
                }
            }

            return CurrentPage = page;
        }
        private int GetNumbersOfPage()
        {
            // safe checks for page count
            var pagesCount = TotalItems <= 0 ? 0 : (double)TotalItems / PageSize; // in case the search hasnt returned any results pagesCount equals 0
            return (int)(pagesCount % 1 == 0 ? pagesCount : pagesCount + 1); // if the pagesCount is not an integer, adds 1 to it
        }
        private IDictionary<int, string> GetPagingInfos()
        {
            var result = new Dictionary<int, string>();
            for (var i = 1; i <= _numbersOfPage; i++)
            {
                result.Add(i, GetPagingInfo(i));
            }
            return result;
        }
        private string GetPagingInfo(int index)
        {
            if (index == 1) return string.Empty;
            var skip = (index - 1) * PageSize;
            var lastPageItemIndex = (int)((skip > TotalItems ? TotalItems : skip) - 1); // transform to zero based arrays
            var lastPageItemID = OriginalData.OfType<SPListItem>().ToList().ElementAt(lastPageItemIndex).ID;
            return string.Format(@"Paged=TRUE&p_ID={0}", lastPageItemID);
        }

        #endregion
    }
}
