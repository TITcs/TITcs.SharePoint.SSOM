using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TITcs.SharePoint.SSOM.Services
{
    public class SignalRServiceSection : ConfigurationSection
    {
        #region fields and properties

        [ConfigurationProperty("assemblyName", DefaultValue = "", IsRequired = true)]
        public string AssemblyName
        {
            get { return (string)this["assemblyName"]; }
            set { this["assemblyName"] = value; }
        }

        #endregion
    }
}
