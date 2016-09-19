
using Microsoft.SharePoint;

namespace TITcs.SharePoint.SSOM
{
    public interface ISharePointContext
    {
        SPWeb Web { get; }
    }
}
