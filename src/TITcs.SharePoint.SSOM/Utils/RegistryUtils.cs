using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TITcs.SharePoint.SSOM.Utils
{
    public static class RegistryUtils
    {
        /// <summary>
        /// Reads an entry from de windows registry.
        /// </summary>
        /// <param name="keyName">The full registry path of the key, beginning with a valid registry root, such as "HKEY_CURRENT_USER".</param>
        /// <param name="valueName">The name of the name/value pair.</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static object Read(string keyName, string valueName)
        {
            return Registry.GetValue(keyName, valueName, default(object));
        }
    }
}
