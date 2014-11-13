//------------------------------------------------------------------------------
// <copyright file="DataRowCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;

    public sealed class DataRowCollection : InternalDataCollectionBase {

        private sealed class DataRowTree : RBTree<DataRow> {
            internal DataRowTree() : base(TreeAccessMethod.INDEX_ONLY) {
            }
            
            protected override int CompareNode (DataRow record1, DataRow record2) {
                throw ExceptionBuilder.InternalRBTreeError(RBTreeError.CompareNodeInDataRowTree);
            }
            protected override int CompareSateliteTreeNode (DataRow record1, DataRow record2) {
                throw ExceptionBuilder.InternalRBTreeError(RBTreeError.CompareSateliteTreeNodeInDataRowTree);
            }
        }

        private readonly DataTable table;
        private readonly DataRowTree list = new DataRowTree();
        internal int nullInList = 0;

        /// <devdoc>
        /// Creates the DataRowCollection for the given table.
        /// </devdoc>
        internal DataRowCollection(DataTable table) {
            this.table = table;
        }

        public override int Count {
            get {
                return list.Count;
            }
        }

        /// <devdoc>
        ///    <para>Gets the row at the specified index.</para>
        /// </devdoc>
        public DataRow this[int index] {
            get {
                return list[index];
            }
        }

        /// <devdoc>
        /// <para>Adds the specified <see cref='System.Data.DataRow'/> to the <see cref='System.Data.DataRowCollection'/> object.</para>
        /// </devdoc>
        public void Add(DataRow row) {
            table.AddRow(row, -1);
        }
        
        public void InsertAt(DataRow row, int pos) {
            if (pos < 0)
                throw ExceptionBuilder.RowInsertOutOfRange(pos);
            if (pos >= list.Count)
                table.AddRow(row, -1);
            else
                table.InsertRow(row, -1, pos);
        }

        internal void DiffInsertAt(DataRow row, int pos) {
            if ((pos < 0) || (pos == list.Count)) {
                table.AddRow(row, pos >-1? pos+1 : -1);
                return;
            }
            if (table.NestedParentRelations.Length > 0) { // get in this trouble only if  table has a nested parent 
            // get into trouble if table has JUST a nested parent? how about multi parent!
                if (pos < list.Count) {
                    if (list[pos] != null) {
                        throw ExceptionBuilder.RowInsertTwice(pos, table.TableName);
                    }
                    list.RemoveAt(pos);
                    nullInList--;
                    table.InsertRow(row, pos+1, pos);
                }
                else {
                    while (pos>list.Count) {
                        list.Add(null);
                        nullInList++;
                    }
                    table.AddRow(row, pos+1);
                }
            }
            else {
                table.InsertRow(row, pos+1, pos > list.Count ? -1 : pos);
            }
        }

        public Int32 IndexOf(DataRow row) {
            if ((null == row) || (row.Table != this.table) || ((0 == row.RBTreeNodeId) && (row.RowState == DataRowState.Detached))) //Webdata 102857
                return -1;
            return list.IndexOf(row.RBTreeNodeId, row);
        }

        /// <devdoc>
        ///    <para>Creates a row using specified values and adds it to the
        ///    <see cref='System.Data.DataRowCollection'/>.</para>
        /// </devdoc>
        internal DataRow AddWithColumnEvents(params object[] values) {
            DataRow row = table.NewRow(-1);
            row.ItemArray = values;
            table.AddRow(row, -1);
            return row;
        }
        
        public DataRow Add(params object[] values) {
            int record = table.NewRecordFromArray(values);
            DataRow row = table.NewRow(record);
            table.AddRow(row, -1);
            return row;
        }

        internal void ArrayAdd(DataRow row) {
            row.RBTreeNodeId = list.Add(row);
        }

        internal void ArrayInsert(DataRow row, int pos) {
            row.RBTreeNodeId = list.Insert(pos, row);
        }

        internal void ArrayClear() {
            list.Clear();
        }

        internal void ArrayRemove(DataRow row) {
            if (row.RBTreeNodeId == 0) {
                throw ExceptionBuilder.InternalRBTreeError(RBTreeError.AttachedNodeWithZerorbTreeNodeId);
            }
            list.RBDelete(row.RBTreeNodeId);
            row.RBTreeNodeId = 0;
        }
        
        /// <devdoc>
        ///    <para>Gets
        ///       the row specified by the primary key value.
        ///       </para>
        /// </devdoc>
        public DataRow Find(object key) {
            return table.FindByPrimaryKey(key);
        }

        /// <devdoc>
        ///    <para>Gets the row containing the specified primary key values.</para>
        /// </devdoc>
        public DataRow Find(object[] keys) {
            return table.FindByPrimaryKey(keys);
        }

        /// <devdoc>
        ///    <para>Clears the collection of all rows.</para>
        /// </devdoc>
        public void Clear() {
            table.Clear(false);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether the primary key of any row in the
        ///       collection contains the specified value.
        ///    </para>
        /// </devdoc>
        public bool Contains(object key) {
            return(table.FindByPrimaryKey(key) != null);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating if the <see cref='System.Data.DataRow'/> with
        ///       the specified primary key values exists.
        ///    </para>
        /// </devdoc>
        public bool Contains(object[] keys) {
            return(table.FindByPrimaryKey(keys) != null);
        }
        
        public override void CopyTo(Array ar, int index) {
            list.CopyTo(ar, index);
        }

        public void CopyTo(DataRow[] array, int index) {
            list.CopyTo(array, index);
        }
        
        public override IEnumerator GetEnumerator() {
            return list.GetEnumerator();
        }

        /// <devdoc>
        /// <para>Removes the specified <see cref='System.Data.DataRow'/> from the collection.</para>
        /// </devdoc>
        public void Remove(DataRow row) {
            if ((null == row) || (row.Table != table) || (-1 == row.rowID)) {
                throw ExceptionBuilder.RowOutOfRange();
            }

            if ((row.RowState != DataRowState.Deleted) && (row.RowState != DataRowState.Detached))
                row.Delete();

            if (row.RowState != DataRowState.Detached)
                row.AcceptChanges();
        }

        /// <devdoc>
        ///    <para>
        ///       Removes the row with the specified index from
        ///       the collection.
        ///    </para>
        /// </devdoc>
        public void RemoveAt(int index) {
                Remove(this[index]);
        }
    }
}
