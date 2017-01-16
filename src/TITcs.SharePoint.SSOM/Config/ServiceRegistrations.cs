using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TITcs.SharePoint.SSOM.Config
{
    [ConfigurationCollection(typeof(ServiceRegistry), AddItemName = "service")]
    public class ServiceRegistrations : ConfigurationElementCollection, IEnumerable<ServiceRegistry>
    {
        #region constructors

        public ServiceRegistrations()
        {  
        }

        #endregion

        #region overrides

        protected override ConfigurationElement CreateNewElement()
        {
            return new ServiceRegistry();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var _elem = element as ServiceRegistry;
            if (_elem != null)
                return _elem.AssemblyName;
            else
                return null;;
        }

        public ServiceRegistry this[int index]
        {
            get
            {
                return BaseGet(index) as ServiceRegistry;
            }
        }

        #region IEnumerable<ServiceRegistry>
        IEnumerator<ServiceRegistry> IEnumerable<ServiceRegistry>.GetEnumerator()
        {
            return (from i in Enumerable.Range(0, this.Count) select this[i]).GetEnumerator();
        }

        #endregion

        #endregion
    }
}
