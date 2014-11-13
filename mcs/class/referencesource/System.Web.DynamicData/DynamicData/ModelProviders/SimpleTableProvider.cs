namespace System.Web.DynamicData.ModelProviders {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    internal sealed class SimpleTableProvider : TableProvider {
        private List<ColumnProvider> _columns;
        private ICustomTypeDescriptor _descriptor;

        public SimpleTableProvider(DataModelProvider modelProvider, Type entityType)
            : base(modelProvider) {

            if (entityType == null) {
                throw new ArgumentNullException("entityType");
            }

            EntityType = entityType;
            Name = entityType.Name;
            DataContextPropertyName = String.Empty;
            InitializeColumns(TypeDescriptor.GetProperties(entityType));
        }

        public SimpleTableProvider(DataModelProvider modelProvider, ICustomTypeDescriptor descriptor)
            : base(modelProvider) {

            if (descriptor == null) {
                throw new ArgumentNullException("descriptor");
            }

            _descriptor = descriptor;
            Name = descriptor.GetClassName();
            DataContextPropertyName = String.Empty;
            InitializeColumns(descriptor.GetProperties());
        }

        public override ReadOnlyCollection<ColumnProvider> Columns {
            get {
                return _columns.AsReadOnly();
            }
        }

        public override ICustomTypeDescriptor GetTypeDescriptor() {
            return _descriptor ?? base.GetTypeDescriptor();
        }

        public override IQueryable GetQuery(object context) {
            throw new NotSupportedException();
        }

        private void InitializeColumns(PropertyDescriptorCollection columnDescriptors) {
            _columns = columnDescriptors.OfType<PropertyDescriptor>().Select(p => new SimpleColumnProvider(this, p)).OfType<ColumnProvider>().ToList();
        }
    }
}
