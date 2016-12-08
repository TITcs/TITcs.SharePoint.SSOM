using Microsoft.SharePoint;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using TITcs.SharePoint.SSOM.Utils;

namespace TITcs.SharePoint.SSOM
{
    public abstract class SharePointRepository<TEntity> : ISharePointRepository<TEntity> where TEntity : SharePointItem
    {
        #region properties and fields

        /// <summary>
        /// Private reference for the context object
        /// </summary>
        private readonly ISharePointContext _context;

        /// <summary>
        /// Public reference for the context object
        /// </summary>
        public ISharePointContext Context => _context;

        /// <summary>
        /// Public reference for the Title property
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Public reference for the RowLimit property
        /// </summary>
        public uint RowLimit { get; set; }

        #endregion

        #region constructors

        /// <summary>
        /// Default constructor for the repository. Initializes the repository based on the current SPContext object
        /// </summary>
        protected SharePointRepository()
            : this(SPContext.Current.Web)
        {
        }

        /// <summary>
        /// Initializes the repository based on a ISharepointContext object
        /// </summary>
        /// <param name="context">ISharePointContext object initialize the context</param>
        protected SharePointRepository(ISharePointContext context)
        {
            _context = context;

            Title = GetListTitle();
            RowLimit = 0;

            Logger.Logger.Debug("SharePointRepository.Constructor", "Title = {0}", Title);
        }

        /// <summary>
        /// Initializes the repository based on an SPWeb object.
        /// </summary>
        /// <param name="web">SPWeb object initialize the context</param>
        protected SharePointRepository(SPWeb web)
        {
            _context = new SharePointContext(web);

            Title = GetListTitle();
            RowLimit = 0;

            Logger.Logger.Debug("SharePointRepository.Constructor", "Title = {0}", Title);
        }

        #endregion

        #region methods

        /// <summary>
        /// Gets an item by its Id
        /// </summary>
        /// <param name="id">Id of the item to return</param>
        /// <returns>Returns a domain object representing the SPListItem found</returns>
        public TEntity GetById(int id)
        {
            // log execution
            Logger.Logger.Debug("SharePointRepository.GetById", "ID = {0}", id);

            TEntity result = Call(() =>
            {
                using (_context.Web)
                {
                    // find target list
                    var list = _context.Web.Lists.TryGetList(Title);
                    if (list == null)
                        throw new Exception($"The list \"{Title}\" not found");

                    SPQuery query = new SPQuery
                    {
                        Query = $"<Where><Eq><FieldRef Name='ID' /><Value Type='Counter'>{id}</Value></Eq></Where>"
                    };

                    SPListItemCollection items = list.GetItems(query);

                    if (items.Count == 0)
                        return null;
                    else
                    {
                        var entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
                        var listItem = items.Cast<SPListItem>().Single();
                        SetProperties(entity, listItem);

                        return entity;
                    }
                }
            });

            return result;
        }

        /// <summary>
        /// Gets an item by its Id
        /// </summary>
        /// <param name="id">Id of the item to return</param>
        /// <returns>Returns the original SPListItem object found</returns>
        public SPListItem GetSPListItem(int id)
        {
            Logger.Logger.Debug("SharePointRepository.GetSPListItem", "ID = {0}", id);

            SPListItem result = Call(() =>
            {
                using (_context.Web)
                {
                    var list = _context.Web.Lists.TryGetList(Title);

                    if (list == null)
                        throw new Exception($"The list \"{Title}\" not found");

                    SPQuery query = new SPQuery
                    {
                        Query = $"<Where><Eq><FieldRef Name='ID' /><Value Type='Counter'>{id}</Value></Eq></Where>"
                    };

                    SPListItemCollection items = list.GetItems(query);

                    return items.Cast<SPListItem>().SingleOrDefault();
                }
            });

            return result;
        }

        /// <summary>
        /// Finds a folder based on the informed url pattern
        /// </summary>
        /// <param name="url">Url pattern of the folder</param>
        /// <returns>Returns an SPFolder object</returns>
        public SPFolder FindFolder(string url)
        {
            Logger.Logger.Debug("SharePointRepository.FindFolder", "Url = {0}", url);

            SPFolder folder = default(SPFolder);

            try
            {
                if (string.IsNullOrWhiteSpace(url)) { throw new ArgumentNullException("url"); }

                var parts = url.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts != null && parts.Length > 0)
                {
                    var result = Call(() =>
                    {
                        using (_context.Web)
                        {
                            _context.Web.CacheAllSchema = false;

                            // access source list
                            var list = GetSourceList();

                            if (list != null)
                            {
                                for (var i = 0; i < parts.Length; i++)
                                {
                                    var decodedUrl = HttpUtility.UrlDecode(parts[i]);
                                    if (i == 0 || folder == null)
                                    {
                                        folder = list.RootFolder.SubFolders[decodedUrl];
                                    }
                                    else
                                    {
                                        folder = folder.SubFolders[decodedUrl];
                                    }
                                }
                            }

                            return folder;
                        }
                    });
                }
            }
            catch (Exception)
            {
            }

            return folder;
        }

        /// <summary>
        /// Get all items that match the specified criteria
        /// </summary>
        /// <param name="pagingInfo">Paging info string returned from a previous execution</param>
        /// <param name="pageSize">Page size for the query</param>
        /// <param name="camlQuery">Search criteria in CAML format starting from the <Where> clause</param>
        /// <returns>Returns paged list of domain objects</returns>
        public SharePointPagedData<TEntity> GetAll(string pagingInfo, uint pageSize = 10, string camlQuery = null)
        {
            Logger.Logger.Debug("SharePointRepository.GetAll", "PagingInfo = {0}, Query = {1}", pagingInfo, camlQuery);

            SharePointPagedData<TEntity> result = Call(() =>
            {
                using (_context.Web)
                {
                    _context.Web.CacheAllSchema = false;

                    // access source list
                    var list = GetSourceList();

                    // build all items query
                    var query = new SPQuery();

                    if (!string.IsNullOrEmpty(camlQuery))
                        query.Query = camlQuery;

                    var items = list.GetItems(query);
                    var originalItems = items;

                    // count total items
                    var totalItems = originalItems.Count;

                    // set new row limit
                    RowLimit = pageSize > totalItems ? (uint)totalItems : pageSize;

                    query = new SPQuery()
                    {
                        RowLimit = RowLimit
                    };

                    if (!string.IsNullOrEmpty(camlQuery))
                        query.Query = camlQuery;

                    // paged search
                    if (!string.IsNullOrEmpty(pagingInfo))
                    {
                        query.ListItemCollectionPosition = new SPListItemCollectionPosition(pagingInfo);
                    }

                    // execute query
                    items = list.GetItems(query);

                    if (items.ListItemCollectionPosition != null && RowLimit > 0)
                    {
                        pagingInfo = items.ListItemCollectionPosition.PagingInfo;
                    }
                    else
                    {
                        pagingInfo = string.Empty;
                    }

                    return new SharePointPagedData<TEntity>(originalItems, PopulateItems(items), pagingInfo, RowLimit);
                }
            });

            return result;
        }

        /// <summary>
        /// Get all items all folders deep that match the specified criteria
        /// </summary>
        /// <param name="pagingInfo"></param>
        /// <param name="pageSize"></param>
        /// <param name="camlQuery"></param>
        /// <returns>Returns paged list of domain objects</returns>
        public SharePointPagedData<TEntity> GetAllRecursive(string pagingInfo, uint pageSize = 10, string camlQuery = null)
        {
            Logger.Logger.Debug("SharePointRepository.GetAllRecursive", "PagingInfo = {0}, PageSize = {1}, CamlQuery = {2}", pagingInfo, pageSize, camlQuery);

            var result = Call(() =>
            {
                using (_context.Web)
                {
                    // disable schema caching
                    _context.Web.CacheAllSchema = false;

                    // access source list
                    var list = GetSourceList();

                    // build all items query that will return the TotalItems property
                    var query = new SPQuery();

                    // in case a camlQuery parameter was given
                    if (!string.IsNullOrEmpty(camlQuery))
                        query.Query = camlQuery;

                    // execute all items query
                    var items = list.GetItems(query);

                    // keep record of the original dataset
                    var originalItems = items;

                    // count total items
                    var totalItems = items.Count;

                    // safe check for rowLimit
                    RowLimit = pageSize > totalItems ? (uint)totalItems : pageSize;

                    query = new SPQuery()
                    {
                        RowLimit = RowLimit,
                        ViewAttributes = "Scope=\"RecursiveAll\"" // force recursive search
                    };

                    if (!string.IsNullOrEmpty(camlQuery))
                        query.Query = camlQuery;

                    // paged search
                    if (!string.IsNullOrEmpty(pagingInfo))
                    {
                        query.ListItemCollectionPosition = new SPListItemCollectionPosition(pagingInfo);
                    }

                    // execute query with the search criteria
                    items = list.GetItems(query);

                    if (items.ListItemCollectionPosition != null && RowLimit > 0)
                    {
                        pagingInfo = items.ListItemCollectionPosition.PagingInfo;
                    }
                    else
                    {
                        pagingInfo = string.Empty;
                    }

                    return new SharePointPagedData<TEntity>(originalItems, PopulateItems(items), pagingInfo, RowLimit);
                }
            });

            return result;
        }

        /// <summary>
        /// Get all items all folders deep that match the specified criteria
        /// </summary>
        /// <param name="pagingInfo">Paging info string returned from a previous execution</param>
        /// <param name="pageSize">Optional page size for the query</param>
        /// <param name="camlQuery">Search criteria in CAML format starting from the <Where> clause</param>
        /// <returns>Returns paged list of domain objects</returns>
        public SharePointPagedData<TEntity> GetAllRecursive(string pagingInfo, Nullable<uint> pageSize, string camlQuery)
        {
            Logger.Logger.Debug("SharePointRepository.GetAllRecursive", "PagingInfo = {0}, PageSize = {1}, CamlQuery = {2}", pagingInfo, pageSize, camlQuery);

            var result = Call(() =>
            {
                using (_context.Web)
                {
                    // disable schema caching
                    _context.Web.CacheAllSchema = false;

                    // access source list
                    var list = GetSourceList();

                    // build all items query that will return the TotalItems property
                    var query = new SPQuery();

                    // in case a camlQuery parameter was given
                    if (!string.IsNullOrEmpty(camlQuery))
                        query.Query = camlQuery;

                    // execute all items query
                    var items = list.GetItems(query);

                    // keep record of the original dataset
                    var originalItems = items;

                    // count total items
                    var totalItems = items.Count;

                    // safe check for rowLimit
                    RowLimit = pageSize.HasValue ? (pageSize.Value > totalItems ? (uint)totalItems : pageSize.Value) : ((uint)totalItems);

                    query = new SPQuery()
                    {
                        RowLimit = RowLimit,
                        ViewAttributes = "Scope=\"RecursiveAll\"" // force recursive search
                    };

                    if (!string.IsNullOrEmpty(camlQuery))
                        query.Query = camlQuery;

                    // paged search
                    if (!string.IsNullOrEmpty(pagingInfo))
                    {
                        query.ListItemCollectionPosition = new SPListItemCollectionPosition(pagingInfo);
                    }

                    // execute query with the search criteria
                    items = list.GetItems(query);

                    if (items.ListItemCollectionPosition != null && RowLimit > 0)
                    {
                        pagingInfo = items.ListItemCollectionPosition.PagingInfo;
                    }
                    else
                    {
                        pagingInfo = string.Empty;
                    }

                    return new SharePointPagedData<TEntity>(originalItems, PopulateItems(items), pagingInfo, RowLimit);
                }
            });

            return result;
        }

        /// <summary>
        /// Get all items from an specified folder and subfolders that match the specified criteria
        /// </summary>
        /// <param name="folder">Folder relative to list root folder. For example: if the list url pattern is Lists/Orders/US, the parameter should be US</param>
        /// <param name="pagingInfo">Paging info string returned from a previous execution</param>
        /// <param name="pageSize">Page size for the query</param>
        /// <param name="camlQuery">Search criteria in CAML format starting from the <Where> clause</param>
        /// <returns>Returns paged list of domain objects</returns>
        public SharePointPagedData<TEntity> GetAllFromFolderRecursive(string folder, string pagingInfo = null, uint pageSize = 10, string camlQuery = null)
        {
            if (string.IsNullOrWhiteSpace(folder)) throw new ArgumentException("folder");

            Logger.Logger.Debug("SharePointRepository.GetAllFromFolderRecursive", "PagingInfo = {0}, Query = {1}, Folder = {2}", pagingInfo, camlQuery, folder);

            var result = Call(() =>
            {
                using (_context.Web)
                {
                    _context.Web.CacheAllSchema = false;

                    // access source list
                    var list = GetSourceList();

                    // build all items query
                    var query = new SPQuery();

                    if (!string.IsNullOrEmpty(camlQuery))
                        query.Query = camlQuery;

                    var items = list.GetItems(query);
                    var originalItems = items;

                    // count total items
                    var totalItems = items.Count;

                    // set new row limit
                    RowLimit = pageSize > totalItems ? (uint)totalItems : pageSize;

                    query = new SPQuery()
                    {
                        RowLimit = RowLimit,
                        ViewAttributes = "Scope=\"RecursiveAll\"",
                        Folder = list.RootFolder.SubFolders[folder]
                    };

                    if (!string.IsNullOrEmpty(camlQuery))
                        query.Query = camlQuery;

                    // paged search
                    if (!string.IsNullOrEmpty(pagingInfo))
                    {
                        query.ListItemCollectionPosition = new SPListItemCollectionPosition(pagingInfo);
                    }

                    // execute query
                    items = list.GetItems(query);

                    if (items.ListItemCollectionPosition != null && RowLimit > 0)
                    {
                        pagingInfo = items.ListItemCollectionPosition.PagingInfo;
                    }
                    else
                    {
                        pagingInfo = string.Empty;
                    }

                    return new SharePointPagedData<TEntity>(originalItems, PopulateItems(items), pagingInfo, RowLimit);
                }
            });

            return result;
        }

        /// <summary>
        /// Get all items from an specified folder that match the specified criteria
        /// </summary>
        /// <param name="folder">Folder relative to list root. For example: if the folder is Lists/Orders/US, the parameter should be US</param>
        /// <param name="pagingInfo">Paging info string returned from a previous execution</param>
        /// <param name="pageSize">Page size for the query</param>
        /// <param name="camlQuery">Search criteria in CAML format starting from the <Where> clause</param>
        /// <returns>Returns paged list of domain objects</returns>
        public SharePointPagedData<TEntity> GetAllFromFolder(string folder, string pagingInfo = null, uint pageSize = 10, string camlQuery = null)
        {
            Logger.Logger.Debug("SharePointRepository.GetAllFromFolder", "Folder = {0}, PagingInfo = {1}, PageSize = {2}, CamlQuery = {3}", folder, pagingInfo, pageSize, camlQuery);

            SharePointPagedData<TEntity> result = Call(() =>
            {
                using (_context.Web)
                {
                    // disable schema caching
                    _context.Web.CacheAllSchema = false;

                    // access source list
                    var list = GetSourceList();

                    // build all items query
                    var spQuery = new SPQuery();
                    var spFolder = FindFolder(folder);
                    if (spFolder != null)
                    {
                        spQuery.Folder = spFolder;
                    }

                    if (!string.IsNullOrWhiteSpace(camlQuery))
                        spQuery.Query = camlQuery;

                    // execute all items query
                    var items = list.GetItems(spQuery);
                    var originalItems = items;

                    // count total items
                    var totalItems = items.Count;

                    // set new row limit
                    RowLimit = pageSize > totalItems ? (uint)totalItems : pageSize;

                    if (spFolder != null)
                    {
                        spQuery = new SPQuery()
                        {
                            RowLimit = RowLimit,
                            Folder = spFolder
                        };
                    }
                    else
                    {
                        spQuery = new SPQuery()
                        {
                            RowLimit = RowLimit
                        };
                    }

                    if (!string.IsNullOrWhiteSpace(camlQuery))
                        spQuery.Query = camlQuery;

                    // paged search
                    if (!string.IsNullOrWhiteSpace(pagingInfo))
                    {
                        spQuery.ListItemCollectionPosition = new SPListItemCollectionPosition(pagingInfo);
                    }

                    // execute actual query
                    items = list.GetItems(spQuery);
                    if (items.ListItemCollectionPosition != null && RowLimit > 0)
                    {
                        pagingInfo = items.ListItemCollectionPosition.PagingInfo;
                    }
                    else
                    {
                        pagingInfo = string.Empty;
                    }

                    return new SharePointPagedData<TEntity>(originalItems, PopulateItems(items), pagingInfo, RowLimit);
                }
            });

            return result;
        }

        /// <summary>
        /// Get all items that match the specified criteria
        /// </summary>
        /// <param name="camlQuery">Search criteria in CAML format starting from the <Where> clause</param>
        /// <returns>Returns list of domain objects</returns>
        public ICollection<TEntity> GetAll(string camlQuery = null)
        {
            Logger.Logger.Debug("SharePointRepository.GetAll", "Query = {0}", camlQuery);

            ICollection<TEntity> result = Call(() =>
            {
                using (_context.Web)
                {
                    _context.Web.CacheAllSchema = false;

                    var list = GetSourceList();

                    SPQuery query = new SPQuery();

                    if (RowLimit > 0)
                        query.RowLimit = RowLimit;

                    if (!string.IsNullOrEmpty(camlQuery))
                        query.Query = camlQuery;

                    var items = list.GetItems(query);

                    var entities = PopulateItems(items);

                    return entities;
                }
            });

            return result;
        }

        /// <summary>
        /// Get all items that match the specified criteria
        /// </summary>
        /// <param name="camlQuery">Search criteria in CAML format starting from the <Where> clause</param>
        /// <returns>Returns list of domain objects</returns>
        public ICollection<TEntity> GetAllRecursive(string camlQuery = null)
        {
            Logger.Logger.Debug("SharePointRepository.GetAll", "Query = {0}", camlQuery);

            ICollection<TEntity> result = Call(() =>
            {
                using (_context.Web)
                {
                    _context.Web.CacheAllSchema = false;

                    var list = GetSourceList();

                    SPQuery query = new SPQuery();

                    query.ViewAttributes = "Scope=\"RecursiveAll\"";

                    if (RowLimit > 0)
                        query.RowLimit = RowLimit;

                    if (!string.IsNullOrEmpty(camlQuery))
                        query.Query = camlQuery;

                    var items = list.GetItems(query);

                    var entities = PopulateItems(items);

                    return entities;
                }
            });

            return result;
        }

        #region protected and private methods

        protected TResult Call<TResult>(Func<TResult> method)
        {
            try
            {
                return method();
            }
            catch (Exception exception)
            {
                Logger.Logger.Unexpected("ServiceCache.Call", exception.Message);
                throw;
            }
        }
        protected void Exec(Action method)
        {
            try
            {
                method();
            }
            catch (Exception exception)
            {
                Logger.Logger.Unexpected("ServiceCache.Exec", exception.Message);
                throw;
            }
        }
        protected int Insert(Fields<TEntity> fields)
        {
            Logger.Logger.Information("SharePointRepository<TEntity>.Insert", string.Format("List = {0}, Fields = {1}", Title, string.Join(",", fields.ItemDictionary.Select(i => string.Format("{0} = {1}", i.Key, i.Value)).ToArray())));

            return Call(() =>
            {
                using (_context.Web)
                {
                    var list = GetSourceList();

                    SPListItem newitem = list.AddItem();

                    bool allowUnsafeUpdates = _context.Web.AllowUnsafeUpdates;
                    _context.Web.AllowUnsafeUpdates = true;

                    foreach (var field in fields.ItemDictionary)
                    {
                        var columnName = GetFieldColumn(typeof(TEntity), field.Key);

                        if (field.Value is IEnumerable<Lookup>)
                        {
                            var fieldValues = new SPFieldLookupValueCollection();

                            foreach (var keyValuePair in (IEnumerable<Lookup>)field.Value)
                            {
                                fieldValues.Add(new SPFieldLookupValue
                                {
                                    LookupId = keyValuePair.Id
                                });
                            }
                            newitem[columnName] = fieldValues;
                            continue;
                        }

                        var lookup = field.Value as Lookup;
                        if (lookup != null)
                        {
                            newitem[columnName] = lookup.Id;
                            continue;
                        }

                        newitem[columnName] = field.Value;
                    }

                    newitem.Update();

                    _context.Web.AllowUnsafeUpdates = allowUnsafeUpdates;

                    return newitem.ID;
                }

            });
        }
        protected SPList GetSourceList()
        {
            return ListUtils.GetList(_context.Web, Title);
        }
        protected void Update(Fields<TEntity> fields)
        {
            Logger.Logger.Information("SharePointRepository<TEntity>.Update", "List = {0}, Fields = {1}", Title, string.Join(",", fields.ItemDictionary.Select(i => $"{i.Key} = {i.Value}")).ToArray());

            if (!fields.ItemDictionary.ContainsKey("Id"))
                throw new ArgumentException("Can not update the item without the Id field");

            var itemId = fields.ItemDictionary["Id"].ToString();

            Int32 id = 0;

            if (!Int32.TryParse(itemId, out id))
                throw new ArgumentException("Invalid Id");

            Exec(() =>
            {
                using (_context.Web)
                {
                    var list = GetSourceList();

                    bool allowUnsafeUpdates = _context.Web.AllowUnsafeUpdates;
                    _context.Web.AllowUnsafeUpdates = true;

                    var item = list.GetItemById(id);

                    foreach (var field in fields.ItemDictionary)
                    {
                        var columnName = GetFieldColumn(typeof(TEntity), field.Key);

                        // lógica de atualização de campo UserMulti e afins
                        if (field.Value is IEnumerable<Lookup>)
                        {
                            var fieldValues = new SPFieldLookupValueCollection();
                            var _multi = (IEnumerable<Lookup>)field.Value;
                            if (_multi != null)
                            {
                                foreach (var keyValuePair in _multi)
                                {
                                    fieldValues.Add(new SPFieldLookupValue
                                    {
                                        LookupId = keyValuePair.Id
                                    });
                                }
                                item[columnName] = fieldValues;
                                continue;
                            }
                        }
                        if (!field.Key.Equals("Id", StringComparison.InvariantCultureIgnoreCase))
                            item[columnName] = field.Value;
                    }

                    item.Update();
                    _context.Web.AllowUnsafeUpdates = allowUnsafeUpdates;
                }

            });
        }
        protected void Delete(int id)
        {
            Logger.Logger.Information("SharePointRepository<TEntity>.Delete", string.Format("List = {0}, ID = {1}", Title, id));

            Exec(() =>
            {
                using (_context.Web)
                {
                    var list = GetSourceList();

                    bool allowUnsafeUpdates = _context.Web.AllowUnsafeUpdates;
                    _context.Web.AllowUnsafeUpdates = true;

                    var item = list.GetItemById(id);

                    item.Delete();

                    _context.Web.AllowUnsafeUpdates = allowUnsafeUpdates;
                }
            });
        }
        protected File UploadImage(string fileName, Stream stream, Fields<TEntity> fields = null, int maxLength = 4000000)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new Exception("The file name can not be null.");

            if (stream.Length > maxLength)
                throw new Exception(string.Format("The maximum file size is {0}mb", ConvertBytesToMegabytes(maxLength)));

            string ext = Path.GetExtension(fileName).ToLower();

            if (ext == null || (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".tif" && ext != ".gif"))
                throw new Exception("The image is not in the correct format. The allowed formats are: gif, jpg, png, bmp, tif e jpeg.");

            fileName = fileName.Replace(" ", "-");

            return Call(() =>
            {
                using (_context.Web)
                {
                    var list = GetSourceList();

                    var fileRef = string.Format("{0}/{1}", list.RootFolder.ServerRelativeUrl, fileName);

                    bool allowUnsafeUpdates = _context.Web.AllowUnsafeUpdates;
                    _context.Web.AllowUnsafeUpdates = true;

                    var file = list.RootFolder.Files.Add(fileRef, stream, true);

                    if (fields != null)
                    {
                        foreach (var item in fields.ItemDictionary)
                        {
                            file.Item[item.Key] = item.Value;
                        }
                        file.Item.Update();

                    }

                    _context.Web.AllowUnsafeUpdates = allowUnsafeUpdates;

                    return new File
                    {
                        Url = fileRef,
                        Name = fileName,
                        Length = stream.Length,
                        Created = DateTime.Now,
                        Extension = ext,
                        Title = fileName
                    };
                }
            });
        }
        private string GetFieldColumn(Type type, string columnName)
        {
            return type.GetProperties().Single(p => p.Name == columnName).GetCustomAttribute<SharePointFieldAttribute>().Name;
        }
        private ICollection<TEntity> PopulateItems(SPListItemCollection items)
        {
            ICollection<TEntity> entities = new Collection<TEntity>();

            if (items.Count > 0)
            {
                foreach (var listItem in items.Cast<SPListItem>().ToList())
                {
                    var entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
                    SetProperties(entity, listItem);
                    entities.Add(entity);
                }
            }
            return entities;
        }
        private ICollection<TEntity> PopulateItems(ICollection<SPListItem> items)
        {
            ICollection<TEntity> entities = new Collection<TEntity>();

            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    var entity = (TEntity)Activator.CreateInstance(typeof(TEntity));
                    SetProperties(entity, item);
                    entities.Add(entity);
                }
            }
            return entities;
        }
        private void SetProperties(TEntity entity, SPListItem listItem)
        {
            const string FILE_SYSTEM_OBJECT_TYPE = "FileSystemObjectType";
            typeof(TEntity).GetProperties().ToList().ForEach(p =>
            {
                var customAttributes = p.GetCustomAttribute<SharePointFieldAttribute>();
                if(customAttributes != null)
                {
                    var columnName = customAttributes.Name;

                    // in case there is a propetry FileSystemObjectType on the TEntity object
                    if (string.Compare(columnName, FILE_SYSTEM_OBJECT_TYPE) == 0)
                    {
                        if (p.PropertyType == typeof(string))
                            p.SetValue(entity, listItem.FileSystemObjectType.ToString());
                    }
                    else
                    {
                        if (listItem.Fields.ContainsField(columnName))
                        {
                            var field = listItem.Fields.GetFieldByInternalName(columnName);
                            var value = listItem[columnName];
                            if (value != null)
                            {
                                p.SetValue(entity, ValidateValueType(field, value));
                            }
                        }
                        else
                        {
                            if (columnName.Equals("File"))
                            {
                                p.SetValue(entity, ValidateValueTypeFile(listItem.File));
                            }
                        }
                    }
                }
            });

        }
        private object ValidateValueTypeFile(SPFile file)
        {
            if (file == null)
                return file;

            return new File
            {
                Name = file.Name,
                Title = file.Title,
                Created = file.TimeCreated,
                Length = file.Length,
                Url = file.ServerRelativeUrl,
                Extension = Path.GetExtension(file.ServerRelativeUrl),
                Content = file.OpenBinary()
            };
        }
        private string GetListTitle()
        {
            return this.GetType().GetCustomAttribute<SharePointListAttribute>().Title;
        }
        private object ValidateValueType(SPField field, object value)
        {
            switch (field.Type)
            {
                case SPFieldType.Invalid:

                    var imageField = value as Microsoft.SharePoint.Publishing.Fields.ImageFieldValue;

                    if (imageField != null)
                    {
                        return imageField.ImageUrl;
                    }
                    if (value is double)
                    {
                        return (double)value;
                    }

                    break;
                case SPFieldType.Integer:
                    {
                        return Int32.Parse(value.ToString());

                    }
                case SPFieldType.Text:
                    {
                        return value.ToString();

                    }
                case SPFieldType.Note:
                    {
                        return value.ToString();

                    }
                case SPFieldType.DateTime:
                    {
                        return (DateTime)value;

                    }
                case SPFieldType.Counter:
                    {
                        return (Int32)value;

                    }
                case SPFieldType.Choice:
                    {
                        return value.ToString();

                    }
                case SPFieldType.Lookup:
                    {
                        var fieldLookupValue = value as SPFieldLookupValue;
                        if (fieldLookupValue != null)
                        {
                            var lookupValue = fieldLookupValue;
                            return new Lookup(lookupValue.LookupId, lookupValue.LookupValue);
                        }
                        var collection = value as SPFieldLookupValueCollection;
                        if (collection != null)
                        {
                            var lookupValueCollection = collection;
                            var lookups = lookupValueCollection.ToDictionary(i => i.LookupId, j => j.LookupValue);

                            var result = new Collection<Lookup>();

                            foreach (var lookup in lookups)
                            {
                                result.Add(new Lookup(lookup.Key, lookup.Value));
                            }

                            return result;
                        }
                        var stringLookup = value as string;
                        if (stringLookup != null)
                        {
                            if (stringLookup.IndexOf(";#") > 0)
                            {
                                var lkpValue = new SPFieldLookupValue(stringLookup);
                                return new Lookup(lkpValue.LookupId, lkpValue.LookupValue);
                            }

                            return value.ToString();
                        }

                        break;
                    }
                case SPFieldType.Boolean:
                    {
                        return (bool)value;

                    }
                case SPFieldType.Number:
                    {
                        return double.Parse(value.ToString());

                    }
                case SPFieldType.Currency:
                    {
                        return double.Parse(value.ToString());

                    }
                case SPFieldType.URL:
                    {
                        try
                        {
                            var urlValue = value as string;
                            if (urlValue.IndexOf(',') > 0)
                            {
                                var parts = urlValue.Split(',');

                                return new Url
                                {
                                    Uri = new Uri(parts[0]),
                                    Description = string.Join(",", parts.Skip(1).Select(i => i).ToArray())
                                };
                            }
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }

                        return null;
                    }
                case SPFieldType.Computed:
                    break;
                case SPFieldType.Threading:
                    break;
                case SPFieldType.Guid:
                    break;
                case SPFieldType.MultiChoice:
                    {
                        if (value.GetType() == typeof(string))
                        {
                            return value.ToString().Replace(";#", "|").Split('|').ToArray().Where(i => i != "").ToArray();
                        }
                    }

                    break;
                case SPFieldType.GridChoice:
                    break;
                case SPFieldType.Calculated:
                    break;
                case SPFieldType.File:

                    if (value == null)
                        return null;

                    File file = value as SPFile;
                    return file;

                case SPFieldType.Attachments:
                    break;
                case SPFieldType.User:
                    {
                        //Usado somente quando o campo permite somente selecionar um usuário ou grupo
                        if (value is string)
                        {
                            var stringLookup = value as string;
                            if (stringLookup != null)
                            {
                                if (stringLookup.IndexOf(";#") > 0)
                                {
                                    var lkpValue = new SPFieldLookupValue(stringLookup);

                                    return new Lookup(lkpValue.LookupId, lkpValue.LookupValue);
                                }
                                return stringLookup;
                            }
                        }

                        //Usado somente quando o campo permite selecionar vários usuários ou grupos
                        if (value is SPFieldUserValueCollection)
                        {
                            var userValues = value as SPFieldUserValueCollection;

                            if (userValues != null)
                            {
                                var result = new Collection<Lookup>();

                                foreach (var userValue in userValues)
                                {
                                    result.Add(new Lookup(userValue.LookupId, userValue.LookupValue));
                                }

                                return result;
                            }
                        }

                        return null;
                    }
                case SPFieldType.Recurrence:
                    break;
                case SPFieldType.CrossProjectLink:
                    break;
                case SPFieldType.ModStat:
                    break;
                case SPFieldType.Error:
                    break;
                case SPFieldType.ContentTypeId:
                    break;
                case SPFieldType.PageSeparator:
                    break;
                case SPFieldType.ThreadIndex:
                    break;
                case SPFieldType.WorkflowStatus:
                    break;
                case SPFieldType.AllDayEvent:
                    break;
                case SPFieldType.WorkflowEventType:
                    break;
                case SPFieldType.Geolocation:
                    break;
                case SPFieldType.OutcomeChoice:
                    break;
                case SPFieldType.MaxItems:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            throw new Exception(string.Format("Type \"{0}\" was not implemented.", field.Type));
        }
        private double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        #endregion

        #endregion
    }
}
