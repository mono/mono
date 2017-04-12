//------------------------------------------------------------------------------
// <copyright file="RelatedView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;

    internal sealed class RelatedView : DataView, IFilter {
        private readonly Nullable<DataKey> parentKey;  
        private readonly DataKey childKey;
        private readonly DataRowView parentRowView;
        private readonly object[] filterValues;

        public RelatedView(DataColumn[] columns, object[] values)
            : base(columns[0].Table, false) {
            if (values == null) {
                throw ExceptionBuilder.ArgumentNull("values");
            }
            this.parentRowView = null;
            this.parentKey = null;
            this.childKey = new DataKey(columns, true);
            this.filterValues = values;
            Debug.Assert(this.Table == childKey.Table, "Key.Table Must be equal to Current Table");
            base.ResetRowViewCache();
        }


        public RelatedView(DataRowView parentRowView, DataKey parentKey, DataColumn[] childKeyColumns) : base(childKeyColumns[0].Table, false) {
            this.filterValues = null;
            this.parentRowView = parentRowView;
            this.parentKey = parentKey;
            this.childKey = new DataKey(childKeyColumns, true);
            Debug.Assert (this.Table == childKey.Table, "Key.Table Must be equal to Current Table");
            base.ResetRowViewCache();
        }

        private object[] GetParentValues()
        {
            if (filterValues != null) {
                return filterValues;
            }
          
            if (!parentRowView.HasRecord()) {
                return null;
            }
            return parentKey.Value.GetKeyValues(parentRowView.GetRecord());
        }


        public bool Invoke(DataRow row, DataRowVersion version) {
            object[] parentValues = GetParentValues();
            if (parentValues == null) {
                return false;
            }

            object[] childValues = row.GetKeyValues(childKey, version);
#if false
            for (int i = 0; i < keyValues.Length; i++) {
                Debug.WriteLine("keyvalues[" + (i).ToString() + "] = " + Convert.ToString(keyValues[i]));
            }
            for (int i = 0; i < values.Length; i++) {
                Debug.WriteLine("values[" + (i).ToString() + "] = " + Convert.ToString(values[i]));
            }
#endif
            bool allow = true;
            if (childValues.Length != parentValues.Length) {
                allow = false;
            }
            else {
                for (int i = 0; i < childValues.Length; i++) {
                    if (!childValues[i].Equals(parentValues[i])) {
                        allow = false;
                        break;
                    }
                }
            }

            IFilter baseFilter = base.GetFilter();
            if (baseFilter != null) {
                allow &= baseFilter.Invoke(row, version);
            }

            return allow;
        }

        internal override IFilter GetFilter() {
            return this;
        }

        // move to OnModeChanged
        public override DataRowView AddNew() {
            DataRowView addNewRowView = base.AddNew();
            addNewRowView.Row.SetKeyValues(childKey, GetParentValues());
            return addNewRowView;
        }

        internal override void SetIndex(string newSort, DataViewRowState newRowStates, IFilter newRowFilter) {
            SetIndex2(newSort, newRowStates, newRowFilter, false);
            Reset();
        }

        public override bool Equals( DataView dv) {
            RelatedView other = dv as RelatedView;
            if (other == null) {
                return false;
            }
            if (!base.Equals(dv)) {
                return false;
            }
            if (filterValues != null) {
                return (CompareArray(this.childKey.ColumnsReference, other.childKey.ColumnsReference) && CompareArray(this.filterValues, other.filterValues));
            }
            else {
                if (other.filterValues != null)
                    return false;
                return (CompareArray(this.childKey.ColumnsReference, other.childKey.ColumnsReference) &&
                        CompareArray(this.parentKey.Value.ColumnsReference, this.parentKey.Value.ColumnsReference) &&
                        parentRowView.Equals(other.parentRowView));
            }
        }

        private bool CompareArray(object[] value1, object[] value2) {
            if (value1 == null || value2 == null) {
                return value1 == value2;
            }
            if (value1.Length != value2.Length) {
                return false;
            }
            for(int i = 0; i < value1.Length; i++) {
                if (value1[i] != value2[i])
                    return false;
            }
            return true;
        }
    }
}
