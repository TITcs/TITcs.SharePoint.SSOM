using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using TITcs.SharePoint.SSOM.Config;

namespace TITcs.SharePoint.SSOM.Services
{
    public class HandlerFactory : IHttpHandlerFactory
    {
        #region fields and properties

        private static readonly List<Type> _handlerTypes = new List<Type>();
        private static SharePointServiceSection _serviceSection;
        public List<Type> HandlerTypes { get { return _handlerTypes; } }

        #endregion

        #region constructors

        public HandlerFactory()
        {
            var hasLoadedTypes = false;

            try
            {
                if (_handlerTypes == null || _handlerTypes.Count == 0)
                {
                    _serviceSection = (SharePointServiceSection)ConfigurationManager.GetSection("sharePointServices");

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
                                    .ExportedTypes.Where(i => i.BaseType != null && i.BaseType.Name == "ServiceBase")
                                    .ToList();

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

                                    AddIfNotExistsExportedTypes(exportedTypes);

                                    hasLoadedTypes = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Logger.Unexpected("HandlerFactory.constructor", e.Message);

                ResponseJSON(HttpContext.Current.Response, e);
            }
        }

        private static void AddIfNotExistsExportedTypes(List<Type> exportedTypes)
        {
            foreach (var exportedType in exportedTypes)
            {
                if (!_handlerTypes.Contains(exportedType))
                    _handlerTypes.Add(exportedType);
            }
        }

        #endregion

        #region events and methods

        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            try
            {
                //if (_serviceSection.EnableCrossDomain)
                //{
                //    if (context.Request.UrlReferrer == null)
                //        throw new Exception("Invalid Cross Domain");

                //    var urlReferrer = string.Format("{0}://{1}", context.Request.UrlReferrer.Scheme,
                //        context.Request.UrlReferrer.Authority);
                //}

                var className = Path.GetFileNameWithoutExtension(context.Request.PhysicalPath);

                Logger.Logger.Information("HandlerFactory.GetHandler", string.Format("Variable className = {0}", className));

                var type = HandlerTypes.SingleOrDefault(i => i.Name.ToLower() == className.ToLower());

                if (type != null)
                {
                    Logger.Logger.Debug("HandlerFactory.GetHandler", string.Format("Instance of {0}", type.Name));

                    // TODO: IMPLEMENTAR CROSS DOMAIN POR ASSEMBLY

                    var handler = (IHttpHandler)Activator.CreateInstance(type);
                    return handler;
                }

                var message = string.Format("The service \"{0}{1}\" not defined", className, Path.GetExtension(context.Request.PhysicalPath));

                throw new HttpException(500, message);

            }
            catch (Exception ex)
            {
                Logger.Logger.Unexpected("HandlerFactory.GetHandler", ex.Message);

                ResponseJSON(context.Response, ex);
            }

            return null;
        }
        private void ResponseJSON(HttpResponse response, Exception e)
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
}