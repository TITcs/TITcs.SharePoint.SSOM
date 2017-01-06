using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Server.Search.Administration;
using Microsoft.Office.Server.Search.Query;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;
using Microsoft.SharePoint.Publishing.Internal;

namespace TITcs.SharePoint.SSOM.Utils
{
    public class SearchUtils
    {
        public enum SearchCategory
        {
            Basic,
            Business_Data,
            Document_Parser,
            Internal,
            Mail,
            Notes,
            Office,
            People,
            SharePoint,
            Tiff,
            Web,
            XML
        }

        public static void CreateManagedProperty(string name, string crawledName, ManagedDataType type, SearchCategory searchCategory = SearchCategory.SharePoint, bool searchable = true, bool refinable = true, bool retrievable = true, bool sortable = true, bool hasMultipleValues = false, bool safeForAnonymous = false, bool tokenNormalization = false)
        {
            // Get the default service context
            SPServiceContext context = SPServiceContext.GetContext(SPServiceApplicationProxyGroup.Default, SPSiteSubscriptionIdentifier.Default);// Get the search service application proxy
            var searchProxy = context.GetDefaultProxy(typeof(SearchServiceApplicationProxy)) as SearchServiceApplicationProxy;

            // Get the search service application info object so we can find the Id of our Search Service App
            if (searchProxy != null)
            {
                SearchServiceApplicationInfo ssai = searchProxy.GetSearchServiceApplicationInfo();

                // Get the application itself
                var application = SearchService.Service.SearchApplications.GetValue<SearchServiceApplication>(ssai.SearchServiceApplicationId);

                // Get the schema of our Search Service Application
                Schema schema = new Schema(application);

                if (schema.AllManagedProperties.SingleOrDefault(i => i.Name == name) != null)
                {
                    Logger.Logger.Debug("SearchUtils.CreateManagedProperty", $"The property \"{name}\" has exists.");
                    return;
                }

                var categoryName = Enum.GetName(typeof(SearchCategory), searchCategory).Replace("_", " ");

                Category category = schema.AllCategories.Single(i => i.Name == categoryName);

                CrawledProperty crawledProperty = category.GetAllCrawledProperties().SingleOrDefault(i => i.Name == crawledName);

                if (crawledProperty != null)
                {
                    // Get all the managed properties
                    ManagedPropertyCollection properties = schema.AllManagedProperties;

                    // Add a new property
                    ManagedProperty property = properties.Create(name, type);
                    property.Searchable = searchable;
                    property.Refinable = refinable;
                    property.Retrievable = retrievable;
                    property.Sortable = sortable;
                    property.HasMultipleValues = hasMultipleValues;
                    property.TokenNormalization = tokenNormalization;
                    property.SafeForAnonymous = safeForAnonymous;

                    // Get the current mappings
                    MappingCollection mappings = property.GetMappings();

                    // Add a new mapping to a previously crawled field
                    var myMapping = new Mapping();
                    myMapping.CrawledPropertyName = crawledProperty.Name;
                    myMapping.CrawledPropset = crawledProperty.Propset;
                    myMapping.ManagedPid = property.PID;

                    // Add the mapping
                    mappings.Add(myMapping);

                    // Update the collection of mappings
                    property.SetMappings(mappings);

                    // Write the changes back
                    property.Update();
                }

            }
        }
    }
}
