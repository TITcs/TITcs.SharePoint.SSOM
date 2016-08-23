using System.Web;
using System.Web.SessionState;

namespace TITcs.SharePoint.SOM.Services
{
    public class ServiceContext
    {
        private HttpContext _httpContext;

        public ServiceContext(HttpContext httpContext)
        {
            _httpContext = httpContext;
        }

        public string RequestType
        {
            get { return _httpContext.Request.RequestType; }
        }
        public dynamic Model { get; set; }

        public HttpRequest Request
        {
            get { return _httpContext.Request; }
        }

        public HttpResponse Response
        {
            get { return _httpContext.Response; }
        }

        public HttpSessionState Session
        {
            get { return _httpContext.Session; }
        }
    }
}
