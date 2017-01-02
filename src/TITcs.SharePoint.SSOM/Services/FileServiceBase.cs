using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using TITcs.SharePoint.SSOM.Extensions;

namespace TITcs.SharePoint.SSOM.Services
{
    public abstract class FileServiceBase : serviceBase, IHttpHandler, IRequiresSessionState
    {

        public string FileName { get; set; }

        protected override void Process()
        {
            object result = "";

            try
            {
                result = InvokeMethod();
                Context.Response.Download(result as byte[], FileName);
            }
            catch (Exception e)
            {
                Logger.Logger.Unexpected("ServiceBase.ProcessRequest", e.Message);

                if (e.InnerException != null)
                {
                    Logger.Logger.Unexpected("ServiceBase.ProcessRequest.InnerException", e.InnerException.Message);
                }


                Context.Response.StatusCode = 500;
                Context.Response.TrySkipIisCustomErrors = true;

                result = Error(e);
                Context.Response.Write(result);
            }
        }
        
    }
}
