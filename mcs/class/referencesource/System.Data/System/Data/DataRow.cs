//------------------------------------------------------------------------------
// <copyright file="DataRow.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml;


    /// <devdoc>
    /// <para>Represents a row of data in a <see cref='System.Data.DataTable'/>.</para>
    /// </devdoc>
    public class DataRow {
        private readonly DataTable _table;
        private readonly DataColumnCollection _columns;

        internal int  oldRecord = -1;
        internal int  newRecord = -1;
        internal int  tempRecord;
        internal long  _rowID = -1;

        internal DataRowAction _action;

        internal bool  inChangingEvent;
        internal bool  inDeletingEvent;
        internal bool  inCascade;

        private DataColumn _lastChangedColumn; // last successfully changed column
        private int _countColumnChange;        // number of columns changed during edit mode
        
        private DataError  error;
        private object    _element;
        
        private int _rbTreeNodeId; // if row is not detached, Id used for computing index in rows collection

        private static int _objectTypeCount; // Bid counter
        internal readonly int ObjectID = System.Threading.Interlocked.Increment(ref _objectTypeCount);

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the DataRow.
        ///    </para>
        ///    <para>
        ///       Constructs a row from the builder. Only for internal usage..
        ///    </para>
        /// </devdoc>
        protected internal DataRow (DataRowBuilder builder) {
            tempRecord = builder._record;
            _table = builder._table;
            _columns = _table.Columns;
        }

        internal XmlBoundElement  Element {
            get {
                return (XmlBoundElement) _element;
            }
            set {
                _element = value;
            }
        }

        internal DataColumn LastChangedColumn {
            get { // last successfully changed column or if multiple columns changed: null
                if (_countColumnChange != 1) {
                    return null;
                }
                return _lastChangedColumn;
            }
            set {
                _countColumnChange++;
                _lastChangedColumn = value;
            }
        }

        internal bool HasPropertyChanged {
            get { return (0 < _countColumnChange); }
        }

        internal int RBTreeNodeId {
            get {
                return _rbTreeNodeId;
            }
            set {
                Bid.Trace("<ds.DataRow.set_RBTreeNodeId|INFO> %d#, value=%d\n", ObjectID, value);
                _rbTreeNodeId = value;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the custom error description for a row.</para>
        /// </devdoc>
        public string RowError {
            get {
                return(error == null ? String.Empty :error.Text);
            }
            set {
                Bid.Trace("<ds.DataRow.set_RowError|API> %d#, value='%ls'\n", ObjectID, value);
                if (error == null) {
                    if (!Common.ADP.IsEmpty(value)) {
                        error = new DataError(value);
                    }
                    RowErrorChanged();
                }
                else if(error.Text != value) {
                    error.Text = value;
                    RowErrorChanged();
                }
            }
        }

        private void RowErrorChanged() {
            // We don't know wich record was used by view index. try to use both.
            if (oldRecord != -1)
                _table.RecordChanged(oldRecord);
            if (newRecord != -1)
                _table.RecordChanged(newRecord);
        }

        internal long rowID {
            get {
                return _rowID;
            }
            set {
                ResetLastChangedColumn();
                _rowID = value;
            }
        }

        /// <devdoc>
        ///    <para>Gets the current state of the row in regards to its relationship to the table.</para>
        /// </devdoc>
        public DataRowState RowState {
            get {
                /*
                if (oldRecord == -1 && newRecord == -1)
                    state = DataRowState.Detached; // 2
                else if (oldRecord == newRecord)
                    state = DataRowState.Unchanged; // 2
                else if (oldRecord == -1)
                    state = DataRowState.Added; // 4
                else if (newRecord == -1)
                    state = DataRowState.Deleted; // 4
                else
                    state = DataRowState.Modified; // 4
                */
                if (oldRecord == newRecord) {
                    if (oldRecord == -1) {
                        return DataRowState.Detached; // 2
                    }
                    if (0 < _columns.ColumnsImplementingIChangeTrackingCount) {
                        foreach(DataColumn dc in _columns.ColumnsImplementingIChangeTracking) {
                            object value = this[dc];
                            if ((DBNull.Value != value) && ((IChangeTracking)value).IsChanged) {
                                return DataRowState.Modified; // 3 + _columns.columnsImplementingIChangeTracking.Count
                            }
                        }
                    }
                    return DataRowState.Unchanged; // 3
                }
                else if (oldRecord == -1) {
                    return DataRowState.Added; // 2
                }
                else if (newRecord == -1) {
                    return DataRowState.Deleted; // 3
                }
                return DataRowState.Modified; // 3

            }
        }

        /// <devdoc>
        /// <para>Gets the <see cref='System.Data.DataTable'/>
        /// for which this row has a schema.</para>
        /// </devdoc>
        public DataTable Table {
            get {
                return _table;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets the data stored in the column specified by index.</para>
        /// </devdoc>
        public object this[int columnIndex] {
            get {
                DataColumn column = _columns[columnIndex];
                int record = GetDefaultRecord();
                _table.recordManager.VerifyRecord(record, this);
                VerifyValueFromStorage(column, DataRowVersion.Default, column[record]);
                return column[record];
            }
            set {
                DataColumn column = _columns[columnIndex];
                this[column] = value;
            }
        }

        internal void CheckForLoops(DataRelation rel){
            // don't check for loops in the diffgram
            // because there may be some holes in the rowCollection
            // and index creation may fail. The check will be done
            // after all the loading is done _and_ we are sure there
            // are no holes in the collection.
            if (_table.fInLoadDiffgram || (_table.DataSet != null && _table.DataSet.fInLoadDiffgram))
              return;
            int count = _table.Rows.Count, i = 0;
            // need to optimize this for count > 100
            DataRow parent = this.GetParentRow(rel);
            while (parent != null) {
                if ((parent == this) || (i>count))
                    throw ExceptionBuilder.NestedCircular(_table.TableName);
                i++;
                parent = parent.GetParentRow(rel);
            }
        }

        internal int GetNestedParentCount() {
            int count = 0;
            DataRelation[] nestedParentRelations = _table.NestedParentRelations;
            foreach(DataRelation rel in nestedParentRelations) {
                if (rel == null) // don't like this but done for backward code compatability
                    continue;
                if (rel.ParentTable == _table) // self-nested table
                    this.CheckForLoops(rel);
                DataRow row = this.GetParentRow(rel);
                if (row != null) {
                    count++;
                }
            }
            return count ;
            // Rule 1: At all times, only ONE FK  "(in a row) can be non-Null
            // we wont allow a row to have multiple parents, as we cant handle it , also in diffgram
        }

        /// <devdoc>
        ///    <para>Gets or sets the data stored in the column specified by
        ///       name.</para>
        /// </devdoc>
        public object this[string columnName] {
            get {
                DataColumn column = GetDataColumn(columnName);
                int record = GetDefaultRecord();
                _table.recordManager.VerifyRecord(record, this);
                VerifyValueFromStorage(column, DataRowVersion.Default, column[record]);
                return column[record];
            }
            set {
                DataColumn column = GetDataColumn(columnName);
                this[column] = value;
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets
        ///       the data stored in the specified <see cref='System.Data.DataColumn'/>.</para>
        /// </devdoc>
        public object this[DataColumn column] {
            get {
                CheckColumn(column);
                int record = GetDefaultRecord();
                _table.recordManager.VerifyRecord(record, this);
                VerifyValueFromStorage(column, DataRowVersion.Default, column[record]);
                return column[record];
            }
            set {
                CheckColumn(column);
                if (inChangingEvent) {
                    throw ExceptionBuilder.EditInRowChanging();
                }
                if ((-1 != rowID) && column.ReadOnly) {
                    throw ExceptionBuilder.ReadOnly(column.ColumnName);
                }

                // allow users to tailor the proposed value, or throw an exception.
                // note we intentionally do not try/catch this event.
                // note: we also allow user to do anything at this point
                // infinite loops are possible if user calls Item or ItemArray during the event
                DataColumnChangeEventArgs e = null;
                if (_table.NeedColumnChangeEvents) {
                    e = new DataColumnChangeEventArgs(this, column, value);
                    _table.OnColumnChanging(e);
                }

                if (column.Table != _table) {
                    // user removed column from table during OnColumnChanging event
                    throw ExceptionBuilder.ColumnNotInTheTable(column.ColumnName, _table.TableName);
                }
                if ((-1 != rowID) && column.ReadOnly) {
                    // user adds row to table during OnColumnChanging event
                    throw ExceptionBuilder.ReadOnly(column.ColumnName);
                }
                
                object proposed = ((null != e) ? e.ProposedValue : value);
                if (null == proposed) {
                    if (column.IsValueType) { // WebData 105963
                        throw ExceptionBuilder.CannotSetToNull(column);
                    }
                    proposed = DBNull.Value;
                }

                bool immediate = BeginEditInternal();
                try {
                    int record = GetProposedRecordNo();
                    _table.recordManager.VerifyRecord(record, this);
                    column[record] = proposed;
                }
                catch (Exception e1){
                    // 
                    if (Common.ADP.IsCatchableOrSecurityExceptionType(e1)) {
                        if (immediate) {
                            Debug.Assert(!inChangingEvent, "how are we in a changing event to cancel?");
                            Debug.Assert(-1 != tempRecord, "how no propsed record to cancel?");
                            CancelEdit(); // WebData 107154
                        }
                    }
                    throw;
                }
                LastChangedColumn = column;

                // note: we intentionally do not try/catch this event.
                // infinite loops are possible if user calls Item or ItemArray during the event
                if (null != e) {
                    _table.OnColumnChanged(e); // user may call CancelEdit or EndEdit
                }

                if (immediate) {
                    Debug.Assert(!inChangingEvent, "how are we in a changing event to end?");
                    EndEdit();
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets the data stored
        ///       in the column, specified by index and version of the data to retrieve.</para>
        /// </devdoc>
        public object this[int columnIndex, DataRowVersion version] {
            get {
                DataColumn column = _columns[columnIndex];
                int record = GetRecordFromVersion(version);
                _table.recordManager.VerifyRecord(record, this);
                VerifyValueFromStorage(column, version, column[record]);
                return column[record];
            }
        }

        /// <devdoc>
        ///    <para> Gets the specified version of data stored in
        ///       the named column.</para>
        /// </devdoc>
        public object this[string columnName, DataRowVersion version] {
            get {
                DataColumn column = GetDataColumn(columnName);
                int record = GetRecordFromVersion(version);
                _table.recordManager.VerifyRecord(record, this);
                VerifyValueFromStorage(column, version, column[record]);
                return column[record];
            }
        }

        /// <devdoc>
        /// <para>Gets the specified version of data stored in the specified <see cref='System.Data.DataColumn'/>.</para>
        /// </devdoc>
        public object this[DataColumn column, DataRowVersion version] {
            get {
                CheckColumn(column);
                int record = GetRecordFromVersion(version);
                _table.recordManager.VerifyRecord(record, this);
                VerifyValueFromStorage(column, version, column[record]);
                return column[record];
            }
        }

        /// <devdoc>
        ///    <para>Gets
        ///       or sets all of the values for this row through an array.</para>
        /// </devdoc>
        public object[] ItemArray {
            get {
                int record = GetDefaultRecord();
                _table.recordManager.VerifyRecord(record, this);
                object[] values = new object[_columns.Count];
                for (int i = 0; i < values.Length; i++) {
                    DataColumn column = _columns[i];
                    VerifyValueFromStorage(column, DataRowVersion.Default, column[record]);
                    values[i] = column[record];
                }
                return values;
            }
            set {
                if (null == value) { // WebData 104372
                    throw ExceptionBuilder.ArgumentNull("ItemArray");
                }
                if (_columns.Count < value.Length) {
                    throw ExceptionBuilder.ValueArrayLength();
                }
                DataColumnChangeEventArgs e = null;
                if (_table.NeedColumnChangeEvents) {
                    e = new DataColumnChangeEventArgs(this);
                }
                bool immediate = BeginEditInternal();

                for (int i = 0; i < value.Length; ++i) {
                    // Empty means don't change the row.
                    if (null != value[i]) {
                        // may throw exception if user removes column from table during event
                        DataColumn column = _columns[i];

                        if ((-1 != rowID) && column.ReadOnly) {
                            throw ExceptionBuilder.ReadOnly(column.ColumnName);
                        }

                        // allow users to tailor the proposed value, or throw an exception.
                        // note: we intentionally do not try/catch this event.
                        // note: we also allow user to do anything at this point
                        // infinite loops are possible if user calls Item or ItemArray during the event
                        if (null != e) {
                            e.InitializeColumnChangeEvent(column, value[i]);
                            _table.OnColumnChanging(e);
                        }

                        if (column.Table != _table) {
                            // user removed column from table during OnColumnChanging event
                            throw ExceptionBuilder.ColumnNotInTheTable(column.ColumnName, _table.TableName);
                        }
                        if ((-1 != rowID) && column.ReadOnly) {
                            // user adds row to table during OnColumnChanging event
                            throw ExceptionBuilder.ReadOnly(column.ColumnName);
                        }
                        if (tempRecord == -1) {
                            // user affected CancelEdit or EndEdit during OnColumnChanging event of the last value
                            BeginEditInternal();
                        }

                        object proposed = (null != e) ? e.ProposedValue : value[i];
                        if (null == proposed) {
                            if (column.IsValueType) { // WebData 105963
                                throw ExceptionBuilder.CannotSetToNull(column);
                            }
                            proposed = DBNull.Value;
                        }

                        try {
                            // must get proposed record after each event because user may have
                            // called EndEdit(), AcceptChanges(), BeginEdit() during the event
                            int record = GetProposedRecordNo();
                            _table.recordManager.VerifyRecord(record, this);
                            column[record] = proposed;
                        }
                        catch (Exception e1) {
                            // 
                            if (Common.ADP.IsCatchableOrSecurityExceptionType(e1)) {
                                if (immediate) {
                                    Debug.Assert(!inChangingEvent, "how are we in a changing event to cancel?");
                                    Debug.Assert(-1 != tempRecord, "how no propsed record to cancel?");
                                    CancelEdit(); // WebData 107154
                                }
                            }
                            throw;
                        }
                        LastChangedColumn = column;

                        // note: we intentionally do not try/catch this event.
                        // infinite loops are possible if user calls Item or ItemArray during the event
                        if (null != e) {
                            _table.OnColumnChanged(e);  // user may call CancelEdit or EndEdit
                        }
                    }
                }

                // proposed breaking change: if (immediate){ EndEdit(); } because table currently always fires RowChangedEvent
                Debug.Assert(!inChangingEvent, "how are we in a changing event to end?");
                EndEdit();
            }
        }

        /// <devdoc>
        ///    <para>Commits all the changes made to this row
        ///       since the last time <see cref='System.Data.DataRow.AcceptChanges'/> was called.</para>
        /// </devdoc>
        public void AcceptChanges() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataRow.AcceptChanges|API> %d#\n", ObjectID);
            try {
                EndEdit();

                if (this.RowState != DataRowState.Detached && this.RowState != DataRowState.Deleted) {
                    if (_columns.ColumnsImplementingIChangeTrackingCount > 0) {
                        foreach(DataColumn dc in _columns.ColumnsImplementingIChangeTracking) {
                            object value = this[dc];
                            if (DBNull.Value != value) {
                                IChangeTracking tracking = (IChangeTracking)value;
                                if (tracking.IsChanged) {
                                    tracking.AcceptChanges();
                                }
                            }
                        }
                    }
                }
                _table.CommitRow(this);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        /// <para>Begins an edit operation on a <see cref='System.Data.DataRow'/>object.</para>
        /// </devdoc>
        [
        EditorBrowsableAttribute(EditorBrowsableState.Advanced),
        ]
        public void BeginEdit() {
            BeginEditInternal();
        }

        private bool BeginEditInternal() {
            if (inChangingEvent) {
                throw ExceptionBuilder.BeginEditInRowChanging();
            }
            if (tempRecord != -1) {
                if (tempRecord < _table.recordManager.LastFreeRecord) {
                    return false; // we will not call EndEdit
                }
                else {
                    // partial fix for detached row after Table.Clear scenario
                    // in debug, it will have asserted earlier, but with this
                    // it will go get a new record for editing
                    tempRecord = -1;
                }
                // shifted VerifyRecord to first make the correction, then verify
                _table.recordManager.VerifyRecord(tempRecord, this);
            }

            if (oldRecord != -1 && newRecord == -1) {
                throw ExceptionBuilder.DeletedRowInaccessible();
            }

            // 

            ResetLastChangedColumn(); // shouldn't have to do this

            tempRecord = _table.NewRecord(newRecord);
            Debug.Assert(-1 != tempRecord, "missing temp record");
            Debug.Assert(0 == _countColumnChange, "unexpected column change count");
            Debug.Assert(null == _lastChangedColumn, "unexpected last column change");
            return true;
        }

        /// <devdoc>
        ///    <para>Cancels the current edit on the row.</para>
        /// </devdoc>
        [
        EditorBrowsableAttribute(EditorBrowsableState.Advanced),
        ]
        public void CancelEdit() {
            if (inChangingEvent) {
                throw ExceptionBuilder.CancelEditInRowChanging();
            }

            _table.FreeRecord(ref tempRecord);
            Debug.Assert(-1 == tempRecord, "unexpected temp record");
            ResetLastChangedColumn();
        }

        private void CheckColumn(DataColumn column) {
            if (column == null) {
                throw ExceptionBuilder.ArgumentNull("column");
            }

            if (column.Table != _table) {
                throw ExceptionBuilder.ColumnNotInTheTable(column.ColumnName, _table.TableName);
            }
        }

        /// <devdoc>
        /// Throws a RowNotInTableException if row isn't in table.
        /// </devdoc>
        internal void CheckInTable() {
            if (rowID == -1) {
                throw ExceptionBuilder.RowNotInTheTable();
            }
        }

        /// <devdoc>
        ///    <para>Deletes the row.</para>
        /// </devdoc>
        public void Delete() {
            if (inDeletingEvent) {
                throw ExceptionBuilder.DeleteInRowDeleting();
            }

            if (newRecord == -1)
                return;

            _table.DeleteRow(this);
        }

        /// <devdoc>
        ///    <para>Ends the edit occurring on the row.</para>
        /// </devdoc>
        [
        EditorBrowsableAttribute(EditorBrowsableState.Advanced),
        ]
        public void EndEdit() {
            if (inChangingEvent) {
                throw ExceptionBuilder.EndEditInRowChanging();
            }

            if (newRecord == -1) {
                return; // this is meaningless, detatched row case
            }

            if (tempRecord != -1) {
                try {
                    // suppressing the ensure property changed because it's possible that no values have been modified
                    _table.SetNewRecord(this, tempRecord, suppressEnsurePropertyChanged: true);
                }
                finally {
                    // a constraint violation may be thrown during SetNewRecord
                    ResetLastChangedColumn();
                }
            }
        }

        /// <devdoc>
        ///    <para>Sets the error description for a column specified by index.</para>
        /// </devdoc>
        public void SetColumnError(int columnIndex, string error) {
            DataColumn column = _columns[columnIndex];
            if (column == null)
                throw ExceptionBuilder.ColumnOutOfRange(columnIndex);

            SetColumnError(column, error);
        }

        /// <devdoc>
        ///    <para>Sets
        ///       the error description for a column specified by name.</para>
        /// </devdoc>
        public void SetColumnError(string columnName, string error) {
            DataColumn column = GetDataColumn(columnName);
            SetColumnError(column, error);
        }

        /// <devdoc>
        /// <para>Sets the error description for a column specified as a <see cref='System.Data.DataColumn'/>.</para>
        /// </devdoc>
        public void SetColumnError(DataColumn column, string error) {
            CheckColumn(column);
            
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataRow.SetColumnError|API> %d#, column=%d, error='%ls'\n", ObjectID, column.ObjectID, error);
            try {            
                if (this.error == null)  this.error = new DataError();
                if(GetColumnError(column) != error) {
                    this.error.SetColumnError(column, error);
                    RowErrorChanged();
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <devdoc>
        ///    <para>Gets the error description for the column specified
        ///       by index.</para>
        /// </devdoc>
        public string GetColumnError(int columnIndex) {
            DataColumn column = _columns[columnIndex];
            return GetColumnError(column);
        }

        /// <devdoc>
        ///    <para>Gets the error description for a column, specified by name.</para>
        /// </devdoc>
        public string GetColumnError(string columnName) {
            DataColumn column = GetDataColumn(columnName);
            return GetColumnError(column);
        }

        /// <devdoc>
        ///    <para>Gets the error description of
        ///       the specified <see cref='System.Data.DataColumn'/>.</para>
        /// </devdoc>
        public string GetColumnError(DataColumn column) {
            CheckColumn(column);
            if (error == null)  error = new DataError();
            return error.GetColumnError(column);
        }

        /// <summary>
        /// Clears the errors for the row, including the <see cref='System.Data.DataRow.RowError'/>
        /// and errors set with <see cref='System.Data.DataRow.SetColumnError(DataColumn, string)'/>
        /// </summary>
        public void ClearErrors() {
            if (error != null) {
                error.Clear();
                RowErrorChanged();
            }
        }

        internal void ClearError(DataColumn column) {
            if (error != null) {
                error.Clear(column);
                RowErrorChanged();
            }
        }

        /// <devdoc>
        ///    <para>Gets a value indicating whether there are errors in a columns collection.</para>
        /// </devdoc>
        public bool HasErrors {
            get {
                return(error == null ? false : error.HasErrors);
            }
        }

        /// <devdoc>
        ///    <para>Gets an array of columns that have errors.</para>
        /// </devdoc>
        public DataColumn[] GetColumnsInError() {
            if (error == null)
                return DataTable.zeroColumns;
            else
                return error.GetColumnsInError();
        }


        public DataRow[] GetChildRows(string relationName) {
            return GetChildRows(_table.ChildRelations[relationName], DataRowVersion.Default);
        }

        public DataRow[] GetChildRows(string relationName, DataRowVersion version) {
            return GetChildRows(_table.ChildRelations[relationName], version);
        }

        /// <devdoc>
        /// <para>Gets the child rows of this <see cref='System.Data.DataRow'/> using the
        ///    specified <see cref='System.Data.DataRelation'/>
        ///    .</para>
        /// </devdoc>
        public DataRow[] GetChildRows(DataRelation relation) {
            return GetChildRows(relation, DataRowVersion.Default);
        }

        /// <devdoc>
        /// <para>Gets the child rows of this <see cref='System.Data.DataRow'/> using the specified <see cref='System.Data.DataRelation'/> and the specified <see cref='System.Data.DataRowVersion'/></para>
        /// </devdoc>
        public DataRow[] GetChildRows(DataRelation relation, DataRowVersion version) {
            if (relation == null)
                return _table.NewRowArray(0);

            //if (-1 == rowID)
            //    throw ExceptionBuilder.RowNotInTheTable();

            if (relation.DataSet != _table.DataSet)
                throw ExceptionBuilder.RowNotInTheDataSet();
            if (relation.ParentKey.Table != _table)
                throw ExceptionBuilder.RelationForeignTable(relation.ParentTable.TableName, _table.TableName);
            return DataRelation.GetChildRows(relation.ParentKey, relation.ChildKey, this, version);
        }

        internal DataColumn GetDataColumn(string columnName) {
            DataColumn column = _columns[columnName];
            if (null != column) {
                return column;
            }
            throw ExceptionBuilder.ColumnNotInTheTable(columnName, _table.TableName);
        }

        public DataRow GetParentRow(string relationName) {
            return GetParentRow(_table.ParentRelations[relationName], DataRowVersion.Default);
        }

        public DataRow GetParentRow(string relationName, DataRowVersion version) {
            return GetParentRow(_table.ParentRelations[relationName], version);
        }

        /// <devdoc>
        /// <para>Gets the parent row of this <see cref='System.Data.DataRow'/> using the specified <see cref='System.Data.DataRelation'/> .</para>
        /// </devdoc>
        public DataRow GetParentRow(DataRelation relation) {
            return GetParentRow(relation, DataRowVersion.Default);
        }

        /// <devdoc>
        /// <para>Gets the parent row of this <see cref='System.Data.DataRow'/>
        /// using the specified <see cref='System.Data.DataRelation'/> and <see cref='System.Data.DataRowVersion'/>.</para>
        /// </devdoc>
        public DataRow GetParentRow(DataRelation relation, DataRowVersion version) {
            if (relation == null)
                return null;

            //if (-1 == rowID)
            //    throw ExceptionBuilder.RowNotInTheTable();

            if (relation.DataSet != _table.DataSet)
                throw ExceptionBuilder.RelationForeignRow();

            if (relation.ChildKey.Table != _table)
                throw ExceptionBuilder.GetParentRowTableMismatch(relation.ChildTable.TableName, _table.TableName);

            return DataRelation.GetParentRow(relation.ParentKey, relation.ChildKey, this, version);
        }
        // a multiple nested child table's row can have only one non-null FK per row. So table has multiple
        // parents, but a row can have only one parent. Same nested row cannot below to 2 parent rows.
        internal DataRow GetNestedParentRow(DataRowVersion version) {
            // 1) Walk over all FKs and get the non-null. 2) Get the relation. 3) Get the parent Row.
            DataRelation[] nestedParentRelations = _table.NestedParentRelations;
            foreach(DataRelation rel in nestedParentRelations) {
                if (rel == null) // don't like this but done for backward code compatability
                    continue;
                if (rel.ParentTable == _table) // self-nested table
                    this.CheckForLoops(rel);
                DataRow row = this.GetParentRow(rel, version);
                if (row != null) {
                    return row;
                }
            }
            return null;// Rule 1: At all times, only ONE FK  "(in a row) can be non-Null

        }
        // No Nested in 1-many

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DataRow[] GetParentRows(string relationName) {
            return GetParentRows(_table.ParentRelations[relationName], DataRowVersion.Default);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public DataRow[] GetParentRows(string relationName, DataRowVersion version) {
            return GetParentRows(_table.ParentRelations[relationName], version);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the parent rows of this <see cref='System.Data.DataRow'/> using the specified <see cref='System.Data.DataRelation'/> .
        ///    </para>
        /// </devdoc>
        public DataRow[] GetParentRows(DataRelation relation) {
            return GetParentRows(relation, DataRowVersion.Default);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the parent rows of this <see cref='System.Data.DataRow'/> using the specified <see cref='System.Data.DataRelation'/> .
        ///    </para>
        /// </devdoc>
        public DataRow[] GetParentRows(DataRelation relation, DataRowVersion version) {
            if (relation == null)
                return _table.NewRowArray(0);

            //if (-1 == rowID)
            //    throw ExceptionBuilder.RowNotInTheTable();

            if (relation.DataSet != _table.DataSet)
                throw ExceptionBuilder.RowNotInTheDataSet();

            if (relation.ChildKey.Table != _table)
                throw ExceptionBuilder.GetParentRowTableMismatch(relation.ChildTable.TableName, _table.TableName);

            return DataRelation.GetParentRows(relation.ParentKey, relation.ChildKey, this, version);
        }

        internal object[] GetColumnValues(DataColumn[] columns) {
            return GetColumnValues(columns, DataRowVersion.Default);
        }

        internal object[] GetColumnValues(DataColumn[] columns, DataRowVersion version) {
            DataKey key = new DataKey(columns, false); // temporary key, don't copy columns
            return GetKeyValues(key, version);
        }

        internal object[] GetKeyValues(DataKey key) {
            int record = GetDefaultRecord();
            return key.GetKeyValues(record);
        }

        internal object[] GetKeyValues(DataKey key, DataRowVersion version) {
            int record = GetRecordFromVersion(version);
            return key.GetKeyValues(record);
        }

        internal int GetCurrentRecordNo() {
            if (newRecord == -1)
                throw ExceptionBuilder.NoCurrentData();
            return newRecord;
        }

        internal int GetDefaultRecord() {
            if (tempRecord != -1)
                return tempRecord;
            if (newRecord != -1) {
                return newRecord;
            }
            // If row has oldRecord - this is deleted row.
            if (oldRecord == -1)
                throw ExceptionBuilder.RowRemovedFromTheTable();
            else
                throw ExceptionBuilder.DeletedRowInaccessible();
        }

        internal int GetOriginalRecordNo() {
            if (oldRecord == -1)
                throw ExceptionBuilder.NoOriginalData();
            return oldRecord;
        }

        private int GetProposedRecordNo() {
            if (tempRecord == -1)
                throw ExceptionBuilder.NoProposedData();
            return tempRecord;
        }

        internal int GetRecordFromVersion(DataRowVersion version) {
            switch (version) {
                case DataRowVersion.Original:
                    return GetOriginalRecordNo();
                case DataRowVersion.Current:
                    return GetCurrentRecordNo();
                case DataRowVersion.Proposed:
                    return GetProposedRecordNo();
                case DataRowVersion.Default:
                    return GetDefaultRecord();
                default:
                    throw ExceptionBuilder.InvalidRowVersion();
            }
        }

        internal DataRowVersion GetDefaultRowVersion(DataViewRowState viewState) {
            if (oldRecord == newRecord) {
                if (oldRecord == -1) {
                    // should be DataView.addNewRow
                    return DataRowVersion.Default;
                }
                Debug.Assert(0 != (DataViewRowState.Unchanged & viewState), "not DataViewRowState.Unchanged");
                return DataRowVersion.Default;
            }
            else if (oldRecord == -1) {
                Debug.Assert(0 != (DataViewRowState.Added & viewState), "not DataViewRowState.Added");
                return DataRowVersion.Default;
            }
            else if (newRecord == -1) {
                Debug.Assert(_action==DataRowAction.Rollback || 0 != (DataViewRowState.Deleted & viewState), "not DataViewRowState.Deleted");
                return DataRowVersion.Original;
            }
            else if (0 != (DataViewRowState.ModifiedCurrent & viewState)) {
                return DataRowVersion.Default;
            }
            Debug.Assert(0 != (DataViewRowState.ModifiedOriginal & viewState), "not DataViewRowState.ModifiedOriginal");
            return DataRowVersion.Original;
        }

        internal DataViewRowState GetRecordState(int record) {
            if (record == -1)
                return DataViewRowState.None;
            if (record == oldRecord && record == newRecord)
                return DataViewRowState.Unchanged;
            if (record == oldRecord)
                return(newRecord != -1) ? DataViewRowState.ModifiedOriginal : DataViewRowState.Deleted;
            if (record == newRecord)
                return(oldRecord != -1) ? DataViewRowState.ModifiedCurrent : DataViewRowState.Added;
            return DataViewRowState.None;
        }

        internal bool HasKeyChanged(DataKey key) {
            return HasKeyChanged(key, DataRowVersion.Current, DataRowVersion.Proposed);
        }

        internal bool HasKeyChanged(DataKey key, DataRowVersion version1, DataRowVersion version2) {
            if (!HasVersion(version1) || !HasVersion(version2))
                return true;
            return !key.RecordsEqual(GetRecordFromVersion(version1), GetRecordFromVersion(version2));
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether a specified version exists.
        ///    </para>
        /// </devdoc>
        public bool HasVersion(DataRowVersion version) {
            switch (version) {
                case DataRowVersion.Original:
                    return(oldRecord != -1);
                case DataRowVersion.Current:
                    return(newRecord != -1);
                case DataRowVersion.Proposed:
                    return(tempRecord != -1);
                case DataRowVersion.Default:
                    return(tempRecord != -1 || newRecord != -1);
                default:
                    throw ExceptionBuilder.InvalidRowVersion();
            }
        }

        internal bool HasChanges() {
            if (!HasVersion(DataRowVersion.Original) || !HasVersion(DataRowVersion.Current)) {
                return true; // if does not have original, its added row, if does not have current, its deleted row so it has changes
            }
            foreach(DataColumn dc in Table.Columns) {
                if (dc.Compare(oldRecord, newRecord) != 0) {
                    return true;
                }
            }
            return false;
        }

        internal bool HaveValuesChanged(DataColumn[] columns) {
            return HaveValuesChanged(columns, DataRowVersion.Current, DataRowVersion.Proposed);
        }

        internal bool HaveValuesChanged(DataColumn[] columns, DataRowVersion version1, DataRowVersion version2) {
            for (int i = 0; i < columns.Length; i++) {
                CheckColumn(columns[i]);
            }
            DataKey key = new DataKey(columns, false); // temporary key, don't copy columns
            return HasKeyChanged(key, version1, version2);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       a value indicating whether the column at the specified index contains a
        ///       null value.
        ///    </para>
        /// </devdoc>
        public bool IsNull(int columnIndex) {
            DataColumn column = _columns[columnIndex];
            int record = GetDefaultRecord();
            return column.IsNull(record);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether the named column contains a null value.
        ///    </para>
        /// </devdoc>
        public bool IsNull(string columnName) {
            DataColumn column = GetDataColumn(columnName);
            int record = GetDefaultRecord();
            return column.IsNull(record);
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether the specified <see cref='System.Data.DataColumn'/>
        ///       contains a null value.
        ///    </para>
        /// </devdoc>
        public bool IsNull(DataColumn column) {
            CheckColumn(column);
            int record = GetDefaultRecord();
            return column.IsNull(record);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsNull(DataColumn column, DataRowVersion version) {
            CheckColumn(column);
            int record = GetRecordFromVersion(version);
            return column.IsNull(record);
        }

        /// <devdoc>
        ///    <para>
        ///       Rejects all changes made to the row since <see cref='System.Data.DataRow.AcceptChanges'/>
        ///       was last called.
        ///    </para>
        /// </devdoc>
        public void RejectChanges() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataRow.RejectChanges|API> %d#\n", ObjectID);
            try {
                if (this.RowState != DataRowState.Detached) {
                    if (_columns.ColumnsImplementingIChangeTrackingCount != _columns.ColumnsImplementingIRevertibleChangeTrackingCount) {
                        foreach(DataColumn dc in _columns.ColumnsImplementingIChangeTracking) {
                            if (!dc.ImplementsIRevertibleChangeTracking) {
                                object value = null;
                                if (this.RowState != DataRowState.Deleted)
                                    value = this[dc];
                                else
                                    value = this[dc, DataRowVersion.Original];
                                if (DBNull.Value != value){
                                    if (((IChangeTracking)value).IsChanged) {
                                        throw ExceptionBuilder.UDTImplementsIChangeTrackingButnotIRevertible(dc.DataType.AssemblyQualifiedName);
                                    }
                                }
                            }
                        }
                    }
                    foreach(DataColumn dc in _columns.ColumnsImplementingIChangeTracking) {
                        object value = null;
                         if (this.RowState != DataRowState.Deleted)
                            value = this[dc];
                         else
                            value = this[dc, DataRowVersion.Original];
                        if (DBNull.Value != value) {
                            IChangeTracking tracking = (IChangeTracking)value;
                            if (tracking.IsChanged) {
                                ((IRevertibleChangeTracking)value).RejectChanges();
                            }
                        }
                    }
                }
                _table.RollbackRow(this);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }
        
        internal void ResetLastChangedColumn() {
            _lastChangedColumn = null;
            _countColumnChange = 0;
        }

        internal void SetKeyValues(DataKey key, object[] keyValues) {
            bool fFirstCall = true;
            bool immediate = (tempRecord == -1);

            for (int i = 0; i < keyValues.Length; i++) {
                object value = this[key.ColumnsReference[i]];
                if (!value.Equals(keyValues[i])) {
                    if (immediate && fFirstCall) {
                        fFirstCall = false;
                        BeginEditInternal();
                    }
                    this[key.ColumnsReference[i]] = keyValues[i];
                }
            }
            if (!fFirstCall)
                EndEdit();
        }

        /// <devdoc>
        ///    <para>
        ///       Sets the specified column's value to a null value.
        ///    </para>
        /// </devdoc>
        protected void SetNull(DataColumn column) {
            this[column] = DBNull.Value;
        }

        internal void SetNestedParentRow(DataRow parentRow, bool setNonNested) {
            if (parentRow == null) {
                SetParentRowToDBNull();
                return;
            }

            foreach (DataRelation relation in _table.ParentRelations) {
                if (relation.Nested || setNonNested) {
                    if (relation.ParentKey.Table == parentRow._table) {
                        object[] parentKeyValues = parentRow.GetKeyValues(relation.ParentKey);
                        this.SetKeyValues(relation.ChildKey, parentKeyValues);

                        if (relation.Nested) {
                            if (parentRow._table == _table)
                                this.CheckForLoops(relation);
                            else
                                this.GetParentRow(relation);
                        }
                    }
                }
            }
        }
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public void SetParentRow(DataRow parentRow) {
            SetNestedParentRow(parentRow, true);
        }

        /// <devdoc>
        ///    <para>
        ///       Sets current row's parent row with specified relation.
        ///    </para>
        /// </devdoc>
        public void SetParentRow(DataRow parentRow, DataRelation relation) {
            if (relation == null) {
                SetParentRow(parentRow);
                return;
            }

            if (parentRow == null) {
                SetParentRowToDBNull(relation);
                return;
            }

            //if (-1 == rowID)
            //    throw ExceptionBuilder.ChildRowNotInTheTable();

            //if (-1 == parentRow.rowID)
            //    throw ExceptionBuilder.ParentRowNotInTheTable();

            if (_table.DataSet != parentRow._table.DataSet)
                throw ExceptionBuilder.ParentRowNotInTheDataSet();

            if (relation.ChildKey.Table != _table)
                throw ExceptionBuilder.SetParentRowTableMismatch(relation.ChildKey.Table.TableName, _table.TableName);

            if (relation.ParentKey.Table != parentRow._table)
                throw ExceptionBuilder.SetParentRowTableMismatch(relation.ParentKey.Table.TableName, parentRow._table.TableName);

            object[] parentKeyValues = parentRow.GetKeyValues(relation.ParentKey);
            this.SetKeyValues(relation.ChildKey, parentKeyValues);
        }

        internal void SetParentRowToDBNull() {
            //if (-1 == rowID)
            //    throw ExceptionBuilder.ChildRowNotInTheTable();

            foreach (DataRelation relation in _table.ParentRelations)
                SetParentRowToDBNull(relation);
        }

        internal void SetParentRowToDBNull(DataRelation relation) {
            Debug.Assert(relation != null, "The relation should not be null here.");

            //if (-1 == rowID)
            //    throw ExceptionBuilder.ChildRowNotInTheTable();

            if (relation.ChildKey.Table != _table)
                throw ExceptionBuilder.SetParentRowTableMismatch(relation.ChildKey.Table.TableName, _table.TableName);


            object[] parentKeyValues = new object[1];
            parentKeyValues[0] = DBNull.Value;
            this.SetKeyValues(relation.ChildKey, parentKeyValues);
        }
        public void SetAdded(){
            if (this.RowState == DataRowState.Unchanged) {
                _table.SetOldRecord(this, -1);
            }
            else {
                throw ExceptionBuilder.SetAddedAndModifiedCalledOnnonUnchanged();
            }
        }

        public void SetModified(){
            if (this.RowState == DataRowState.Unchanged) {
                tempRecord = _table.NewRecord(newRecord);
                if (tempRecord != -1) {
                    // suppressing the ensure property changed because no values have changed
                    _table.SetNewRecord(this, tempRecord, suppressEnsurePropertyChanged: true);
                }
            }
            else {
                throw ExceptionBuilder.SetAddedAndModifiedCalledOnnonUnchanged();
            }
        }

/*
    RecordList contains the empty column storage needed. We need to copy the existing record values into this storage.
*/
        internal int CopyValuesIntoStore(ArrayList storeList, ArrayList nullbitList, int storeIndex) {
            int recordCount = 0;
            if (oldRecord != -1) {//Copy original record for the row in Unchanged, Modified, Deleted state.
                for (int i = 0; i < _columns.Count; i++) {
                    _columns[i].CopyValueIntoStore(oldRecord, storeList[i], (BitArray) nullbitList[i], storeIndex);
                }
                recordCount++;
                storeIndex++;
            }

            DataRowState state = RowState;
            if ((DataRowState.Added == state) || (DataRowState.Modified == state)) { //Copy current record for the row in Added, Modified state.
                for (int i = 0; i < _columns.Count; i++) {
                    _columns[i].CopyValueIntoStore(newRecord, storeList[i], (BitArray) nullbitList[i], storeIndex);
                }
                recordCount++;
                storeIndex++;
            }

            if (-1 != tempRecord) {//Copy temp record for the row in edit mode.                
                for (int i = 0; i < _columns.Count; i++) {
                    _columns[i].CopyValueIntoStore(tempRecord, storeList[i], (BitArray)nullbitList[i], storeIndex);
                }
                recordCount++;
                storeIndex++;
            }
            return recordCount;
        }
		
        [Conditional("DEBUG")]
        private void VerifyValueFromStorage(DataColumn column, DataRowVersion version, object valueFromStorage) {
            // Dev11 900390: ignore deleted rows by adding "newRecord != -1" condition - we do not evaluate computed rows if they are deleted
            if (column.DataExpression != null && !inChangingEvent && tempRecord == -1 && newRecord != -1) 
            {
                // for unchanged rows, check current if original is asked for.
                // this is because by design, there is only single storage for an unchanged row.
                if (version == DataRowVersion.Original && oldRecord == newRecord) {
                    version = DataRowVersion.Current;
                }
                // There are various known issues detected by this assert for non-default versions, 
                // for example DevDiv2 bug 73753
                // Since changes consitutute breaking change (either way customer will get another result), 
                // we decided not to fix them in Dev 11
                Debug.Assert(valueFromStorage.Equals(column.DataExpression.Evaluate(this, version)),
                    "Value from storage does lazily computed expression value"); 
            }
        } 
    }

    public sealed class DataRowBuilder {
        internal readonly DataTable   _table;
        internal int                  _record;

        internal DataRowBuilder(DataTable table, int record) {
            _table = table;
            _record = record;
        }
    }
}
