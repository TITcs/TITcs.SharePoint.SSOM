using Microsoft.SharePoint;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using TITcs.SharePoint.SSOM.Utils;

namespace TITcs.SharePoint.SSOM
{
    public abstract class SharePointRepository<TEntity> : ISharePointRepository<TEntity> where TEntity : SharePointItem
    {
        #region properties and fields

        private readonly ISharePointContext _context;
        public ISharePointContext Context => _context;
        public string Title { get; set; }
        public uint RowLimit { get; set; }

        #endregion

        #region constructors

        protected SharePointRepository()
            : this(SPContext.Current.Web)
        {
        }

        protected SharePointRepository(ISharePointContext context)
        {
            _context = context;

            Title = GetListTitle();
            RowLimit = 0;

            Logger.Logger.Debug("SharePointRepository.Constructor", "Title = {0}", Title);
        }

        protected SharePointRepository(SPWeb web)
        {
            _context = new SharePointContext(web);

            Title = GetListTitle();
            RowLimit = 0;

            Logger.Logger.Debug("SharePointRepository.Constructor", "Title = {0}", Title);
        }

        #endregion

        #region methods

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
        public TEntity GetById(int id)
        {
            Logger.Logger.Debug("SharePointRepository.GetById", "ID = {0}", id);

            TEntity result = Call(() =>
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
        protected int Insert(Fields<TEntity> fields)
        {
            Logger.Logger.Information("SharePointRepository<TEntity>.Insert", $"List = {Title}, Fields = {string.Join(",", fields.ItemDictionary.Select(i => $"{i.Key} = {i.Value}").ToArray())}");

            return Call(() =>
            {
                using (_context.Web)
                {
                    var list = GetSourceList();

                    SPListItem item = list.AddItem();

                    bool allowUnsafeUpdates = _context.Web.AllowUnsafeUpdates;
                    _context.Web.AllowUnsafeUpdates = true;

                    foreach (var field in fields.ItemDictionary)
                    {
                         ValidateFieldItemDictionary(field, item);
                    }

                    item.Update();

                    _context.Web.AllowUnsafeUpdates = allowUnsafeUpdates;

                    return item.ID;
                }

            });
        }

        private void ValidateFieldItemDictionary(KeyValuePair<string, object> field, SPListItem listItem)
        {
            if (field.Key.Equals("Id", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var columnName = getFieldColumn(typeof(TEntity), field.Key);

            if (field.Value is IEnumerable<Lookup>)
            {
                var fieldValues = new SPFieldLookupValueCollection();

                foreach (var keyValuePair in (IEnumerable<Lookup>) field.Value)
                {
                    fieldValues.Add(new SPFieldLookupValue
                    {
                        LookupId = keyValuePair.Id
                    });
                }
                listItem[columnName] = fieldValues;
                return;
            }

            var lookup = field.Value as Lookup;
            if (lookup != null)
            {
                listItem[columnName] = lookup.Id;
                return;
            }

            listItem[columnName] = field.Value;
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
                        ValidateFieldItemDictionary(field, item);
                    }

                    item.Update();
                    _context.Web.AllowUnsafeUpdates = allowUnsafeUpdates;
                }

            });
        }
        private string getFieldColumn(Type type, string columnName)
        {
            return type.GetProperties().Single(p => p.Name == columnName).GetCustomAttribute<SharePointFieldAttribute>().Name;
        }
        public SharePointPagedData<TEntity> GetAll(string pagingInfo, uint pageSize = 10, string camlQuery = null)
        {
            Logger.Logger.Debug("SharePointRepository.GetAll", "PagingInfo = {0}, Query = {1}", pagingInfo, camlQuery);

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
        private void SetProperties(TEntity entity, SPListItem listItem)
        {
            typeof(TEntity).GetProperties().ToList().ForEach(p =>
            {
                var customAttribute = p.GetCustomAttribute<SharePointFieldAttribute>();

                if (customAttribute != null)
                {
                    var columnName = p.GetCustomAttribute<SharePointFieldAttribute>().Name;

                    if (listItem.Fields.ContainsField(columnName))
                    {
                        var field = listItem.Fields.GetFieldByInternalName(columnName);

                        if (listItem[field.Id] != null || string.IsNullOrEmpty(field.DefaultValue))
                        {
                            var value = listItem[field.Id];

                            if (value != null)
                            {
                                p.SetValue(entity, ValidateValueType(field, value));
                            }
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

            throw new Exception($"Type \"{field.Type}\" was not implemented.");
        }
        protected void Delete(int id)
        {
            Logger.Logger.Information("SharePointRepository<TEntity>.Delete", $"List = {Title}, ID = {id}");

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
        protected File Upload(string fileName, Stream stream, Fields<TEntity> fields = null, int maxLength = 4000000)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new Exception("The file name can not be null.");

            if (stream.Length > maxLength)
                throw new Exception($"The maximum file size is {ConvertBytesToMegabytes(maxLength)}mb");

            string ext = Path.GetExtension(fileName).ToLower();

            fileName = fileName.Replace(" ", "-");

            return Call(() =>
            {
                using (_context.Web)
                {
                    var list = GetSourceList();

                    var fileRef = $"{list.RootFolder.ServerRelativeUrl}/{fileName}";

                    bool allowUnsafeUpdates = _context.Web.AllowUnsafeUpdates;
                    _context.Web.AllowUnsafeUpdates = true;

                    var spFile = list.RootFolder.Files.Add(fileRef, stream, true);

                    if (fields != null)
                    {
                        foreach (var field in fields.ItemDictionary)
                        {
                            ValidateFieldItemDictionary(field, spFile.Item);
                        }

                        spFile.Item.Update();
                    }

                    _context.Web.AllowUnsafeUpdates = allowUnsafeUpdates;

                    var file = new File
                    {
                        Id = Convert.ToInt32(spFile.Item["ID"]),
                        Url = fileRef,
                        Name = fileName,
                        Length = stream.Length,
                        Created = DateTime.Now,
                        Extension = ext,
                        Title = fileName
                    };

                    return file;
                }
            });
        }
        protected File UploadImage(string fileName, Stream stream, Fields<TEntity> fields = null, int maxLength = 4000000)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new Exception("The file name can not be null.");

            if (stream.Length > maxLength)
                throw new Exception($"The maximum file size is {ConvertBytesToMegabytes(maxLength)}mb");

            string ext = Path.GetExtension(fileName).ToLower();

            if (ext == null || (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".tif" && ext != ".gif"))
                throw new Exception("The image is not in the correct format. The allowed formats are: gif, jpg, png, bmp, tif e jpeg.");

            fileName = fileName.Replace(" ", "-");

            return Call(() =>
            {
                using (_context.Web)
                {
                    var list = GetSourceList();

                    var fileRef = $"{list.RootFolder.ServerRelativeUrl}/{fileName}";

                    bool allowUnsafeUpdates = _context.Web.AllowUnsafeUpdates;
                    _context.Web.AllowUnsafeUpdates = true;

                    var spFile = list.RootFolder.Files.Add(fileRef, stream, true);

                    if (fields != null)
                    {
                        foreach (var field in fields.ItemDictionary)
                        {
                            ValidateFieldItemDictionary(field, spFile.Item);
                        }

                        spFile.Item.Update();

                    }

                    _context.Web.AllowUnsafeUpdates = allowUnsafeUpdates;

                    var file = new File
                    {
                        Id = Convert.ToInt32(spFile.Item["ID"]),
                        Url = fileRef,
                        Name = fileName,
                        Length = stream.Length,
                        Created = DateTime.Now,
                        Extension = ext,
                        Title = fileName
                    };

                    return file;
                }
            });
        }
        private double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        #endregion
    }

}
