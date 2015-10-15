//------------------------------------------------------------------------------
// <copyright file="Selection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Threading;

    internal struct IndexField {
        public readonly DataColumn Column;
        public readonly bool IsDescending; // false = Asc; true = Desc what is default value for this?

        internal IndexField(DataColumn column, bool isDescending) {
            Debug.Assert(column != null, "null column");

            Column = column;
            IsDescending = isDescending;
        }

        public static bool operator == (IndexField if1, IndexField if2) {
            return if1.Column == if2.Column && if1.IsDescending == if2.IsDescending;
        }

        public static bool operator !=(IndexField if1, IndexField if2) {
            return !(if1 == if2);
        }

        // must override Equals if == operator is defined
        public override bool Equals(object obj) {
            if (obj is IndexField)
                return this == (IndexField)obj;
            else
                return false;
        }

        // must override GetHashCode if Equals is redefined
        public override int GetHashCode() {
            return Column.GetHashCode() ^ IsDescending.GetHashCode();
        }
    }

    internal sealed class Index {

        private sealed class IndexTree : RBTree<int> {
            private readonly Index _index;

            internal IndexTree(Index index) : base(TreeAccessMethod.KEY_SEARCH_AND_INDEX) {
                _index = index;
            }

            protected override int CompareNode (int record1, int record2) {
                return _index.CompareRecords(record1, record2);
            }
            protected override int CompareSateliteTreeNode (int record1, int record2) {
                return _index.CompareDuplicateRecords(record1, record2);
            }
        }

        // these constants are used to update a DataRow when the record and Row are known, but don't match
        private const int DoNotReplaceCompareRecord  = 0;
        private const int ReplaceNewRecordForCompare = 1;
        private const int ReplaceOldRecordForCompare = 2;

        private readonly DataTable table;
        internal readonly IndexField[] IndexFields;

        /// <summary>Allow a user implemented comparision of two DataRow</summary>
        /// <remarks>User must use correct DataRowVersion in comparison or index corruption will happen</remarks>
        private readonly System.Comparison<DataRow> _comparison;

        private readonly DataViewRowState recordStates;
        private WeakReference rowFilter;
        private IndexTree records;
        private int recordCount;
        private int refCount;

        private Listeners<DataViewListener> _listeners;

        private bool suspendEvents;

        private readonly static object[] zeroObjects = new object[0];
        private readonly bool isSharable;
        private readonly bool _hasRemoteAggregate;

        internal const Int32 MaskBits     = unchecked((int)0x7FFFFFFF);

        private static int _objectTypeCount; // Bid counter
        private readonly int _objectID = System.Threading.Interlocked.Increment(ref _objectTypeCount);

        public Index(DataTable table, IndexField[] indexFields, DataViewRowState recordStates, IFilter rowFilter)
            : this(table, indexFields, null, recordStates, rowFilter) { }

        public Index(DataTable table, System.Comparison<DataRow> comparison, DataViewRowState recordStates, IFilter rowFilter)
            : this(table, GetAllFields(table.Columns), comparison, recordStates, rowFilter) { }

        // for the delegate methods, we don't know what the dependent columns are - so all columns are dependent
        private static IndexField[] GetAllFields(DataColumnCollection columns) {
            IndexField[] fields = new IndexField[columns.Count];
            for(int i = 0; i < fields.Length; ++i) {
                fields[i] = new IndexField(columns[i], false);
            }
            return fields;
        }

        private Index(DataTable table, IndexField[] indexFields, System.Comparison<DataRow> comparison, DataViewRowState recordStates, IFilter rowFilter) {
            Bid.Trace("<ds.Index.Index|API> %d#, table=%d, recordStates=%d{ds.DataViewRowState}\n",
                            ObjectID, (table != null) ? table.ObjectID : 0, (int)recordStates);
            Debug.Assert(indexFields != null);
            Debug.Assert(null != table, "null table");
            if ((recordStates &
                 (~(DataViewRowState.CurrentRows | DataViewRowState.OriginalRows))) != 0) {
                throw ExceptionBuilder.RecordStateRange();
            }
            this.table = table;
            _listeners = new Listeners<DataViewListener>(ObjectID,
                delegate(DataViewListener listener)
                {
                    return (null != listener); 
                });

            IndexFields = indexFields;
            this.recordStates = recordStates;
            _comparison = comparison;

            DataColumnCollection columns = table.Columns;
            isSharable = (rowFilter == null) && (comparison == null); // a filter or comparison make an index unsharable
            if (null != rowFilter) {
                this.rowFilter = new WeakReference(rowFilter);
                DataExpression expr = (rowFilter as DataExpression);
                if (null != expr) {
                    _hasRemoteAggregate = expr.HasRemoteAggregate();
                }
            }
            InitRecords(rowFilter);

            // do not AddRef in ctor, every caller should be responsible to AddRef it
            // if caller does not AddRef, it is expected to be a one-time read operation because the index won't be maintained on writes
        }

        public bool Equal(IndexField[] indexDesc, DataViewRowState recordStates, IFilter rowFilter) {
            if (
                !isSharable ||
                IndexFields.Length != indexDesc.Length ||
                this.recordStates     != recordStates     ||
                null                  != rowFilter
            ) {
                return false;
            }

            for (int loop = 0; loop < IndexFields.Length; loop++) {
                if (IndexFields[loop].Column!= indexDesc[loop].Column ||
                    IndexFields[loop].IsDescending != indexDesc[loop].IsDescending) {
                    return false;
                }
            }

            return true;
        }

        internal bool HasRemoteAggregate {
            get {
                return _hasRemoteAggregate;
            }
        }

        internal int ObjectID {
            get {
                return _objectID;
            }
        }

        public DataViewRowState RecordStates {
            get { return recordStates; }
        }

        public IFilter RowFilter {
            get { return (IFilter)((null != rowFilter) ? rowFilter.Target : null); }
        }

        public int GetRecord(int recordIndex) {
            Debug.Assert (recordIndex >= 0 && recordIndex < recordCount, "recordIndex out of range");
            return records[recordIndex];
        }

        public bool HasDuplicates {
            get {
                return records.HasDuplicates;
            }
        }

        public int RecordCount {
            get {
                return recordCount;
            }
        }

        public bool IsSharable {
            get {
                return isSharable;
            }
        }


        private bool AcceptRecord(int record) {
            return AcceptRecord(record, RowFilter);
        }

        private bool AcceptRecord(int record, IFilter filter) {
            Bid.Trace("<ds.Index.AcceptRecord|API> %d#, record=%d\n", ObjectID, record);
            if (filter == null)
                return true;

            DataRow row = table.recordManager[record];

            if (row == null)
                return true;

            // 

            DataRowVersion version = DataRowVersion.Default;
            if (row.oldRecord == record) {
                version = DataRowVersion.Original;
            }
            else if (row.newRecord == record) {
                version = DataRowVersion.Current;
            }
            else if (row.tempRecord == record) {
                version = DataRowVersion.Proposed;
            }

            return filter.Invoke(row, version);
        }

        /// <remarks>Only call from inside a lock(this)</remarks>
        internal void ListChangedAdd(DataViewListener listener) {
            _listeners.Add(listener);
        }

        /// <remarks>Only call from inside a lock(this)</remarks>
        internal void ListChangedRemove(DataViewListener listener) {
            _listeners.Remove(listener);
        }

        public int RefCount {
            get {
                return refCount;
            }
        }

        public void AddRef() {
            Bid.Trace("<ds.Index.AddRef|API> %d#\n", ObjectID);
            LockCookie lc = table.indexesLock.UpgradeToWriterLock(-1);
            try {
                Debug.Assert(0 <= refCount, "AddRef on disposed index");
                Debug.Assert(null != records, "null records");
                if (refCount == 0) {
                    table.ShadowIndexCopy();
                    table.indexes.Add(this);
                }
                refCount++;
            }
            finally {
                table.indexesLock.DowngradeFromWriterLock(ref lc);
            }
        }

        public int RemoveRef() {
            Bid.Trace("<ds.Index.RemoveRef|API> %d#\n", ObjectID);
            int count;
            LockCookie lc = table.indexesLock.UpgradeToWriterLock(-1);
            try {
                count = --refCount;
                if (refCount <= 0) {
                    table.ShadowIndexCopy();
                    table.indexes.Remove(this);
                }
            }
            finally {
                table.indexesLock.DowngradeFromWriterLock(ref lc);
            }
            return count;
        }

        private void ApplyChangeAction(int record, int action, int changeRecord) {
            if (action != 0) {
                if (action > 0) {
                    if (AcceptRecord(record)) {
                        InsertRecord(record, true);
                    }
                }
                else if ((null != _comparison) && (-1 != record)) {
                    // when removing a record, the DataRow has already been updated to the newer record
                    // depending on changeRecord, either the new or old record needs be backdated to record
                    // for Comparison<DataRow> to operate correctly
                    DeleteRecord(GetIndex(record, changeRecord));
                }
                else {
                    // unnecessary codepath other than keeping original code path for redbits
                    DeleteRecord(GetIndex(record));
                }
            }
        }

        public bool CheckUnique() {
#if DEBUG
            Debug.Assert(records.CheckUnique(records.root) != HasDuplicates, "CheckUnique difference");
#endif
            return !HasDuplicates;
        }

// only used for main tree compare, not satalite tree
        private int CompareRecords(int record1, int record2) {
            if (null != _comparison) {
                return CompareDataRows(record1, record2);
            }
            if (0 < IndexFields.Length) {
                for (int i = 0; i < IndexFields.Length; i++) {
                    int c = IndexFields[i].Column.Compare(record1, record2);
                    if (c != 0) {
                        return (IndexFields[i].IsDescending ? -c : c);
                    }
                }
                return 0;
            }
            else {
                Debug.Assert(null != table.recordManager[record1], "record1 no datarow");
                Debug.Assert(null != table.recordManager[record2], "record2 no datarow");

                // DataRow needs to always updated appropriately via GetIndex(int,int)
                //table.recordManager.VerifyRecord(record1, table.recordManager[record1]);
                //table.recordManager.VerifyRecord(record2, table.recordManager[record2]);

                // Need to use compare because subtraction will wrap
                // to positive for very large neg numbers, etc.
                return table.Rows.IndexOf(table.recordManager[record1]).CompareTo(table.Rows.IndexOf(table.recordManager[record2]));
            }
        }

        private int CompareDataRows(int record1, int record2)
        {
            table.recordManager.VerifyRecord(record1, table.recordManager[record1]);
            table.recordManager.VerifyRecord(record2, table.recordManager[record2]);
            return _comparison(table.recordManager[record1], table.recordManager[record2]);
        }


// PS: same as previous CompareRecords, except it compares row state if needed
// only used for satalite tree compare
        private int CompareDuplicateRecords(int record1, int record2) {
#if DEBUG
            if (null != _comparison) {
                Debug.Assert(0 == CompareDataRows(record1, record2), "duplicate record not a duplicate by user function");
            }
            else if (record1 != record2) {
                for (int i = 0; i < IndexFields.Length; i++) {
                    int c = IndexFields[i].Column.Compare(record1, record2);
                    Debug.Assert(0 == c, "duplicate record not a duplicate");
                }
            }
#endif
            Debug.Assert(null != table.recordManager[record1], "record1 no datarow");
            Debug.Assert(null != table.recordManager[record2], "record2 no datarow");

            // DataRow needs to always updated appropriately via GetIndex(int,int)
            //table.recordManager.VerifyRecord(record1, table.recordManager[record1]);
            //table.recordManager.VerifyRecord(record2, table.recordManager[record2]);

            if (null == table.recordManager[record1]) {
                return ((null == table.recordManager[record2]) ? 0 : -1);
            }
            else if (null == table.recordManager[record2]) {
                return 1;
            }

            // Need to use compare because subtraction will wrap
            // to positive for very large neg numbers, etc.
            int diff = table.recordManager[record1].rowID.CompareTo(table.recordManager[record2].rowID);

            // if they're two records in the same row, we need to be able to distinguish them.
            if ((diff == 0) && (record1 != record2)) {
                diff = ((int)table.recordManager[record1].GetRecordState(record1)).CompareTo((int)table.recordManager[record2].GetRecordState(record2));
            }
            return diff;
        }

        private int CompareRecordToKey(int record1, object[] vals) {
            for (int i = 0; i < IndexFields.Length; i++) {
                int c = IndexFields[i].Column.CompareValueTo(record1, vals[i]);
                if (c != 0) {
                    return (IndexFields[i].IsDescending ? -c : c);
                }
            }
            return 0;
        }

// DeleteRecordFromIndex deletes the given record from index and does not fire any Event. IT SHOULD NOT FIRE EVENT
// I added this since I can not use existing DeleteRecord which is not silent operation
        public  void DeleteRecordFromIndex(int recordIndex) { // this is for expression use, to maintain expression columns's sort , filter etc. do not fire event
            DeleteRecord(recordIndex, false);
        }
// old and existing DeleteRecord behavior, we can not use this for silently deleting
        private void DeleteRecord(int recordIndex) {
            DeleteRecord(recordIndex, true);
        }
        private void DeleteRecord(int recordIndex, bool fireEvent) {
            Bid.Trace("<ds.Index.DeleteRecord|INFO> %d#, recordIndex=%d, fireEvent=%d{bool}\n", ObjectID, recordIndex, fireEvent);

            if (recordIndex >= 0) {
                recordCount--;
                int record = records.DeleteByIndex(recordIndex);

                MaintainDataView(ListChangedType.ItemDeleted, record, !fireEvent);

                if (fireEvent) {
                    // 1) Webdata 104939 do not fix this, it would be breaking change
                    // 2) newRecord = -1, oldrecord = recordIndex;
                    OnListChanged(ListChangedType.ItemDeleted, recordIndex);
                }
            }
        }

        // SQLBU 428961: Serious performance issue when creating DataView
        // this improves performance by allowing DataView to iterating instead of computing for records over index
        // this will also allow Linq over DataSet to enumerate over the index
        // avoid boxing by returning RBTreeEnumerator (a struct) instead of IEnumerator<int>
        public RBTree<int>.RBTreeEnumerator GetEnumerator(int startIndex) {
            return new IndexTree.RBTreeEnumerator(records, startIndex);
        }

        // What it actually does is find the index in the records[] that
        // this record inhabits, and if it doesn't, suggests what index it would
        // inhabit while setting the high bit.
        // 

        public int GetIndex(int record) {
            int index = records.GetIndexByKey(record);
            return index;
        }

        /// <summary>
        /// When searching by value for a specific record, the DataRow may require backdating to reflect the appropriate state
        /// otherwise on Delete of a DataRow in the Added state, would result in the <see cref="System.Comparison&lt;DataRow&gt;"/> where the row
        /// reflection record would be in the Detatched instead of Added state.
        /// </summary>
        private int GetIndex(int record, int changeRecord) {
            Debug.Assert(null != _comparison, "missing comparison");

            int index;
            DataRow row = table.recordManager[record];

            int a = row.newRecord;
            int b = row.oldRecord;
            try {
                switch(changeRecord) {
                case ReplaceNewRecordForCompare:
                    row.newRecord = record;
                    break;
                case ReplaceOldRecordForCompare:
                     row.oldRecord = record;
                     break;
                }
                table.recordManager.VerifyRecord(record, row);

                index = records.GetIndexByKey(record);
            }
            finally {
                switch(changeRecord) {
                case ReplaceNewRecordForCompare:
                    Debug.Assert(record == row.newRecord, "newRecord has change during GetIndex");
                    row.newRecord = a;
                    break;
                case ReplaceOldRecordForCompare:
                    Debug.Assert(record == row.oldRecord, "oldRecord has change during GetIndex");
                     row.oldRecord = b;
                     break;
                }
#if DEBUG
                if (-1 != a) {
                    table.recordManager.VerifyRecord(a, row);
                }
#endif      
            }
            return index;
        }

        public object[] GetUniqueKeyValues() {
            if (IndexFields == null || IndexFields.Length == 0) {
                return zeroObjects;
            }
            List<object[]> list = new List<object[]>();
            GetUniqueKeyValues(list, records.root);
            return list.ToArray();
        }

        /// <summary>
        /// Find index of maintree node that matches key in record
        /// </summary>
        public int FindRecord(int record) {
            int nodeId = records.Search(record);
            if (nodeId!=IndexTree.NIL)
                return records.GetIndexByNode(nodeId); //always returns the First record index
            else
                return -1;
        }

        public int FindRecordByKey(object key) {
            int nodeId = FindNodeByKey(key);
            if (IndexTree.NIL != nodeId) {
                return records.GetIndexByNode(nodeId);
            }
            return -1; // return -1 to user indicating record not found
        }

        public int FindRecordByKey(object[] key) {
            int nodeId = FindNodeByKeys(key);
            if (IndexTree.NIL != nodeId) {
                return records.GetIndexByNode(nodeId);
            }
            return -1; // return -1 to user indicating record not found
        }

        private int FindNodeByKey(object originalKey) {
            int x, c;
            if (IndexFields.Length != 1) {
                throw ExceptionBuilder.IndexKeyLength(IndexFields.Length, 1);
            }

            x = records.root;
            if (IndexTree.NIL != x) { // otherwise storage may not exist
                DataColumn column = IndexFields[0].Column;
                object key = column.ConvertValue(originalKey);

                x = records.root;
                if (IndexFields[0].IsDescending) {
                    while (IndexTree.NIL != x) {
                        c = column.CompareValueTo(records.Key(x), key);
                        if (c == 0) { break; }
                        if (c < 0) { x = records.Left(x); } // < for decsending
                        else { x = records.Right(x); }
                    }
                }
                else {
                    while (IndexTree.NIL != x) {
                        c = column.CompareValueTo(records.Key(x), key);
                        if (c == 0) { break; }
                        if (c > 0) { x = records.Left(x); } // > for ascending
                        else { x = records.Right(x); }
                    }
                }
            }
            return x;
        }

        private int FindNodeByKeys(object[] originalKey) {
            int x, c;
            c = ((null != originalKey) ? originalKey.Length : 0);
            if ((0 == c) || (IndexFields.Length != c)) {
                throw ExceptionBuilder.IndexKeyLength(IndexFields.Length, c);
            }

            x = records.root;
            if (IndexTree.NIL != x) { // otherwise storage may not exist
                // copy array to avoid changing original
                object[] key = new object[originalKey.Length];
                for(int i = 0; i < originalKey.Length; ++i) {
                    key[i] = IndexFields[i].Column.ConvertValue(originalKey[i]);
                }

                x = records.root;
                while (IndexTree.NIL != x) {
                    c = CompareRecordToKey(records.Key(x), key);
                    if (c == 0) { break; }
                    if (c > 0) { x = records.Left(x); }
                    else { x = records.Right(x); }
                }
            }
            return x;
        }

        private int FindNodeByKeyRecord(int record) {
            int x, c;
            x = records.root;
            if (IndexTree.NIL != x) { // otherwise storage may not exist
                x = records.root;
                while (IndexTree.NIL != x) {
                    c = CompareRecords(records.Key(x), record);
                    if (c == 0) { break; }
                    if (c > 0) { x = records.Left(x); }
                    else { x = records.Right(x); }
                }
            }
            return x;
        }

        
        internal delegate int ComparisonBySelector<TKey,TRow>(TKey key, TRow row) where TRow:DataRow;

        /// <summary>This method exists for LinqDataView to keep a level of abstraction away from the RBTree</summary>
        internal Range FindRecords<TKey,TRow>(ComparisonBySelector<TKey,TRow> comparison, TKey key) where TRow:DataRow
        {
            int x = records.root;
            while (IndexTree.NIL != x)
            {
                int c = comparison(key, (TRow)table.recordManager[records.Key(x)]);
                if (c == 0) { break; }
                if (c < 0) { x = records.Left(x); }
                else { x = records.Right(x); }
            }
            return GetRangeFromNode(x);
        }

        private Range GetRangeFromNode(int nodeId)
        {
            // fill range with the min and max indexes of matching record (i.e min and max of satelite tree)
            // min index is the index of the node in main tree, and max is the min + size of satelite tree-1

            if (IndexTree.NIL == nodeId) {
                return new Range();
            }
            int recordIndex = records.GetIndexByNode(nodeId);

            if (records.Next (nodeId) == IndexTree.NIL)
                return new Range (recordIndex, recordIndex);

            int span = records.SubTreeSize(records.Next(nodeId));
            return new Range (recordIndex, recordIndex + span - 1);
        }

        public Range FindRecords(object key) {
            int nodeId = FindNodeByKey (key);    // main tree node associated with key
            return GetRangeFromNode(nodeId);
        }

        public Range FindRecords(object[] key) {
            int nodeId = FindNodeByKeys (key);    // main tree node associated with key
            return GetRangeFromNode(nodeId);
        }

        internal void FireResetEvent() {
            Bid.Trace("<ds.Index.FireResetEvent|API> %d#\n", ObjectID);
            if (DoListChanged) {
                OnListChanged(DataView.ResetEventArgs);
            }
        }

        private int GetChangeAction(DataViewRowState oldState, DataViewRowState newState) {
            int oldIncluded = ((int)recordStates & (int)oldState) == 0? 0: 1;
            int newIncluded = ((int)recordStates & (int)newState) == 0? 0: 1;
            return newIncluded - oldIncluded;
        }

        /// <summary>Determine if the record that needs backdating is the newRecord or oldRecord or neither</summary>
        private static int GetReplaceAction(DataViewRowState oldState)
        {
            return ((0 != (DataViewRowState.CurrentRows & oldState)) ? ReplaceNewRecordForCompare :    // Added/ModifiedCurrent/Unchanged
                    ((0 != (DataViewRowState.OriginalRows & oldState)) ? ReplaceOldRecordForCompare :   // Deleted/ModififedOriginal
                      DoNotReplaceCompareRecord));                                                      // None
        }

        public DataRow GetRow(int i) {
            return table.recordManager[GetRecord(i)];
        }

        public DataRow[] GetRows(Object[] values) {
            return GetRows(FindRecords(values));
        }

        public DataRow[] GetRows(Range range) {
            DataRow[] newRows = table.NewRowArray(range.Count);
            if (0 < newRows.Length) {
                RBTree<int>.RBTreeEnumerator iterator = GetEnumerator(range.Min);
                for (int i = 0; i < newRows.Length && iterator.MoveNext(); i++) {
                    newRows[i] = table.recordManager[iterator.Current];
                }
            }
            return newRows;
        }

        private void InitRecords(IFilter filter) {
            DataViewRowState states = recordStates;

            // SQLBU 428961: Serious performance issue when creating DataView
            // this improves performance when the is no filter, like with the default view (creating after rows added)
            // we know the records are in the correct order, just append to end, duplicates not possible
            bool append = (0 == IndexFields.Length);

            records = new IndexTree(this);

            recordCount = 0;

            // SQLBU 428961: Serious performance issue when creating DataView
            // this improves performance by iterating of the index instead of computing record by index
            foreach(DataRow b in table.Rows)
            {
                int record = -1;
                if (b.oldRecord == b.newRecord) {
                    if ((int)(states & DataViewRowState.Unchanged) != 0) {
                        record = b.oldRecord;
                    }
                }
                else if (b.oldRecord == -1) {
                    if ((int)(states & DataViewRowState.Added) != 0) {
                        record = b.newRecord;
                    }
                }
                else if (b.newRecord == -1) {
                    if ((int)(states & DataViewRowState.Deleted) != 0) {
                        record = b.oldRecord;
                    }
                }
                else {
                    if ((int)(states & DataViewRowState.ModifiedCurrent) != 0) {
                        record = b.newRecord;
                    }
                    else if ((int)(states & DataViewRowState.ModifiedOriginal) != 0) {
                        record = b.oldRecord;
                    }
                }
                if (record != -1 && AcceptRecord(record, filter))
                {
                    records.InsertAt(-1, record, append);
                    recordCount++;
                }
            }
        }


// InsertRecordToIndex inserts the given record to index and does not fire any Event. IT SHOULD NOT FIRE EVENT
// I added this since I can not use existing InsertRecord which is not silent operation
// it returns the position that record is inserted
        public  int  InsertRecordToIndex(int record) {
            int pos = -1;
            if (AcceptRecord(record)) {
                pos = InsertRecord(record,  false);
            }
            return pos;
        }

// existing functionality, it calls the overlaod with fireEvent== true, so it still fires the event
        private int InsertRecord(int record, bool fireEvent) {
            Bid.Trace("<ds.Index.InsertRecord|INFO> %d#, record=%d, fireEvent=%d{bool}\n", ObjectID, record, fireEvent);

            // SQLBU 428961: Serious performance issue when creating DataView
            // this improves performance when the is no filter, like with the default view (creating before rows added)
            // we know can append when the new record is the last row in table, normal insertion pattern
            bool append = false;
            if ((0 == IndexFields.Length) && (null != table))
            {
                DataRow row = table.recordManager[record];
                append = (table.Rows.IndexOf(row)+1 == table.Rows.Count);
            }
            int nodeId = records.InsertAt(-1, record, append);

            recordCount++;

            MaintainDataView(ListChangedType.ItemAdded, record, !fireEvent);

            if (fireEvent) {
                if (DoListChanged) {
                    OnListChanged(ListChangedType.ItemAdded, records.GetIndexByNode(nodeId));
                }
                return 0;
            }
            else {
                return records.GetIndexByNode(nodeId);
            }
        }


    // Search for specified key
        public bool IsKeyInIndex(object key) {
            int x_id = FindNodeByKey(key);
            return (IndexTree.NIL != x_id);
        }

        public bool IsKeyInIndex(object[] key) {
            int x_id = FindNodeByKeys(key);
            return (IndexTree.NIL != x_id);
        }

        public bool IsKeyRecordInIndex(int record) {
            int x_id = FindNodeByKeyRecord(record);
            return (IndexTree.NIL != x_id);
        }

        private bool DoListChanged {
            get { return (!suspendEvents && _listeners.HasListeners && !table.AreIndexEventsSuspended); }
        }

        private void OnListChanged(ListChangedType changedType, int newIndex, int oldIndex) {
            if (DoListChanged) {
                OnListChanged(new ListChangedEventArgs(changedType, newIndex, oldIndex));
            }
        }

        private void OnListChanged(ListChangedType changedType, int index) {
            if (DoListChanged) {
                OnListChanged(new ListChangedEventArgs(changedType, index));
            }
        }

        private void OnListChanged(ListChangedEventArgs e) {
            Bid.Trace("<ds.Index.OnListChanged|INFO> %d#\n", ObjectID);
            Debug.Assert(DoListChanged, "supposed to check DoListChanged before calling to delay create ListChangedEventArgs");

            _listeners.Notify(e, false, false,
                delegate(DataViewListener listener, ListChangedEventArgs args, bool arg2, bool arg3)
                {
                    listener.IndexListChanged(args);
                });
        }

        private void MaintainDataView(ListChangedType changedType, int record, bool trackAddRemove) {
            Debug.Assert(-1 <= record, "bad record#");

            _listeners.Notify(changedType, ((0 <= record) ? table.recordManager[record] : null), trackAddRemove,
                delegate(DataViewListener listener, ListChangedType type, DataRow row, bool track)
                {
                    listener.MaintainDataView(changedType, row, track);
                });
        }

        public void Reset() {
            Bid.Trace("<ds.Index.Reset|API> %d#\n", ObjectID);
            InitRecords(RowFilter);
            MaintainDataView(ListChangedType.Reset, -1, false); // SQLBU 360388
            FireResetEvent();
        }

        public void RecordChanged(int record) {
            Bid.Trace("<ds.Index.RecordChanged|API> %d#, record=%d\n", ObjectID, record);
            if (DoListChanged) {
                int index = GetIndex(record);
                if (index >= 0) {
                    OnListChanged(ListChangedType.ItemChanged, index);
                }
            }
        }
// new RecordChanged which takes oldIndex and newIndex and fires onListChanged
        public void RecordChanged(int oldIndex, int newIndex) {
            Bid.Trace("<ds.Index.RecordChanged|API> %d#, oldIndex=%d, newIndex=%d\n", ObjectID, oldIndex, newIndex);

            if (oldIndex > -1 || newIndex > -1) { // no need to fire if it was not and will not be in index: this check means at least one version should be in index
                if (oldIndex  == newIndex ) {
                    OnListChanged(ListChangedType.ItemChanged, newIndex, oldIndex);
                }
                else if (oldIndex == -1) { // it is added
                    OnListChanged(ListChangedType.ItemAdded, newIndex, oldIndex);
                }
                else  if (newIndex == -1) { // its deleted
                    // Do not fix this. see 
                    OnListChanged(ListChangedType.ItemDeleted, oldIndex);
                }
                else {
                    OnListChanged(ListChangedType.ItemMoved, newIndex, oldIndex);
                }
            }
        }

        public void RecordStateChanged(int record, DataViewRowState oldState, DataViewRowState newState) {
            Bid.Trace("<ds.Index.RecordStateChanged|API> %d#, record=%d, oldState=%d{ds.DataViewRowState}, newState=%d{ds.DataViewRowState}\n", ObjectID, record, (int)oldState, (int)newState);

            int action = GetChangeAction(oldState, newState);
            ApplyChangeAction(record, action, GetReplaceAction(oldState));
        }

        public void RecordStateChanged(int oldRecord, DataViewRowState oldOldState, DataViewRowState oldNewState,
                                       int newRecord, DataViewRowState newOldState, DataViewRowState newNewState) {

            Bid.Trace("<ds.Index.RecordStateChanged|API> %d#, oldRecord=%d, oldOldState=%d{ds.DataViewRowState}, oldNewState=%d{ds.DataViewRowState}, newRecord=%d, newOldState=%d{ds.DataViewRowState}, newNewState=%d{ds.DataViewRowState}\n", ObjectID,oldRecord, (int)oldOldState, (int)oldNewState, newRecord, (int)newOldState, (int)newNewState);

            Debug.Assert((-1 == oldRecord) || (-1 == newRecord) ||
                         table.recordManager[oldRecord] == table.recordManager[newRecord],
                         "not the same DataRow when updating oldRecord and newRecord");

            int oldAction = GetChangeAction(oldOldState, oldNewState);
            int newAction = GetChangeAction(newOldState, newNewState);
            if (oldAction == -1 && newAction == 1 && AcceptRecord(newRecord)) {

                int oldRecordIndex;
                if ((null != _comparison) && oldAction < 0)
                { // when oldRecord is being removed, allow GetIndexByKey updating the DataRow for Comparison<DataRow>
                    oldRecordIndex = GetIndex (oldRecord, GetReplaceAction(oldOldState));
                }
                else {
                    oldRecordIndex = GetIndex (oldRecord);
                }

                if ((null == _comparison) && oldRecordIndex != -1 && CompareRecords(oldRecord, newRecord)==0) {
                    records.UpdateNodeKey(oldRecord, newRecord);    //change in place, as Both records have same key value

                    int commonIndexLocation = GetIndex(newRecord);
                    OnListChanged(ListChangedType.ItemChanged, commonIndexLocation, commonIndexLocation);
                }
                else {
                    suspendEvents = true;
                    if (oldRecordIndex != -1) {
                        records.DeleteByIndex(oldRecordIndex); // DeleteByIndex doesn't require searching by key
                        recordCount--;
                    }

                    records.Insert(newRecord);
                    recordCount++;

                    suspendEvents = false;

                    int newRecordIndex = GetIndex (newRecord);
                    if(oldRecordIndex == newRecordIndex) { // if the position is the same
                        OnListChanged (ListChangedType.ItemChanged, newRecordIndex, oldRecordIndex); // be carefull remove oldrecord index if needed
                    }
                    else {
                        if (oldRecordIndex == -1) {
                            MaintainDataView(ListChangedType.ItemAdded, newRecord, false);
                            OnListChanged(ListChangedType.ItemAdded, GetIndex(newRecord)); // oldLocation would be -1
                        }
                        else {
                            OnListChanged (ListChangedType.ItemMoved, newRecordIndex, oldRecordIndex);
                        }
                    }
                }
            }
            else {
                ApplyChangeAction(oldRecord, oldAction, GetReplaceAction(oldOldState));
                ApplyChangeAction(newRecord, newAction, GetReplaceAction(newOldState));
            }
        }

        internal DataTable Table {
            get {
                return table;
            }
        }

        private void GetUniqueKeyValues(List<object[]> list, int curNodeId) {
            if (curNodeId != IndexTree.NIL) {
                GetUniqueKeyValues(list, records.Left(curNodeId));

                int record = records.Key(curNodeId);
                object[] element = new object[IndexFields.Length]; // number of columns in PK
                for (int j = 0; j < element.Length; ++j) {
                    element[j] = IndexFields[j].Column[record];
                }
                list.Add(element);

                GetUniqueKeyValues(list, records.Right(curNodeId));
            }
        }

        internal static int IndexOfReference<T>(List<T> list, T item) where T : class {
            if (null != list) {
                for (int i = 0; i < list.Count; ++i) {
                    if (Object.ReferenceEquals(list[i], item)) {
                        return i;
                    }
                }
            }
            return -1;
        }
        internal static bool ContainsReference<T>(List<T> list, T item) where T : class {
            return (0 <= IndexOfReference(list, item));
        }
    }

    internal sealed class Listeners<TElem> where TElem : class
    {
        private readonly List<TElem> listeners;
        private readonly Func<TElem, bool> filter;
        private readonly int ObjectID;
        private int _listenerReaderCount;

        /// <summary>Wish this was defined in mscorlib.dll instead of System.Core.dll</summary>
        internal delegate void Action<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);

        /// <summary>Wish this was defined in mscorlib.dll instead of System.Core.dll</summary>
        internal delegate TResult Func<T1, TResult>(T1 arg1);

        internal Listeners(int ObjectID, Func<TElem, bool> notifyFilter) {
            listeners = new List<TElem>();
            filter = notifyFilter;
            this.ObjectID = ObjectID;
            _listenerReaderCount = 0;
        }

        internal bool HasListeners {
            get { return (0 < listeners.Count); }
        }


        /// <remarks>Only call from inside a lock</remarks>
        internal void Add(TElem listener) {
            Debug.Assert(null != listener, "null listener");
            Debug.Assert(!Index.ContainsReference(listeners, listener), "already contains reference");
            listeners.Add(listener);
        }

        internal int IndexOfReference(TElem listener) {
            return Index.IndexOfReference(listeners, listener);
        }

        /// <remarks>Only call from inside a lock</remarks>
        internal void Remove(TElem listener) {
            Debug.Assert(null != listener, "null listener");

            int index = IndexOfReference(listener);
            Debug.Assert(0 <= index, "listeners don't contain listener");
            listeners[index] = null;

            if (0 == _listenerReaderCount) {
                listeners.RemoveAt(index);
                listeners.TrimExcess();
            }
        }

        /// <summary>
        /// Write operation which means user must control multi-thread and we can assume single thread
        /// </summary>
        internal void Notify<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3, Action<TElem, T1, T2, T3> action) {
            Debug.Assert(null != action, "no action");
            Debug.Assert(0 <= _listenerReaderCount, "negative _listEventCount");

            int count = listeners.Count;
            if (0 < count) {
                int nullIndex = -1;

                // protect against listeners shrinking via Remove
                _listenerReaderCount++;
                try {
                    // protect against listeners growing via Add since new listeners will already have the Notify in progress
                    for (int i = 0; i < count; ++i) {
                        // protect against listener being set to null (instead of being removed)
                        TElem listener = listeners[i];
                        if (filter(listener)) {
                            // perform the action on each listener
                            // some actions may throw an exception blocking remaning listeners from being notified (just like events)
                            action(listener, arg1, arg2, arg3);
                        }
                        else {
                            listeners[i] = null;
                            nullIndex = i;
                        }
                    }
                }
                finally {
                    _listenerReaderCount--;
                }
                if (0 == _listenerReaderCount) {
                    RemoveNullListeners(nullIndex);
                }
            }
        }

        private void RemoveNullListeners(int nullIndex) {
            Debug.Assert((-1 == nullIndex) || (null == listeners[nullIndex]), "non-null listener");
            Debug.Assert(0 == _listenerReaderCount, "0 < _listenerReaderCount");
            for (int i = nullIndex; 0 <= i; --i) {
                if (null == listeners[i]) {
                    listeners.RemoveAt(i);
                }
            }
        }
    }
}
