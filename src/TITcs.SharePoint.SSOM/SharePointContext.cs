using Microsoft.SharePoint;

namespace TITcs.SharePoint.SSOM
{
    public class SharePointContext : ISharePointContext
    {
        public SharePointContext(SPWeb web)
        {
            Web = web;
        }
        public SPWeb Web { get; }
    }
}
