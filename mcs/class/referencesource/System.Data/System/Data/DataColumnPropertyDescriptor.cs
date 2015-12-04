//------------------------------------------------------------------------------
// <copyright file="DataColumnPropertyDescriptor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Data.Common;

    internal sealed class DataColumnPropertyDescriptor : PropertyDescriptor {

        DataColumn column;

        internal DataColumnPropertyDescriptor(DataColumn dataColumn) : base(dataColumn.ColumnName, null) {
            this.column = dataColumn;    
        }

        public override AttributeCollection Attributes {
            get {
                if (typeof(System.Collections.IList).IsAssignableFrom(this.PropertyType)) {
                    Attribute[] attrs = new Attribute[base.Attributes.Count + 1];
                    base.Attributes.CopyTo(attrs, 0);
                    // we don't want to show the columns which are of type IList in the designer
                    attrs[attrs.Length - 1] = new ListBindableAttribute(false);
                    return new AttributeCollection(attrs);
                } else {
                    return base.Attributes;
                }
            }
        }

        internal DataColumn Column {
            get {
                return column;
            }
        }

        public override Type ComponentType {
            get {
                return typeof(DataRowView);
            }
        }

        public override bool IsReadOnly {
            get {
                return column.ReadOnly;
            }
        }

        public override Type PropertyType {
            get {
                return column.DataType;
            }
        }

        public override bool Equals(object other) {
            if (other is DataColumnPropertyDescriptor) {
                DataColumnPropertyDescriptor descriptor = (DataColumnPropertyDescriptor) other;
                return(descriptor.Column == Column);
            }
            return false;
        }

        public override Int32 GetHashCode() {
            return Column.GetHashCode();
        }

        public override bool CanResetValue(object component) {
            DataRowView dataRowView = (DataRowView) component;
            if (!column.IsSqlType)
            	return (dataRowView.GetColumnValue(column) != DBNull.Value);
            return (!DataStorage.IsObjectNull(dataRowView.GetColumnValue(column)));
        }

        public override object GetValue(object component) {
            DataRowView dataRowView = (DataRowView) component;
            return dataRowView.GetColumnValue(column);
        }

        public override void ResetValue(object component) {
            DataRowView dataRowView = (DataRowView) component;
            dataRowView.SetColumnValue(column, DBNull.Value);// no need to ccheck for the col type and set Sql...Null! 
        }

        public override void SetValue(object component, object value) {
            DataRowView dataRowView = (DataRowView) component;
            dataRowView.SetColumnValue(column, value);
            OnValueChanged(component, EventArgs.Empty);
        }

        public override bool ShouldSerializeValue(object component) {
            return false;
        }

		public override bool IsBrowsable {
			get {
				return (column.ColumnMapping == System.Data.MappingType.Hidden ? false : base.IsBrowsable);
			}
		}
    }   
}
