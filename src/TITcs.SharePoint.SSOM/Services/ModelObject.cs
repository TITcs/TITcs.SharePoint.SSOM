using System.Collections.Generic;
using System.Dynamic;

namespace TITcs.SharePoint.SSOM.Services
{
    public sealed class ModelObject : DynamicObject
    {
        private readonly Dictionary<string, object> _properties;

        public ModelObject()
        {
            _properties = new Dictionary<string, object>();
        }

        public ModelObject(Dictionary<string, object> properties)
        {
            _properties = properties;
        }

        public void AddProperty(string key, object value)
        {
            _properties.Add(key, value);
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return _properties.Keys;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_properties.ContainsKey(binder.Name))
            {
                result = _properties[binder.Name];
                return true;
            }

            result = null;
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (_properties.ContainsKey(binder.Name))
            {
                _properties[binder.Name] = value;
                return true;
            }

            return false;
        }
    }
}
