//------------------------------------------------------------------------------
// <copyright file="DataTableMappingCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;

    [
    Editor("Microsoft.VSDesigner.Data.Design.DataTableMappingCollectionEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
    ListBindable(false)
    ]
    public sealed class DataTableMappingCollection : MarshalByRefObject, ITableMappingCollection {
        private List<DataTableMapping> items; // delay creation until AddWithoutEvents, Insert, CopyTo, GetEnumerator

        public DataTableMappingCollection() {
        }

        // explicit ICollection implementation
        bool System.Collections.ICollection.IsSynchronized {
            get { return false;}
        }
        object System.Collections.ICollection.SyncRoot {
            get { return this;}
        }

        // explicit IList implementation
        bool System.Collections.IList.IsReadOnly {
            get { return false;}
        }
         bool System.Collections.IList.IsFixedSize {
            get { return false;}
        }
        object System.Collections.IList.this[int index] {
            get {
                return this[index];
            }
            set {
                ValidateType(value);
                this[index] = (DataTableMapping) value;
            }
        }

        object ITableMappingCollection.this[string index] {
            get {
                return this[index];
            }
            set {
                ValidateType(value);
                this[index] = (DataTableMapping) value;
            }
        }
        ITableMapping ITableMappingCollection.Add(string sourceTableName, string dataSetTableName) {
            return Add(sourceTableName, dataSetTableName);
        }
        ITableMapping ITableMappingCollection.GetByDataSetTable(string dataSetTableName) {
            return GetByDataSetTable(dataSetTableName);
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResDescriptionAttribute(Res.DataTableMappings_Count),
        ]
        public int Count {
            get {
                return ((null != items) ? items.Count : 0);
            }
        }

        private Type ItemType {
            get { return typeof(DataTableMapping); }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResDescriptionAttribute(Res.DataTableMappings_Item),
        ]
        public DataTableMapping this[int index] {
            get {
                RangeCheck(index);
                return items[index];
            }
            set {
                RangeCheck(index);
                Replace(index, value);
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ResDescriptionAttribute(Res.DataTableMappings_Item),
        ]
        public DataTableMapping this[string sourceTable] {
            get {
                int index = RangeCheck(sourceTable);
                return items[index];
            }
            set {
                int index = RangeCheck(sourceTable);
                Replace(index, value);
            }
        }

        public int Add(object value) {
            ValidateType(value);
            Add((DataTableMapping) value);
            return Count-1;
        }

        private DataTableMapping Add(DataTableMapping value) {
            AddWithoutEvents(value);
            return value;
        }

        public void AddRange(DataTableMapping[] values) { // V1.0.3300
            AddEnumerableRange(values, false);
        }

        public void AddRange(System.Array values) { // V1.2.3300
            AddEnumerableRange(values, false);
        }

        private void AddEnumerableRange(IEnumerable values, bool doClone) {
            if (null == values) {
                throw ADP.ArgumentNull("values");
            }
            foreach(object value in values) {
                ValidateType(value);
            }
            if (doClone) {
                foreach(ICloneable value in values) {
                    AddWithoutEvents(value.Clone() as DataTableMapping);
                }
            }
            else {
                foreach(DataTableMapping value in values) {
                    AddWithoutEvents(value);
                }
            }
        }

        /*/// <include file='doc\DataTableMappingCollection.uex' path='docs/doc[@for="DataTableMappingCollection.AddCloneOfRange"]/*' />
        public void AddCloneOfRange(IEnumerable values) {
            AddEnumerableRange(values, true);
        }*/

        public DataTableMapping Add(string sourceTable, string dataSetTable) {
            return Add(new DataTableMapping(sourceTable, dataSetTable));
        }

        private void AddWithoutEvents(DataTableMapping value) {
            Validate(-1, value);
            value.Parent = this;
            ArrayList().Add(value);
        }

        // implemented as a method, not as a property because the VS7 debugger
        // object browser calls properties to display their value, and we want this delayed
        private List<DataTableMapping> ArrayList() {
            if (null == this.items) {
                this.items = new List<DataTableMapping>();
            }
            return this.items;
        }

        public void Clear() {
            if (0 < Count) {
                ClearWithoutEvents();
            }
        }

        private void ClearWithoutEvents() {
            if (null != items) {
                foreach(DataTableMapping item in items) {
                    item.Parent = null;
                }
                items.Clear();
            }
        }

        public bool Contains(string value) {
            return (-1 != IndexOf(value));
        }

        public bool Contains(object value) {
            return (-1 != IndexOf(value));
        }

        public void CopyTo(Array array, int index) {
            ((ICollection)ArrayList()).CopyTo(array, index);
        }

        public void CopyTo(DataTableMapping[] array, int index) {
            ArrayList().CopyTo(array, index);
        }

        public DataTableMapping GetByDataSetTable(string dataSetTable) {
            int index = IndexOfDataSetTable(dataSetTable);
            if (0 > index) {
                throw ADP.TablesDataSetTable(dataSetTable);
            }
            return items[index];
        }

        public IEnumerator GetEnumerator() {
            return ArrayList().GetEnumerator();
        }

        public int IndexOf(object value) {
            if (null != value) {
                ValidateType(value);
                for (int i = 0; i < Count; ++i) {
                    if (items[i] == value) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public int IndexOf(string sourceTable) {
            if (!ADP.IsEmpty(sourceTable)) {
                for (int i = 0; i < Count; ++i) {
                    string value = items[i].SourceTable;
                    if ((null != value) && (0 == ADP.SrcCompare(sourceTable, value))) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public int IndexOfDataSetTable(string dataSetTable) {
            if (!ADP.IsEmpty(dataSetTable)) {
                for (int i = 0; i < Count; ++i) {
                    string value = items[i].DataSetTable;
                    if ((null != value) && (0 == ADP.DstCompare(dataSetTable, value))) {
                        return i;
                    }
                }
            }
            return -1;
        }

        public void Insert(int index, Object value) {
            ValidateType(value);
            Insert(index, (DataTableMapping) value);
        }

        public void Insert(int index, DataTableMapping value) {
            if (null == value) {
                throw ADP.TablesAddNullAttempt("value");
            }
            Validate(-1, value);
            value.Parent = this;
            ArrayList().Insert(index, value);
        }

        private void RangeCheck(int index) {
            if ((index < 0) || (Count <= index)) {
                throw ADP.TablesIndexInt32(index, this);
            }
        }

        private int RangeCheck(string sourceTable) {
            int index = IndexOf(sourceTable);
            if (index < 0) {
                throw ADP.TablesSourceIndex(sourceTable);
            }
            return index;
        }

        public void RemoveAt(int index) {
            RangeCheck(index);
            RemoveIndex(index);
        }

        public void RemoveAt(string sourceTable) {
            int index = RangeCheck(sourceTable);
            RemoveIndex(index);
        }

        private void RemoveIndex(int index) {
            Debug.Assert((null != items) && (0 <= index) && (index < Count), "RemoveIndex, invalid");
            items[index].Parent = null;
            items.RemoveAt(index);
        }

        public void Remove(object value) {
            ValidateType(value);
            Remove((DataTableMapping) value);
        }

        public void Remove (DataTableMapping value) {
            if (null == value) {
                throw ADP.TablesAddNullAttempt ("value");
            }
            int index = IndexOf (value);

            if (-1 != index) {
                RemoveIndex (index);
            }
            else {
                throw ADP.CollectionRemoveInvalidObject (ItemType, this);
            }
        }

        private void Replace (int index, DataTableMapping newValue) {
            Validate(index, newValue);
            items[index].Parent = null;
            newValue.Parent = this;
            items[index] = newValue;
        }

        private void ValidateType(object value) {
            if (null == value) {
                throw ADP.TablesAddNullAttempt("value");
            }
            else if (!ItemType.IsInstanceOfType(value)) {
                throw ADP.NotADataTableMapping(value);
            }
        }

        private void Validate(int index, DataTableMapping value) {
            if (null == value) {
                throw ADP.TablesAddNullAttempt("value");
            }
            if (null != value.Parent) {
                if (this != value.Parent) {
                    throw ADP.TablesIsNotParent(this);
                }
                else if (index != IndexOf(value)) {
                    throw ADP.TablesIsParent(this);
                }
            }
            String name = value.SourceTable;
            if (ADP.IsEmpty(name)) {
                index = 1;
                do {
                    name = ADP.SourceTable + index.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    index++;
                } while (-1 != IndexOf(name));
                value.SourceTable = name;
            }
            else {
                ValidateSourceTable(index, name);
            }
        }

        internal void ValidateSourceTable(int index, string value) {
            int pindex = IndexOf(value);
            if ((-1 != pindex) && (index != pindex)) { // must be non-null and unique
                throw ADP.TablesUniqueSourceTable(value);
            }
        }

        [ EditorBrowsableAttribute(EditorBrowsableState.Advanced) ] // MDAC 69508
        static public DataTableMapping GetTableMappingBySchemaAction(DataTableMappingCollection tableMappings, string sourceTable, string dataSetTable, MissingMappingAction mappingAction) {
            if (null != tableMappings) {
                int index = tableMappings.IndexOf(sourceTable);
                if (-1 != index) {
#if DEBUG
                    if (AdapterSwitches.DataSchema.TraceWarning) {
                        Debug.WriteLine("mapping match on SourceTable \"" + sourceTable + "\"");
                    }
#endif
                    return tableMappings.items[index];
                }
            }
            if (ADP.IsEmpty(sourceTable)) {
                throw ADP.InvalidSourceTable("sourceTable");
            }
            switch (mappingAction) {
                case MissingMappingAction.Passthrough:
#if DEBUG
                    if (AdapterSwitches.DataSchema.TraceInfo) {
                        Debug.WriteLine("mapping passthrough of SourceTable \"" + sourceTable + "\" -> \"" + dataSetTable + "\"");
                    }
#endif
                    return new DataTableMapping(sourceTable, dataSetTable);

                case MissingMappingAction.Ignore:
#if DEBUG
                    if (AdapterSwitches.DataSchema.TraceWarning) {
                        Debug.WriteLine("mapping filter of SourceTable \"" + sourceTable + "\"");
                    }
#endif
                    return null;

                case MissingMappingAction.Error:
#if DEBUG
                    if (AdapterSwitches.DataSchema.TraceError) {
                        Debug.WriteLine("mapping error on SourceTable \"" + sourceTable + "\"");
                    }
#endif
                    throw ADP.MissingTableMapping(sourceTable);

                default:
                    throw ADP.InvalidMissingMappingAction(mappingAction);
            }
        }
    }
}
