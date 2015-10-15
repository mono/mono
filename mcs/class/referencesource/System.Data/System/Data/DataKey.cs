//------------------------------------------------------------------------------
// <copyright file="DataKey.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;
    using System.ComponentModel;

    internal struct DataKey {
        private const int maxColumns = 32;

        private readonly DataColumn[] columns;

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        internal DataKey(DataColumn[] columns, bool copyColumns) {
            if (columns == null)
                throw ExceptionBuilder.ArgumentNull("columns");

            if (columns.Length == 0)
                throw ExceptionBuilder.KeyNoColumns();

            if (columns.Length > maxColumns)
                throw ExceptionBuilder.KeyTooManyColumns(maxColumns);

            for (int i = 0; i < columns.Length; i++) {
                if (columns[i] == null)
                    throw ExceptionBuilder.ArgumentNull("column");
            }

            for (int i = 0; i < columns.Length; i++) {
                for (int j = 0; j < i; j++) {
                    if (columns[i] == columns[j]) {
                        throw ExceptionBuilder.KeyDuplicateColumns(columns[i].ColumnName);
                    }
                }
            }

            if (copyColumns) {
                // Need to make a copy of all columns
                this.columns = new DataColumn [columns.Length];
                for (int i = 0; i < columns.Length; i++)
                    this.columns[i] = columns[i];
            }
            else {
                // take ownership of the array passed in
                this.columns = columns;
            }
            CheckState();
        }

        internal DataColumn[] ColumnsReference {
            get {
                return columns;
            }
        }

        internal bool HasValue {
            get {
                return (null != columns);
            }
        }

        internal DataTable Table {
            get {
                return columns[0].Table;
            }
        }

        internal void CheckState() {
            DataTable table = columns[0].Table;

            if (table == null) {
                throw ExceptionBuilder.ColumnNotInAnyTable();
            }

            for (int i = 1; i < columns.Length; i++) {
                if (columns[i].Table == null) {
                    throw ExceptionBuilder.ColumnNotInAnyTable();
                }
                if (columns[i].Table != table) {
                    throw ExceptionBuilder.KeyTableMismatch();
                }
            }
        }

        //check to see if this.columns && key2's columns are equal regardless of order
        internal bool ColumnsEqual(DataKey key) {
            return ColumnsEqual(this.columns, ((DataKey)key).columns);
        }

        //check to see if columns1 && columns2 are equal regardless of order
        internal static bool ColumnsEqual(DataColumn[] column1, DataColumn[] column2) {

            if (column1 == column2) {
                return true;
            } else if (column1 == null || column2 == null) {
                return false;
            } else if (column1.Length != column2.Length) {
                return false;
            } else {
                int i, j;
                for (i=0; i<column1.Length; i++) {
                    bool check = false;
                    for (j=0; j<column2.Length; j++) {
                        if (column1[i].Equals(column2[j])) {
                            check = true;
                            break;
                        }
                    }
                    if (!check) {
                        return false;
                    }
                }
            }
            return true;
        }

        internal bool ContainsColumn(DataColumn column) {
            for (int i = 0; i < columns.Length; i++) {
                if (column == columns[i]) {
                    return true;
                }
            }
            return false;
        }

        public override int GetHashCode() {
            Debug.Assert(false, "don't put DataKey into a Hashtable");
            return base.GetHashCode();
        }

        public static bool operator==(DataKey x, DataKey y) {
            return x.Equals((object)y);
        }

        public static bool operator!=(DataKey x, DataKey y) {
            return !x.Equals((object)y);
        }

        public override bool Equals(object value) {
            Debug.Assert(false, "need to directly call Equals(DataKey)");
            return Equals((DataKey)value);
        }

        internal bool Equals(DataKey value) {
            //check to see if this.columns && key2's columns are equal...
            DataColumn[] column1=this.columns;
            DataColumn[] column2=value.columns;

            if (column1 == column2) {
                return true;
            }
            else if (column1 == null || column2 == null) {
                return false;
            }
            else if (column1.Length != column2.Length) {
                return false;
            }
            else {
                for (int i = 0; i <column1.Length; i++) {
                    if (!column1[i].Equals(column2[i])) {
                        return false;
                    }
                }
                return true;
            }
        }

        internal string[] GetColumnNames() {
            string[] values = new string[columns.Length];
            for(int i = 0; i < columns.Length; ++i) {
                values[i] = columns[i].ColumnName;
            }
            return values;
        }

        internal IndexField[] GetIndexDesc() {
            IndexField[] indexDesc = new IndexField[columns.Length];
            for (int i = 0; i < columns.Length; i++) {
                indexDesc[i] = new IndexField(columns[i], false);
            }
            return indexDesc;
        }

        internal object[] GetKeyValues(int record) {
            object[] values = new object[columns.Length];
            for (int i = 0; i < columns.Length; i++) {
                values[i] = columns[i][record];
            }
            return values;
        }

        internal Index GetSortIndex() {
            return GetSortIndex(DataViewRowState.CurrentRows);
        }

        internal Index GetSortIndex(DataViewRowState recordStates) {
            IndexField[] indexDesc = GetIndexDesc();
            return columns[0].Table.GetIndex(indexDesc, recordStates, (IFilter)null);
        }

        internal bool RecordsEqual(int record1, int record2) {
            for (int i = 0; i < columns.Length; i++) {
                if (columns[i].Compare(record1, record2) != 0) {
                    return false;
                }
            }
            return true;
        }

        internal DataColumn[] ToArray() {
            DataColumn[] values = new DataColumn[columns.Length];
            for(int i = 0; i < columns.Length; ++i) {
                values[i] = columns[i];
            }
            return values;
        }
    }
}
