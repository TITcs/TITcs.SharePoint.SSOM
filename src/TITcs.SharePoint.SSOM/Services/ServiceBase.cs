using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using Newtonsoft.Json;


namespace TITcs.SharePoint.SSOM.Services
{
    public abstract class ServiceBase : IHttpHandler, IRequiresSessionState
    {
        private bool _isPost = false;
        private HttpContext _httpContext;
        private string _routeName = "";

        public bool IsReusable
        {
            get { return false; }
        }

        protected HttpContext Context
        {
            get { return _httpContext; }
        }
        protected bool IsPost
        {
            get { return _isPost; }
        }

        public dynamic Model { get; set; }

        protected string Route
        {
            get { return _routeName; }
        }

        public void ProcessRequest(HttpContext context)
        {
#if DEBUG
            Logger.Logger.Debug("ServiceBase.ProcessRequest", "Begin");
#endif

            _httpContext = context;
            _isPost = context.Request.RequestType.Equals("POST");

            Model = CreateModel(context);

            _routeName = ValidateRoute();
            object result = "";

            context.Response.Clear();
            context.Response.ContentType = "application/json; charset=utf-8";

            try
            {
                result = InvokeMethod();

            }
            catch (Exception e)
            {
                Logger.Logger.Unexpected("ServiceBase.ProcessRequest", e.Message);

                if (e.InnerException != null)
                {
                    Logger.Logger.Unexpected("ServiceBase.ProcessRequest.InnerException", e.InnerException.Message);
                }


                context.Response.StatusCode = 500;
                context.Response.TrySkipIisCustomErrors = true;

                result = Error(e);
            }

            context.Response.Write(JsonConvert.SerializeObject(result));

#if DEBUG
            Logger.Logger.Debug("ServiceBase.ProcessRequest", "End");
#endif
        }

        private dynamic CreateModel(HttpContext context)
        {
            var model = new ModelObject();

            var request = new Request(context.Request);

            foreach (var key in request.Keys.Where(i => i != null))
            {
                model.AddProperty(key, request.Values[key]);
            }

            return model;
        }

        private struct Request
        {
            public readonly string[] Keys;
            public readonly NameValueCollection Values;

            public Request(HttpRequest request)
            {
                switch (request.RequestType)
                {
                    case "POST":
                        Keys = request.Form.AllKeys;
                        Values = request.Form;
                        break;

                    case "GET":
                        Keys = request.QueryString.AllKeys;
                        Values = request.QueryString;
                        break;

                    default:
                        {
                            var message = $"Not implemented RequestType: {request.RequestType}";
                            Logger.Logger.Unexpected("ServiceBase.Request", message);
                            throw new Exception(message);
                        }
                }
            }
        }

        protected object Error(int rule, string message)
        {
            _httpContext.Response.StatusCode = 500;
            _httpContext.Response.TrySkipIisCustomErrors = true;

            return new
            {
                status = 500,
                exception = new
                {
                    number = rule,
                    message
                }
            };
        }

        protected object Error(string message)
        {
            _httpContext.Response.StatusCode = 500;
            _httpContext.Response.TrySkipIisCustomErrors = true;

            return new
            {
                status = 500,
                exception = new
                {
                    message
                }
            };
        }

        protected object Error(Exception exception)
        {
            return Error(exception.Message);
        }

        protected object Ok(object data)
        {
            return new
            {
                status = 200,
                data
            };
        }

        protected object BusinessRule(int rule, string description, object data)
        {
            _httpContext.Response.StatusCode = 400;
            _httpContext.Response.TrySkipIisCustomErrors = true;

            return new
            {
                status = 400,
                rule = new
                {
                    number = rule,
                    description
                },
                data
            };
        }

        private string ValidateRoute()
        {
            var url = Context.Request.Url.AbsoluteUri.ToLower();

            var index = url.IndexOf("?", StringComparison.InvariantCulture);

            if (index > -1)
            {
                var @params = url.Split('?');

                url = @params[0];
                goto getRoute;
            }

            index = url.IndexOf("/#", StringComparison.InvariantCulture);
            if (index > -1)
            {
                url = url.Substring(0, index);
                goto getRoute;
            }

            index = url.IndexOf("/!", StringComparison.InvariantCulture);
            if (index > -1)
            {
                url = url.Substring(0, index);
                goto getRoute;
            }

        getRoute:

            if (url.EndsWith("/"))
                url = url.Substring(0, url.Length - 1);

            var route = url.ToLower().Split('/').ToList().Last();

            if (route.IndexOf(".sps", StringComparison.InvariantCulture) > -1)
            {
                var message = "The route name has not been defined";
                Logger.Logger.Unexpected("ServiceBase.ValidateRoute", message);
                throw new Exception(message);
            }

            return route;
        }
        private object InvokeMethod()
        {
            var metodo = GetType()
                .GetMethods().SingleOrDefault(i => i.GetCustomAttributes(typeof(RouteAttribute), false).Length > 0 && i.Name.ToLower() == _routeName);

            if (metodo == null)
            {
                var message = $"The {_routeName} route was not found";
                Logger.Logger.Unexpected("ServiceBase.InvokeMethod", message);
                throw new Exception(message);
            }

            return metodo.Invoke(this, new object[] { });
        }
    }
}
