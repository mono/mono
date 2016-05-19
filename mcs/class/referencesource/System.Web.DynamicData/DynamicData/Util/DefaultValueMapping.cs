namespace System.Web.DynamicData.Util {
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;

    internal class DefaultValueMapping {
        private IDictionary<string, object> _defaultValues;

        public DictionaryCustomTypeDescriptor Instance {
            get;
            private set;
        }

        public IDictionary<string, object> Values {
            get {
                return _defaultValues;
            }
        }

        public DefaultValueMapping(IDictionary<string, object> defaultValues) {
            Debug.Assert(defaultValues != null);
            _defaultValues = defaultValues;
            // Build a custom type descriptor which will act as a lightweight wrapper around the dictionary.
            Instance = new DictionaryCustomTypeDescriptor(defaultValues);
        }

        public bool Contains(MetaColumn column) {
            Debug.Assert(_defaultValues != null);
            return Misc.IsColumnInDictionary(column, _defaultValues);
        }
    }
}
