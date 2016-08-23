using System;

namespace TITcs.SharePoint.SOM
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false)]
    public class SharePointFieldAttribute : Attribute
    {
        public string Name { get; set; }
        public bool LookupText { get; set; }

        public SharePointFieldAttribute(string fieldName, bool lookupText = false)
        {
            Name = fieldName;
            LookupText = lookupText;
        }
    }
}