using System;

namespace TITcs.SharePoint.SSOM
{
    public interface ISharePointItem
    {
        int Id { get; set; }
        DateTime Created { get; set; }
        Lookup Author { get; set; }
    }
}
