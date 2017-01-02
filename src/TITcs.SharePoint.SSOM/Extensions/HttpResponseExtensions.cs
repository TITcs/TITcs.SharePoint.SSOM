using System;
using System.Web;

namespace TITcs.SharePoint.SSOM.Extensions
{
    public static class HttpResponseExtensions
    {
        public static void Download(this HttpResponse response, byte[] content, string filename)
        {
            response.ContentType = "application/octet-stream";
            response.AddHeader("Content-Disposition", String.Format("attachment;filename=\"{0}\"", filename));
            response.AddHeader("Content-Length", content.Length.ToString());
            response.BinaryWrite(content);
            response.Flush();
        }
    }
}
