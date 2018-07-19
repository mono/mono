namespace System.Web.DynamicData.Util {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    internal class DictionaryCustomTypeDescriptor : CustomTypeDescriptor {
        private IDictionary<string, object> _values;
        private PropertyDescriptorCollection _properties;

        public DictionaryCustomTypeDescriptor(IDictionary<string, object> values) {
            if (values == null) {
                throw new ArgumentNullException("values");
            }
            _values = values;
        }

        public object GetValue(string name) {
            object value;
            if (_values.TryGetValue(name, out value)) {
                return value;
            }
            return null;
        }

        public override PropertyDescriptorCollection GetProperties() {
            if (_properties == null) {
                var dictionaryProps = _values.Keys.Select(propName => new DictionaryPropertyDescriptor(propName));
                _properties = new PropertyDescriptorCollection(dictionaryProps.ToArray());
            }
            return _properties;
        }
    }

    internal class DictionaryPropertyDescriptor : PropertyDescriptor {
        public DictionaryPropertyDescriptor(string name)
            : base(name, null /* attrs */) {
        }

        public override bool CanResetValue(object component) {
            throw new NotSupportedException();
        }

        public override Type ComponentType {
            get {
                throw new NotSupportedException();
            }
        }

        public override object GetValue(object component) {
            // Try to cast the component to a DictionaryCustomTypeDescriptor and get the value in the dictonary
            //  that corresponds to this property
            DictionaryCustomTypeDescriptor typeDescriptor = component as DictionaryCustomTypeDescriptor;
            if (typeDescriptor == null) {
                return null;
            }

            return typeDescriptor.GetValue(Name);
        }

        public override bool IsReadOnly {
            get {
                throw new NotSupportedException();
            }
        }

        public override Type PropertyType {
            get {
                throw new NotSupportedException();
            }
        }

        public override void ResetValue(object component) {
            throw new NotSupportedException();
        }

        public override void SetValue(object component, object value) {
            throw new NotSupportedException();
        }

        public override bool ShouldSerializeValue(object component) {
            throw new NotSupportedException();
        }
    }
}
