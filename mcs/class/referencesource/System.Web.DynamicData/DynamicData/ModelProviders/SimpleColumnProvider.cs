namespace System.Web.DynamicData.ModelProviders {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Resources;

    internal sealed class SimpleColumnProvider : ColumnProvider {
        public SimpleColumnProvider(TableProvider tableProvider, PropertyDescriptor propertyDescriptor)
            : base(tableProvider) {
            if (propertyDescriptor.PropertyType == null) {
                throw new ArgumentNullException(DynamicDataResources.SimpleColumnProvider_ColumnTypeRequired);
            }
            Name = propertyDescriptor.Name;
            ColumnType = propertyDescriptor.PropertyType;
            IsPrimaryKey = propertyDescriptor.Attributes.OfType<KeyAttribute>().Any();
            Nullable = Misc.TypeAllowsNull(ColumnType);
            IsReadOnly = propertyDescriptor.IsReadOnly;
            IsSortable = true;
        }

        public override AttributeCollection Attributes {
            get {
                if (!System.Web.UI.DataBinder.IsBindableType(ColumnType)) {
                    return AttributeCollection.FromExisting(base.Attributes, new ScaffoldColumnAttribute(false));
                }
                return base.Attributes;
            }
        }
    }
}
