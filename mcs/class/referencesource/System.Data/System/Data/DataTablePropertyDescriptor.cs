//------------------------------------------------------------------------------
// <copyright file="DataTablePropertyDescriptor.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.ComponentModel;

    internal sealed class DataTablePropertyDescriptor : PropertyDescriptor {

        DataTable table;

        public DataTable Table {
            get {
                return table;
            }
        }

        internal DataTablePropertyDescriptor(DataTable dataTable) : base(dataTable.TableName, null) {
            this.table = dataTable;    
        }

        public override Type ComponentType {
            get {
                return typeof(DataRowView);
            }
        }

        public override bool IsReadOnly {
            get {
                return false;
            }
        }

        public override Type PropertyType {
            get {
                return typeof(IBindingList);
            }
        }

        public override bool Equals(object other) {
            if (other is DataTablePropertyDescriptor) {
                DataTablePropertyDescriptor descriptor = (DataTablePropertyDescriptor) other;
                return(descriptor.Table == Table);
            }
            return false;
        }

        public override Int32 GetHashCode() {
            return Table.GetHashCode();
        }

        public override bool CanResetValue(object component) {
            return false;
        }

        public override object GetValue(object component) {
            DataViewManagerListItemTypeDescriptor dataViewManagerListItem = (DataViewManagerListItemTypeDescriptor) component;
            return dataViewManagerListItem.GetDataView(table);
        }

        public override void ResetValue(object component) {
        }

        public override void SetValue(object component, object value) {
        }

        public override bool ShouldSerializeValue(object component) {
            return false;
        }
    }   
}

