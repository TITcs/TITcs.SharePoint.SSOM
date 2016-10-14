using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TITcs.SharePoint.SSOM.Services
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ServiceConnectionSlug : Attribute
    {
        #region fields and properties

        public string Slug { get; set; }

        #endregion
    }
}
