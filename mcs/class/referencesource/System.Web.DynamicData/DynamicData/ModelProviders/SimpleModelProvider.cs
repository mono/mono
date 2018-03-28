namespace System.Web.DynamicData.ModelProviders {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    internal class SimpleDataModelProvider : DataModelProvider {
        private List<TableProvider> _tables = new List<TableProvider>();
        
        public SimpleDataModelProvider(Type entityType) {
            _tables.Add(new SimpleTableProvider(this, entityType));
        }

        public SimpleDataModelProvider(ICustomTypeDescriptor typeDescriptor) {
            _tables.Add(new SimpleTableProvider(this, typeDescriptor));
        }

        public override ReadOnlyCollection<TableProvider> Tables {
            get {
                return _tables.AsReadOnly();
            }
        }

        public override object CreateContext() {
            throw new NotSupportedException();
        }
    }
}
