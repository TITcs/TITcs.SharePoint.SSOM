using Microsoft.SharePoint;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using TITcs.SharePoint.SSOM.Utils;

namespace TITcs.SharePoint.SSOM
{
    public abstract class SharePointRepository<TEntity> : ISharePointRepository<TEntity> where TEntity : class
    {
        private readonly SPWeb _rootWeb;

        protected SharePointRepository()
            :this(SPContext.Current.Web)
        {
        }

        protected SharePointRepository(SPWeb rootWeb)
        {
            _rootWeb = rootWeb;

            Title = GetListTitle();

            RowLimit = 0;

            Logger.Logger.Debug("SharePointRepository.Constructor", "Title = {0}", Title);
        }

        public string Title { get; set; }
        public uint RowLimit { get; set; }
        public SPWeb Context  { get { return _rootWeb; } }

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
                using (_rootWeb)
                {
                    var list = _rootWeb.Lists.TryGetList(Title);

                    if (list == null)
                        throw new Exception(string.Format("The list \"{0}\" not found", Title));

                    SPQuery query = new SPQuery
                    {
                        Query = string.Format("<Where><Eq><FieldRef Name='ID' /><Value Type='Counter'>{0}</Value></Eq></Where>", id)
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

        protected int Insert(Fields<TEntity> fields)
        {
            Logger.Logger.Information("SharePointRepository<TEntity>.Insert", string.Format("List = {0}, Fields = {1}", Title, string.Join(",", fields.ItemDictionary.Select(i => string.Format("{0} = {1}", i.Key, i.Value)).ToArray())));

            return Call(() =>
            {
                using (_rootWeb)
                {
                    var list = GetSourceList();

                    SPListItem newitem = list.AddItem();

                    bool allowUnsafeUpdates = _rootWeb.AllowUnsafeUpdates;
                    _rootWeb.AllowUnsafeUpdates = true;

                    foreach (var field in fields.ItemDictionary)
                    {
                        var columnName = getFieldColumn(typeof(TEntity), field.Key);

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

                    _rootWeb.AllowUnsafeUpdates = allowUnsafeUpdates;

                    return newitem.ID;
                }

            });
        }

        protected SPList GetSourceList()
        {
            return ListUtils.GetList(_rootWeb, Title);
        }

        protected void Update(Fields<TEntity> fields)
        {
            Logger.Logger.Information("SharePointRepository<TEntity>.Update", string.Format("List = {0}, Fields = {1}", Title, string.Join(",", fields.ItemDictionary.Select(i => string.Format("{0} = {1}", i.Key, i.Value)).ToArray())));

            if (!fields.ItemDictionary.ContainsKey("Id"))
                throw new ArgumentException("Can not update the item without the Id field");

            var itemId = fields.ItemDictionary["Id"].ToString();

            Int32 id = 0;

            if (!Int32.TryParse(itemId, out id))
                throw new ArgumentException("Invalid Id");

            Exec(() =>
            {
                using (_rootWeb)
                {
                    var list = GetSourceList();

                    bool allowUnsafeUpdates = _rootWeb.AllowUnsafeUpdates;
                    _rootWeb.AllowUnsafeUpdates = true;

                    var item = list.GetItemById(id);

                    foreach (var field in fields.ItemDictionary)
                    {
                        var columnName = getFieldColumn(typeof(TEntity), field.Key);

                        if (!field.Key.Equals("Id", StringComparison.InvariantCultureIgnoreCase))
                            item[columnName] = field.Value;
                    }

                    item.Update();

                    _rootWeb.AllowUnsafeUpdates = allowUnsafeUpdates;
                }

            });
        }

        private string getFieldColumn(Type type, string columnName)
        {
            return type.GetProperties().Single(p => p.Name == columnName).GetCustomAttribute<SharePointFieldAttribute>().Name;
        }

        public SharePointPagedData<TEntity> GetAll(string lastPosition, string camlQuery = null)
        {
            Logger.Logger.Debug("SharePointRepository.GetAll", "Lastposition = {0}, Query = {1}", lastPosition, camlQuery);

            var result = Call(() =>
            {
                using (_rootWeb)
                {
                    _rootWeb.CacheAllSchema = false;

                    var list = GetSourceList();

                    SPQuery query = new SPQuery();

                    if (!string.IsNullOrEmpty(camlQuery))
                        query.Query = camlQuery;

                    var items = list.GetItems(query);

                    var totalItems = items.Count;

                    query = new SPQuery()
                    {
                        RowLimit = RowLimit
                    };

                    if (!string.IsNullOrEmpty(camlQuery))
                        query.Query = camlQuery;

                    if (!string.IsNullOrEmpty(lastPosition))
                    {
                        var pos = new SPListItemCollectionPosition(lastPosition);
                        query.ListItemCollectionPosition = pos;
                    }

                    items = list.GetItems(query);

                    if (items.ListItemCollectionPosition != null && RowLimit > 0)
                    {
                        lastPosition = items.ListItemCollectionPosition.PagingInfo;
                    }

                    var entities = PopulateItems(items);

                    return new SharePointPagedData<TEntity>(entities, lastPosition, totalItems);
                }
            });

            return result;
        }

        public ICollection<TEntity> GetAll(string camlQuery = null)
        {
            Logger.Logger.Debug("SharePointRepository.GetAll", "Query = {0}", camlQuery);

            ICollection<TEntity> result = Call(() =>
            {
                using (_rootWeb)
                {
                    _rootWeb.CacheAllSchema = false;

                    var list = GetSourceList();

                    SPQuery query = new SPQuery();

                    if(RowLimit > 0)
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
                var columnName = p.GetCustomAttribute<SharePointFieldAttribute>().Name;

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

        protected void Delete(int id)
        {
            Logger.Logger.Information("SharePointRepository<TEntity>.Delete", string.Format("List = {0}, ID = {1}", Title, id));

            Exec(() =>
            {
                using (_rootWeb)
                {
                    var list = GetSourceList();

                    bool allowUnsafeUpdates = _rootWeb.AllowUnsafeUpdates;
                    _rootWeb.AllowUnsafeUpdates = true;

                    var item = list.GetItemById(id);

                    item.Delete();

                    _rootWeb.AllowUnsafeUpdates = allowUnsafeUpdates;
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
                using (_rootWeb)
                {
                    var list = GetSourceList();

                    var fileRef = string.Format("{0}/{1}", list.RootFolder.ServerRelativeUrl, fileName);

                    bool allowUnsafeUpdates = _rootWeb.AllowUnsafeUpdates;
                    _rootWeb.AllowUnsafeUpdates = true;

                    var file = list.RootFolder.Files.Add(fileRef, stream, true);

                    if (fields != null)
                    {
                        foreach (var item in fields.ItemDictionary)
                        {
                            file.Item[item.Key] = item.Value;
                        }
                        file.Item.Update();

                    }

                    _rootWeb.AllowUnsafeUpdates = allowUnsafeUpdates;

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

        private double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }
    }

}
