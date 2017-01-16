using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Configuration;
using TITcs.SharePoint.SSOM.Config;

namespace TITcs.SharePoint.SSOM.Services
{
    public class HandlerFactory : IHttpHandlerFactory
    {
        #region fields and properties

        private static readonly IEqualityComparer<Type> _serviceBaseComparer = new ServiceBaseComparer();
        private static readonly string _configSection = "titSharePointSSOMServices";
        private static readonly object _lock = new object();
        private static readonly List<Type> _handlerTypes = new List<Type>();
        private static SharePointServiceSection _serviceSection;
        public static List<Type> HandlerTypes { get { return _handlerTypes; } }

        #endregion

        #region constructors

        static HandlerFactory()
        {
            lock (_lock)
            {
                var hasLoadedTypes = false;

                try
                {
                    if (_handlerTypes == null || _handlerTypes.Count == 0)
                    {
                        _serviceSection = (SharePointServiceSection)ConfigurationManager.GetSection(_configSection);

                        if (_serviceSection != null)
                        {
                            foreach (ServiceRegistry service in _serviceSection.Services)
                            {
                                List<Type> exportedTypes;

                                if (service.FilterType == FilterType.AssemblyName)
                                {
                                    if (string.IsNullOrEmpty(service.AssemblyName))
                                        throw new Exception("AssemblyName not defined");

                                    // load the services defined in the referenced assembly
                                    exportedTypes = Assembly.Load(service.AssemblyName)
                                        .ExportedTypes.Where(i => i.GetInterface("IHttpHandler", true) != null)
                                        .ToList();

                                    // load types
                                    AddIfNotExistsExportedTypes(exportedTypes);
                                }
                                else
                                {
                                    if (!hasLoadedTypes)
                                    {
                                        // load the services defined in the current assembly
                                        exportedTypes = AppDomain.CurrentDomain.GetAssemblies()
                                                                            .SelectMany(t => t.GetTypes())
                                                                            .Where(t => t.IsClass && t.IsPublic && t.Namespace == service.Namespace).ToList();

                                        // load types
                                        AddIfNotExistsExportedTypes(exportedTypes);

                                        // mark types as loaded
                                        hasLoadedTypes = true;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    // log error
                    Logger.Logger.Unexpected("HandlerFactory.constructor", e.Message);

                    // return error message
                    ResponseJSON(HttpContext.Current.Response, e);
                }
            }
        }

        private static void AddIfNotExistsExportedTypes(List<Type> exportedTypes)
        {
            foreach (var exportedType in exportedTypes)
            {
                if (!_handlerTypes.Contains(exportedType, _serviceBaseComparer))
                    _handlerTypes.Add(exportedType);
            }
        }

        #endregion

        #region events and methods

        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            try
            {
                // get service name
                var className = Path.GetFileNameWithoutExtension(context.Request.PhysicalPath);

                // log execution
                Logger.Logger.Information("HandlerFactory.GetHandler", string.Format("Variable className = {0}", className));

                // is there a service with the provided path?
                var type = HandlerTypes.SingleOrDefault(i => i.Name.ToLower() == className.ToLower());
                if (type != null)
                {
                    // log execution
                    Logger.Logger.Debug("HandlerFactory.GetHandler", string.Format("Instance of {0}", type.Name));

                    // create service instance to handle request
                    return (IHttpHandler)Activator.CreateInstance(type);
                }

                // customize error message
                var message = string.Format("The service \"{0}{1}\" not defined", className, Path.GetExtension(context.Request.PhysicalPath));

                // if no service is found
                throw new HttpException(500, message);
            }
            catch (Exception ex)
            {
                Logger.Logger.Unexpected("HandlerFactory.GetHandler", ex.Message);

                ResponseJSON(context.Response, ex);
            }

            return null;
        }
        private static void ResponseJSON(HttpResponse response, Exception e)
        {
            response.Clear();
            response.ContentType = "application/json; charset=utf-8";

            response.StatusCode = 500;
            response.TrySkipIisCustomErrors = true;

            response.Write(JsonConvert.SerializeObject(new
            {
                status = 500,
                exception = new
                {
                    message = e.Message
                }
            }));
        }
        public void ReleaseHandler(IHttpHandler handler)
        {
        }

        #endregion
    }

    #region IEqualityComparer implementation

    class ServiceBaseComparer : IEqualityComparer<Type>
    {
        public bool Equals(Type x, Type y)
        {
            return x.FullName == y.FullName;
        }

        public int GetHashCode(Type obj)
        {
            return obj.GetHashCode();
        }
    }

    #endregion
}