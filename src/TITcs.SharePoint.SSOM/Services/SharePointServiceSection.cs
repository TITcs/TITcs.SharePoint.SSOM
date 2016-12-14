using System.Configuration;
using TITcs.SharePoint.SSOM.Config;

namespace TITcs.SharePoint.SSOM.Services
{
    public class SharePointServiceSection : ConfigurationSection
    {
        #region fields and properties

        [ConfigurationProperty("services", IsDefaultCollection = false)]
        public ServiceRegistrations Services {
            get {
                ServiceRegistrations elems = (ServiceRegistrations) base["services"];
                return elems;
            }
        }

        #endregion
    }
}
