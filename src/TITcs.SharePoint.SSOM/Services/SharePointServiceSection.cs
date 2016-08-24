using System.Configuration;

namespace TITcs.SharePoint.SSOM.Services
{
    public class SharePointServiceSection : ConfigurationSection
    {
        [ConfigurationProperty("assemblyName", DefaultValue = "", IsRequired = false)]
        public string AssemblyName
        {
            get { return (string)this["assemblyName"]; }
            set { this["assemblyName"] = value; }
        }

        [ConfigurationProperty("filterType", DefaultValue = FilterType.AssemblyName, IsRequired = false)]
        public FilterType FilterType
        {
            get { return (FilterType)this["filterType"]; }
            set { this["filterType"] = value; }
        }

        [ConfigurationProperty("namespace", DefaultValue = "", IsRequired = false)]
        public string Namespace
        {
            get { return (string)this["namespace"]; }
            set { this["namespace"] = value; }
        }

        [ConfigurationProperty("enableCrossDomain", DefaultValue = false, IsRequired = false)]
        public bool EnableCrossDomain
        {
            get { return (bool)this["enableCrossDomain"]; }
            set { this["enableCrossDomain"] = value; }
        }
    }
}
