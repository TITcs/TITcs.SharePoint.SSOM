using System.Configuration;
using TITcs.SharePoint.SSOM.Config;

namespace TITcs.SharePoint.SSOM.Services
{
    public class SharePointServiceSection : ConfigurationSection
    {
        #region fields and properties

        private static SharePointServiceSection _instance;
        private static readonly string _configSection = "titSharePointSSOMServices";
        [ConfigurationProperty("services", IsDefaultCollection = false)]
        public ServiceRegistrations Services {
            get {
                ServiceRegistrations elems = (ServiceRegistrations) base["services"];
                return elems;
            }
        }

        #endregion

        #region events and methods

        public static SharePointServiceSection Open()
        {
            var ass = System.Reflection.Assembly.GetEntryAssembly();
            return Open(ass.Location);
        }
        public static SharePointServiceSection Open(string path)
        {
            if((object) _instance == null)
            {
                if (path.EndsWith(".config", System.StringComparison.InvariantCultureIgnoreCase))
                    path = path.Remove(path.Length - 7);

                var config = ConfigurationManager.OpenExeConfiguration(path);
                if (config != null)
                {
                    _instance = (SharePointServiceSection)config.Sections[_configSection];
                }
            }

            return _instance;
        }

        #endregion
    }
}
