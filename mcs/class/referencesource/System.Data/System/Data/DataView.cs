//------------------------------------------------------------------------------
// <copyright file="DataView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Diagnostics;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;

    /// <devdoc>
    ///    <para>
    ///       Represents a databindable, customized view of a <see cref='System.Data.DataTable'/>
    ///       for sorting, filtering, searching, editing, and navigation.
    ///    </para>
    /// </devdoc>
    [
    Designer("Microsoft.VSDesigner.Data.VS.DataViewDesigner, " + AssemblyRef.MicrosoftVSDesigner),
    Editor("Microsoft.VSDesigner.Data.Design.DataSourceEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
    DefaultProperty("Table"),
    DefaultEvent("PositionChanged")
    ]
    public class DataView : MarshalByValueComponent, IBindingListView , System.ComponentModel.ITypedList, ISupportInitializeNotification {
        private DataViewManager dataViewManager;
        private DataTable table;
        private bool locked = false;
        private Index index;
        private Dictionary<string,Index> findIndexes;

        private string sort = "";

        /// <summary>Allow a user implemented comparision of two DataRow</summary>
        /// <remarks>User must use correct DataRowVersion in comparison or index corruption will happen</remarks>
        private System.Comparison<DataRow> _comparison;

        /// <summary>
        /// IFilter will allow LinqDataView to wrap <see cref='System.Predicate&lt;DataRow&gt;'/> instead of using a DataExpression
        /// </summary>
        private IFilter rowFilter = null;

        private DataViewRowState recordStates = DataViewRowState.CurrentRows;

        private bool shouldOpen = true;
        private bool open = false;
        private bool allowNew = true;
        private bool allowEdit = true;
        private bool allowDelete = true;
        private bool applyDefaultSort = false;

        internal DataRow addNewRow;
        private ListChangedEventArgs addNewMoved;

        private System.ComponentModel.ListChangedEventHandler onListChanged;
        private System.EventHandler  onInitialized;
        internal static ListChangedEventArgs ResetEventArgs = new ListChangedEventArgs(ListChangedType.Reset, -1);

        private DataTable delayedTable = null;
        private string delayedRowFilter = null;
        private string delayedSort = null;
        private DataViewRowState delayedRecordStates = (DataViewRowState)(-1);
        private bool fInitInProgress = false;
        private bool fEndInitInProgress = false;

        /// <summary>
        /// You can't delay create the DataRowView instances since multiple thread read access is valid
        /// and each thread must obtain the same DataRowView instance and we want to avoid (inter)locking.
        /// </summary>
        /// <remarks>
        /// In V1.1, the DataRowView[] was recreated after every change.  Each DataRowView was bound to a DataRow.
        /// In V2.0 Whidbey, the DataRowView retained but bound to an index instead of DataRow, allowing the DataRow to vary.
        /// In V2.0 Orcas, the DataRowView retained and bound to a DataRow, allowing the index to vary.
        /// </remarks>
        private Dictionary<DataRow, DataRowView> rowViewCache = new Dictionary<DataRow, DataRowView>(DataRowReferenceComparer.Default);

        /// <summary>
        /// This collection allows expression maintaince to (add / remove) from the index when it really should be a (change / move).
        /// </summary>
        private readonly Dictionary<DataRow, DataRowView> rowViewBuffer = new Dictionary<DataRow, DataRowView>(DataRowReferenceComparer.Default);

        private sealed class DataRowReferenceComparer : IEqualityComparer<DataRow> {
            internal static readonly DataRowReferenceComparer Default = new DataRowReferenceComparer();

            private DataRowReferenceComparer() { }

            public bool Equals(DataRow x, DataRow y) {
                return ((object)x == (object)y);
            }
            public int GetHashCode(DataRow obj) {
                return obj.ObjectID;
            }
        }

        DataViewListener dvListener = null;

        private static int _objectTypeCount; // Bid counter
        private  readonly int _objectID = System.Threading.Interlocked.Increment(ref _objectTypeCount);

        internal DataView(DataTable table, bool locked) {
            GC.SuppressFinalize(this);
            Bid.Trace("<ds.DataView.DataView|INFO> %d#, table=%d, locked=%d{bool}\n", ObjectID, (table != null) ? table.ObjectID : 0, locked);

            this.dvListener = new DataViewListener(this);
            this.locked = locked;
            this.table = table;
            dvListener.RegisterMetaDataEvents(this.table);
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Data.DataView'/> class.</para>
        /// </devdoc>
        public DataView() : this(null) {
            SetIndex2("", DataViewRowState.CurrentRows, null, true);
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Data.DataView'/> class with the
        ///    specified <see cref='System.Data.DataTable'/>.</para>
        /// </devdoc>
        public DataView(DataTable table) : this(table, false) {
            SetIndex2("", DataViewRowState.CurrentRows, null, true);
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.Data.DataView'/> class with the
        ///    specified <see cref='System.Data.DataTable'/>.</para>
        /// </devdoc>
        public DataView(DataTable table, String RowFilter, string Sort, DataViewRowState RowState) {
            GC.SuppressFinalize(this);
            Bid.Trace("<ds.DataView.DataView|API> %d#, table=%d, RowFilter='%ls', Sort='%ls', RowState=%d{ds.DataViewRowState}\n",
                           ObjectID, (table != null) ? table.ObjectID : 0, RowFilter, Sort, (int)RowState);
            if (table == null)
                throw ExceptionBuilder.CanNotUse();

            this.dvListener = new DataViewListener(this);
            this.locked = false;
            this.table = table;
            dvListener.RegisterMetaDataEvents(this.table);

            if ((((int)RowState) &
                 ((int)~(DataViewRowState.CurrentRows | DataViewRowState.OriginalRows))) != 0) {
                throw ExceptionBuilder.RecordStateRange();
            }
            else if (( ((int)RowState) & ((int)DataViewRowState.ModifiedOriginal) ) != 0 &&
                     ( ((int)RowState) &  ((int)DataViewRowState.ModifiedCurrent) ) != 0
                    ) {
                throw ExceptionBuilder.SetRowStateFilter();
            }

            if (Sort == null)
                Sort = "";

            if (RowFilter == null)
                RowFilter = "";
            DataExpression newFilter = new DataExpression(table, RowFilter);

            SetIndex(Sort, RowState, newFilter);
        }

        /// <summary>
        /// Allow construction of DataView with <see cref="System.Predicate&lt;DataRow&gt;"/> and <see cref="System.Comparison&lt;DataRow&gt;"/>
        /// </summary>
        /// <remarks>This is a copy of the other DataView ctor and needs to be kept in [....]</remarks>
        internal DataView(DataTable table, System.Predicate<DataRow> predicate, System.Comparison<DataRow> comparison, DataViewRowState RowState) {
            GC.SuppressFinalize(this);
            Bid.Trace("<ds.DataView.DataView|API> %d#, table=%d, RowState=%d{ds.DataViewRowState}\n",
                           ObjectID, (table != null) ? table.ObjectID : 0, (int)RowState);
            if (table == null)
                throw ExceptionBuilder.CanNotUse();

            this.dvListener = new DataViewListener(this);
            this.locked = false;
            this.table = table;
            dvListener.RegisterMetaDataEvents(this.table);

            if ((((int)RowState) &
                 ((int)~(DataViewRowState.CurrentRows | DataViewRowState.OriginalRows))) != 0) {
                throw ExceptionBuilder.RecordStateRange();
            }
            else if (( ((int)RowState) & ((int)DataViewRowState.ModifiedOriginal) ) != 0 &&
                     ( ((int)RowState) &  ((int)DataViewRowState.ModifiedCurrent) ) != 0
                    ) {
                throw ExceptionBuilder.SetRowStateFilter();
            }
            _comparison = comparison;
            SetIndex2("", RowState, ((null != predicate) ? new RowPredicateFilter(predicate) : null), true);
        }

        /// <devdoc>
        ///    <para>
        ///       Sets or gets a value indicating whether deletes are
        ///       allowed.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(true),
        ResDescriptionAttribute(Res.DataViewAllowDeleteDescr)
        ]
        public bool AllowDelete {
            get {
                return allowDelete;
            }
            set {
                if (allowDelete != value) {
                    allowDelete = value;
                    OnListChanged(ResetEventArgs);
                }
            }
        }

        /// <devdoc>
        ///    <para>Gets or sets a value indicating whether to use the default sort.</para>
        /// </devdoc>
        [
        RefreshProperties(RefreshProperties.All),
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(false),
        ResDescriptionAttribute(Res.DataViewApplyDefaultSortDescr)
        ]
        public bool ApplyDefaultSort {
            get {
                return applyDefaultSort;
            }
            set {
                Bid.Trace("<ds.DataView.set_ApplyDefaultSort|API> %d#, %d{bool}\n", ObjectID, value);
                if (applyDefaultSort != value) {
                    _comparison = null; // clear the delegate to allow the Sort string to be effective
                    applyDefaultSort = value;
                    UpdateIndex(true);
                    OnListChanged(ResetEventArgs);
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether edits are allowed.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(true),
        ResDescriptionAttribute(Res.DataViewAllowEditDescr)
        ]
        public bool AllowEdit {
            get {
                return allowEdit;
            }
            set {
                if (allowEdit != value) {
                    allowEdit = value;
                    OnListChanged(ResetEventArgs);
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets a value indicating whether the new rows can
        ///       be added using the <see cref='System.Data.DataView.AddNew'/>
        ///       method.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(true),
        ResDescriptionAttribute(Res.DataViewAllowNewDescr)
        ]
        public bool AllowNew {
            get {
                return allowNew;
            }
            set {
                if (allowNew != value) {
                    allowNew = value;
                    OnListChanged(ResetEventArgs);
                }
            }
        }

        /// <summary>
        /// Gets the number of records in the <see cref='System.Data.DataView'/>.
        /// </summary>
        [Browsable(false), ResDescriptionAttribute(Res.DataViewCountDescr)]
        public int Count {
            get {
                Debug.Assert(rowViewCache.Count == CountFromIndex, "DataView.Count mismatch");
                return rowViewCache.Count;
            }
        }

        private int CountFromIndex {
            get {
                return (((null != index) ? index.RecordCount : 0) + ((null != addNewRow) ? 1 : 0));
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the <see cref='System.Data.DataViewManager'/> associated with this <see cref='System.Data.DataView'/> .
        ///    </para>
        /// </devdoc>
        [Browsable(false), ResDescriptionAttribute(Res.DataViewDataViewManagerDescr)]
        public DataViewManager DataViewManager {
            get {
                return dataViewManager;
            }
        }

        [Browsable(false)]
        public bool IsInitialized {
            get {
                return !fInitInProgress;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a value indicating whether the data source is currently open and
        ///       projecting views of data on the <see cref='System.Data.DataTable'/>.
        ///    </para>
        /// </devdoc>
        [Browsable(false), ResDescriptionAttribute(Res.DataViewIsOpenDescr)]
        protected bool IsOpen {
            get {
                return open;
            }
        }

        bool ICollection.IsSynchronized {
            get {
                return false;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the expression used to filter which rows are viewed in the
        ///    <see cref='System.Data.DataView'/>.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(""),
        ResDescriptionAttribute(Res.DataViewRowFilterDescr)
        ]
        public virtual string RowFilter {
            get {       // ACCESSOR: virtual was missing from this get
                DataExpression expression = (rowFilter as DataExpression);
                return(expression == null ? "" : expression.Expression); // 
            }
            set {
                if (value == null)
                    value = "";
                Bid.Trace("<ds.DataView.set_RowFilter|API> %d#, '%ls'\n", ObjectID, value);

                if (fInitInProgress) {
                    delayedRowFilter = value;
                    return;
                }

                CultureInfo locale = (table != null ? table.Locale : CultureInfo.CurrentCulture);
                if (null == rowFilter || (String.Compare(RowFilter,value,false,locale) != 0)) {
                    DataExpression newFilter = new DataExpression(table, value);
                    SetIndex(sort, recordStates, newFilter);
                }
            }
        }

        #region RowPredicateFilter
        /// <summary>
        /// The predicate delegate that will determine if a DataRow should be contained within the view.
        /// This RowPredicate property is mutually exclusive with the RowFilter property.
        /// </summary>
        internal System.Predicate<DataRow> RowPredicate {
            get {
                RowPredicateFilter filter = (GetFilter() as RowPredicateFilter);
                return ((null != filter) ? filter.PredicateFilter : null);
            }
            set {
                if (!Object.ReferenceEquals(RowPredicate, value)) {
                    SetIndex(Sort, RowStateFilter, ((null != value) ? new RowPredicateFilter(value) : null));
                }
            }
        }

        /// <summary></summary>
        private sealed class RowPredicateFilter : System.Data.IFilter {
            internal readonly System.Predicate<DataRow> PredicateFilter;

            /// <summary></summary>
            internal RowPredicateFilter(System.Predicate<DataRow> predicate) {
                Debug.Assert(null != predicate, "null predicate");
                PredicateFilter = predicate;
            }

            /// <summary></summary>
            bool IFilter.Invoke(DataRow row, DataRowVersion version) {
                Debug.Assert(DataRowVersion.Default != version, "not expecting Default");
                Debug.Assert(DataRowVersion.Proposed != version, "not expecting Proposed");
                return PredicateFilter(row);
            }
        }
        #endregion

        /// <devdoc>
        /// <para>Gets or sets the row state filter used in the <see cref='System.Data.DataView'/>.</para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(DataViewRowState.CurrentRows),
        ResDescriptionAttribute(Res.DataViewRowStateFilterDescr)
        ]
        public DataViewRowState RowStateFilter {
            get {
                return recordStates;
            }
            set {
                Bid.Trace("<ds.DataView.set_RowStateFilter|API> %d#, %d{ds.DataViewRowState}\n", ObjectID, (int)value);
                if (fInitInProgress) {
                    delayedRecordStates = value;
                    return;
                }

                if ((((int)value) &
                     ((int)~(DataViewRowState.CurrentRows | DataViewRowState.OriginalRows))) != 0)
                    throw ExceptionBuilder.RecordStateRange();
                else if (( ((int)value) & ((int)DataViewRowState.ModifiedOriginal) ) != 0 &&
                         ( ((int)value) &  ((int)DataViewRowState.ModifiedCurrent) ) != 0
                        )
                    throw ExceptionBuilder.SetRowStateFilter();

                if (recordStates != value) {
                    SetIndex(sort, value, rowFilter);
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets
        ///       or sets the sort column or columns, and sort order for the table.
        ///    </para>
        /// </devdoc>
        [
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(""),
        ResDescriptionAttribute(Res.DataViewSortDescr)
        ]
        public string Sort {
            get {
                if (sort.Length == 0 && applyDefaultSort && table != null && table._primaryIndex.Length > 0) {
                    return table.FormatSortString(table._primaryIndex);
                }
                else {
                    return sort;
                }
            }
            set {
                if (value == null) {
                    value = "";
                }
                Bid.Trace("<ds.DataView.set_Sort|API> %d#, '%ls'\n", ObjectID, value);

                if (fInitInProgress) {
                    delayedSort = value;
                    return;
                }

                CultureInfo locale = (table != null ? table.Locale : CultureInfo.CurrentCulture);
                if (String.Compare(sort, value, false, locale) != 0 || (null != _comparison)) {
                    CheckSort(value);
                    _comparison = null; // clear the delegate to allow the Sort string to be effective
                    SetIndex(value, recordStates, rowFilter);
                }
            }
        }

        /// <summary>Allow a user implemented comparision of two DataRow</summary>
        /// <remarks>User must use correct DataRowVersion in comparison or index corruption will happen</remarks>
        internal System.Comparison<DataRow> SortComparison {
            get {
                return _comparison;
            }
            set {
                Bid.Trace("<ds.DataView.set_SortComparison|API> %d#\n", ObjectID);
                if (!Object.ReferenceEquals(_comparison, value)) {
                    _comparison = value;
                    SetIndex("", recordStates, rowFilter);
                }
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Resets the <see cref='System.Data.DataView.Sort'/> property to its default state.
        ///    </para>
        /// </devdoc>
        private void ResetSort() {
// this is dead code, no one is calling it
            sort = "";
            SetIndex(sort, recordStates, rowFilter);
        }

        /// <devdoc>
        ///    <para>
        ///       Indicates whether the <see cref='System.Data.DataView.Sort'/> property should be persisted.
        ///    </para>
        /// </devdoc>
        private bool ShouldSerializeSort() {
            return(sort != null);
        }

        object ICollection.SyncRoot {
            get {
                return this;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the source <see cref='System.Data.DataTable'/>.
        ///    </para>
        /// </devdoc>
        [
        TypeConverterAttribute(typeof(DataTableTypeConverter)),
        ResCategoryAttribute(Res.DataCategory_Data),
        DefaultValue(null),
        RefreshProperties(RefreshProperties.All),
        ResDescriptionAttribute(Res.DataViewTableDescr)
        ]
        public DataTable Table {
            get {
                return table;
            }
            set {
                Bid.Trace("<ds.DataView.set_Table|API> %d#, %d\n", ObjectID, (value != null) ? value.ObjectID : 0);
                if (fInitInProgress && value != null) {
                    delayedTable = value;
                    return;
                }

                if (locked)
                    throw ExceptionBuilder.SetTable();

                if (dataViewManager != null)
                    throw ExceptionBuilder.CanNotSetTable();

                if (value != null && value.TableName.Length == 0)
                    throw ExceptionBuilder.CanNotBindTable();

                if (table != value) {
                    dvListener.UnregisterMetaDataEvents();
                    table = value;
                    if (table != null) {
                        dvListener.RegisterMetaDataEvents(this.table);
                    }

                    // SQLBU 427284: ListChanged event was being fired after the table change, before the index update.
                    SetIndex2("", DataViewRowState.CurrentRows, null, false);
                    if (table != null) {
                        OnListChanged(new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, new DataTablePropertyDescriptor(table)));
                    }
                    // index was updated without firing the reset, fire it now
                    OnListChanged(ResetEventArgs);
                }
            }
        }

        object IList.this[int recordIndex] {
            get {
                return this[recordIndex];
            }
            set {
                throw ExceptionBuilder.SetIListObject();
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets a row of data from a specified table.
        ///    </para>
        /// </devdoc>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public DataRowView this[int recordIndex] {
            get {
                return GetRowView(GetRow(recordIndex));
            }
        }

        /// <summary>
        /// Adds a new row of data to view.
        /// </summary>
        /// <remarks>
        /// Only one new row of data allowed at a time, so previous new row will be added to row collection.
        /// Unsupported pattern: dataTable.Rows.Add(dataView.AddNew().Row)
        /// </remarks>
        public virtual DataRowView AddNew() {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataView.AddNew|API> %d#\n", ObjectID);
            try {
                CheckOpen();

                if (!AllowNew)
                    throw ExceptionBuilder.AddNewNotAllowNull();
                if (addNewRow != null) {
                    rowViewCache[addNewRow].EndEdit();
                }

                Debug.Assert(null == addNewRow, "AddNew addNewRow is not null");

                addNewRow = table.NewRow();
                DataRowView drv = new DataRowView(this, addNewRow);
                rowViewCache.Add(addNewRow, drv);
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, IndexOf(drv)));
                return drv;
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        public void BeginInit() {
            fInitInProgress = true;
        }

        public void EndInit() {
            if (delayedTable != null && this.delayedTable.fInitInProgress) {
                this.delayedTable.delayedViews.Add(this);
                return;
            }

            fInitInProgress = false;
            fEndInitInProgress = true;
            if (delayedTable != null) {
                Table = delayedTable;
                delayedTable = null;
            }
            if (delayedSort != null) {
                Sort = delayedSort;
                delayedSort = null;
            }
            if (delayedRowFilter != null) {
                RowFilter = delayedRowFilter;
                delayedRowFilter = null;
            }
            if (delayedRecordStates != (DataViewRowState)(-1)) {
                RowStateFilter = delayedRecordStates;
                delayedRecordStates = (DataViewRowState)(-1);
            }
            fEndInitInProgress = false;

            SetIndex(Sort, RowStateFilter, rowFilter);
            OnInitialized();
        }

        private void CheckOpen() {
            if (!IsOpen) throw ExceptionBuilder.NotOpen();
        }

        private void CheckSort(string sort) {
            if (table == null)
                throw ExceptionBuilder.CanNotUse();
            if (sort.Length == 0)
                return;
            table.ParseSortString(sort);
        }

        /// <devdoc>
        ///    <para>
        ///       Closes the <see cref='System.Data.DataView'/>
        ///       .
        ///    </para>
        /// </devdoc>
        protected void Close() {
            shouldOpen = false;
            UpdateIndex();
            dvListener.UnregisterMetaDataEvents();
        }

        public void CopyTo(Array array, int index) {
            if (null != this.index) {
                RBTree<int>.RBTreeEnumerator iterator = this.index.GetEnumerator(0);
                while (iterator.MoveNext()) {
                    array.SetValue(GetRowView(iterator.Current), index);
                    checked {
                        index++;
                    }
                }
            }
            if (null != addNewRow) {
                array.SetValue(rowViewCache[addNewRow], index);
            }
        }

        private void CopyTo(DataRowView[] array, int index) {
            if (null != this.index) {
                RBTree<int>.RBTreeEnumerator iterator = this.index.GetEnumerator(0);
                while (iterator.MoveNext()) {
                    array[index] = GetRowView(iterator.Current);
                    checked {
                        index++;
                    }
                }
            }
            if (null != addNewRow) {
                array[index] = rowViewCache[addNewRow];
            }
        }

        /// <devdoc>
        ///    <para>Deletes a row at the specified index.</para>
        /// </devdoc>
        public void Delete(int index) {
            Delete(GetRow(index));
        }

        internal void Delete(DataRow row) {
            if (null != row) {
                IntPtr hscp;
                Bid.ScopeEnter(out hscp, "<ds.DataView.Delete|API> %d#, row=%d#", ObjectID, row.ObjectID);
                try {
                    CheckOpen();
                    if (row == addNewRow) {
                        FinishAddNew(false);
                        return;
                    }
                    if (!AllowDelete)
                    {
                        throw ExceptionBuilder.CanNotDelete();
                    }
                    row.Delete();
                }
                finally {
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }


        protected override void Dispose(bool disposing) {
            if (disposing) {
            	Close();
            }
            base.Dispose(disposing);
        }

        /// <devdoc>
        ///    <para>
        ///       Finds a row in the <see cref='System.Data.DataView'/> by the specified primary key
        ///       value.
        ///    </para>
        /// </devdoc>
        public int Find(object key) {
            return FindByKey(key);
       }

        /// <summary>Find index of a DataRowView instance that matches the specified primary key value.</summary>
        internal virtual int FindByKey(object key) {
            return index.FindRecordByKey(key);
        }

        /// <devdoc>
        ///    <para>
        ///       Finds a row in the <see cref='System.Data.DataView'/> by the specified primary key values.
        ///    </para>
        /// </devdoc>
        public int Find(object[] key) {
            return FindByKey(key);
        }

        /// <summary>Find index of a DataRowView instance that matches the specified primary key values.</summary>
        internal virtual int FindByKey(object[] key) {
            return index.FindRecordByKey(key);
        }

        /// <devdoc>
        ///    <para>
        ///       Finds a row in the <see cref='System.Data.DataView'/> by the specified primary key
        ///       value.
        ///    </para>
        /// </devdoc>
        public DataRowView[] FindRows(object key) {
            return FindRowsByKey(new object[] {key});
        }

        /// <devdoc>
        ///    <para>
        ///       Finds a row in the <see cref='System.Data.DataView'/> by the specified primary key values.
        ///    </para>
        /// </devdoc>
        public DataRowView[] FindRows(object[] key) {
            return FindRowsByKey(key);
        }

        /// <summary>Find DataRowView instances that match the specified primary key values.</summary>
        internal virtual DataRowView[] FindRowsByKey(object[] key) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataView.FindRows|API> %d#\n", ObjectID);
            try {
                Range range = index.FindRecords(key);
                return GetDataRowViewFromRange(range);
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }
        }

        /// <summary>This method exists for LinqDataView to keep a level of abstraction away from the RBTree</summary>
        internal Range FindRecords<TKey,TRow>(Index.ComparisonBySelector<TKey,TRow> comparison, TKey key) where TRow:DataRow
        {
            return this.index.FindRecords(comparison, key);
        }

        /// <summary>Convert a Range into a DataRowView[].</summary>
        internal DataRowView[] GetDataRowViewFromRange(Range range)
        {
            if (range.IsNull) {
                return new DataRowView[0];
            }
            DataRowView[] rows = new DataRowView[range.Count];
            for (int i=0; i<rows.Length; i++) {
                rows[i] = this[i + range.Min];
            }
            return rows;
        }

        internal void FinishAddNew(bool success) {
            Debug.Assert(null != addNewRow, "null addNewRow");
            Bid.Trace("<ds.DataView.FinishAddNew|INFO> %d#, success=%d{bool}\n", ObjectID, success);

            DataRow newRow = addNewRow;
            if (success) {
                if (DataRowState.Detached == newRow.RowState) {
                    // MaintainDataView will translate the ItemAdded from the RowCollection into
                    // into either an ItemMoved or no event, since it didn't change position.
                    // also possible it's added to the RowCollection but filtered out of the view.
                    table.Rows.Add(newRow);
                }
                else {
                    // this means that the record was added to the table by different means and not part of view
                    newRow.EndEdit();
                }
            }

            if (newRow == addNewRow) {
                // this means that the record did not get to the view
                bool flag = rowViewCache.Remove(addNewRow);
                Debug.Assert(flag, "didn't remove addNewRow");
                addNewRow = null;

                if (!success) {
                    newRow.CancelEdit();
                }
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, Count));
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets an enumerator for this <see cref='System.Data.DataView'/>.
        ///    </para>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            // V1.1 compatability: returning List<DataRowView>.GetEnumerator() from RowViewCache
            // prevents users from changing data without invalidating the enumerator
            // aka don't 'return this.RowViewCache.GetEnumerator()'
            DataRowView[] temp = new DataRowView[this.Count];
            this.CopyTo(temp, 0);
            return temp.GetEnumerator();
        }

        #region IList

        bool IList.IsReadOnly {
            get {
                return false;
            }
        }

        bool IList.IsFixedSize {
            get {
                return false;
            }
        }

        int IList.Add(object value) {
            if (value == null) {
                // null is default value, so we AddNew.
                AddNew();
                return Count - 1;
            }
            throw ExceptionBuilder.AddExternalObject();
        }

        void IList.Clear() {
            throw ExceptionBuilder.CanNotClear();
        }

        bool IList.Contains(object value) {
            return (0 <= IndexOf(value as DataRowView));
        }

        int IList.IndexOf(object value) {
            return IndexOf(value as DataRowView);
        }

        /// <summary>Return positional index of a <see cref="DataRowView"/> in this DataView</summary>
        /// <remarks>Behavioral change: will now return -1 once a DataRowView becomes detached.</remarks>
        internal int IndexOf(DataRowView rowview) {
            if (null != rowview) {
                if (Object.ReferenceEquals(addNewRow, rowview.Row)) {
                    return Count - 1;
                }
                if ((null != index) && (DataRowState.Detached != rowview.Row.RowState)) {
                    DataRowView cached; // verify the DataRowView is one we currently track - not something previously detached
                    if (rowViewCache.TryGetValue(rowview.Row, out cached) && ((object)cached == (object)rowview)) {
                        return IndexOfDataRowView(rowview);
                    }
                }
            }
            return -1;
        }

        private int IndexOfDataRowView(DataRowView rowview) {
            // rowview.GetRecord() may return the proposed record
            // the index will only contain the original or current record, never proposed.
            // return index.GetIndex(rowview.GetRecord());
            return index.GetIndex(rowview.Row.GetRecordFromVersion(rowview.Row.GetDefaultRowVersion(this.RowStateFilter) & ~DataRowVersion.Proposed));
        }

        void IList.Insert(int index, object value) {
            throw ExceptionBuilder.InsertExternalObject();
        }

        void IList.Remove(object value) {
            int index = IndexOf(value as DataRowView);
            if (0 <= index) {
                // must delegate to IList.RemoveAt
                ((IList)this).RemoveAt(index);
            }
            else {
                throw ExceptionBuilder.RemoveExternalObject();
            }
        }

        void IList.RemoveAt(int index) {
            Delete(index);
        }

        internal Index GetFindIndex(string column, bool keepIndex) {
            if (findIndexes == null) {
                findIndexes = new Dictionary<string,Index>();
            }
            Index findIndex;
            if (findIndexes.TryGetValue(column, out findIndex)) {
                if (!keepIndex) {
                    findIndexes.Remove(column);
                    findIndex.RemoveRef();
                    if (findIndex.RefCount == 1) { // if we have created it and we are removing it, refCount is (1)
                        findIndex.RemoveRef(); // if we are reusing the index created by others, refcount is (2)
                    }
                }
            }
            else {
                if (keepIndex) {
                    findIndex = table.GetIndex(column, recordStates, GetFilter());
                    findIndexes[column] = findIndex;
                    findIndex.AddRef();
                }
            }
            return findIndex;
        }

        #endregion

        #region IBindingList implementation

        bool IBindingList.AllowNew {
            get { return AllowNew; }
        }
        object IBindingList.AddNew() {
            return AddNew();
        }

        bool IBindingList.AllowEdit {
            get { return AllowEdit; }
        }

        bool IBindingList.AllowRemove {
            get { return AllowDelete; }
        }

        bool IBindingList.SupportsChangeNotification {
            get { return true; }
        }

        bool IBindingList.SupportsSearching {
            get { return true; }
        }

        bool IBindingList.SupportsSorting {
            get { return true; }
        }

        bool IBindingList.IsSorted {
            get { return this.Sort.Length != 0; }
        }

        PropertyDescriptor IBindingList.SortProperty {
            get {
                return GetSortProperty();
            }
        }

        internal PropertyDescriptor GetSortProperty() {
            if (table != null && index != null && index.IndexFields.Length == 1) {
                return new DataColumnPropertyDescriptor(index.IndexFields[0].Column);
            }
            return null;
        }

        ListSortDirection IBindingList.SortDirection {
            get {
                if (index.IndexFields.Length == 1 && index.IndexFields[0].IsDescending) {
                    return ListSortDirection.Descending;
                }
                return ListSortDirection.Ascending;
            }
        }

        #endregion

        #region ListChanged & Initialized events

        /// <devdoc>
        ///    <para>
        ///       Occurs when the list managed by the <see cref='System.Data.DataView'/> changes.
        ///    </para>
        /// </devdoc>
        [ResCategoryAttribute(Res.DataCategory_Data), ResDescriptionAttribute(Res.DataViewListChangedDescr)]
        public event System.ComponentModel.ListChangedEventHandler ListChanged {
            add {
                Bid.Trace("<ds.DataView.add_ListChanged|API> %d#\n", ObjectID);
                onListChanged += value;
            }
            remove {
                Bid.Trace("<ds.DataView.remove_ListChanged|API> %d#\n", ObjectID);
                onListChanged -= value;
            }
        }

        [
            ResCategoryAttribute(Res.DataCategory_Action),
            ResDescriptionAttribute(Res.DataSetInitializedDescr)
        ]
        public event System.EventHandler  Initialized {
            add {
                onInitialized += value;
            }
            remove {
                onInitialized -= value;
            }
        }

        #endregion

        #region IBindingList implementation

        void IBindingList.AddIndex(PropertyDescriptor property) {
            GetFindIndex(property.Name, /*keepIndex:*/true);
        }

        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction) {
            this.Sort = CreateSortString(property, direction);
        }

        int IBindingList.Find(PropertyDescriptor property, object key) { // NOTE: this function had keepIndex previosely
            if (property != null) {
                bool created = false;
                Index findIndex = null;
                try {
                    if ((null == findIndexes) || !findIndexes.TryGetValue(property.Name, out findIndex)) {
                        created = true;
                        findIndex = table.GetIndex(property.Name, recordStates, GetFilter());
                        findIndex.AddRef();
                    }
                    Range recordRange = findIndex.FindRecords(key);

                    if (!recordRange.IsNull) {
                        // check to see if key is equal
                        return index.GetIndex(findIndex.GetRecord(recordRange.Min));
                    }
                }
                finally {
                    if (created && (null != findIndex)) {
                        findIndex.RemoveRef();
                        if (findIndex.RefCount == 1) { // if we have created it and we are removing it, refCount is (1)
                            findIndex.RemoveRef(); // if we are reusing the index created by others, refcount is (2)
                        }
                    }
                }
            }
            return -1;
        }

        void IBindingList.RemoveIndex(PropertyDescriptor property) {
            // Ups: If we don't have index yet we will create it before destroing; Fix this later
            GetFindIndex(property.Name, /*keepIndex:*/false);
        }

        void IBindingList.RemoveSort() {
            Bid.Trace("<ds.DataView.RemoveSort|API> %d#\n", ObjectID);
            this.Sort = string.Empty;
        }

        #endregion

        #region Additional method and properties for new interface IBindingListView

        void IBindingListView.ApplySort(ListSortDescriptionCollection sorts) {
            if (sorts == null)
                throw ExceptionBuilder.ArgumentNull("sorts");

            StringBuilder sortString = new StringBuilder();
            bool addCommaToString = false;
            foreach(ListSortDescription sort in sorts) {
                if (sort == null)
                    throw ExceptionBuilder.ArgumentContainsNull("sorts");
                PropertyDescriptor property = sort.PropertyDescriptor;

                if (property == null)
                    throw ExceptionBuilder.ArgumentNull("PropertyDescriptor");

                if (!this.table.Columns.Contains(property.Name)) { // just check if column does not exist, we will handle duplicate column in Sort
                    throw ExceptionBuilder.ColumnToSortIsOutOfRange(property.Name);
                }
                ListSortDirection direction = sort.SortDirection;

                if (addCommaToString) // (sortStr.Length != 0)
                    sortString.Append(',');
                sortString.Append(CreateSortString(property, direction));

                if (!addCommaToString)
                    addCommaToString = true;
            }
            this.Sort  = sortString.ToString(); // what if we dont have any valid sort criteira? we would reset the sort
        }

        private string CreateSortString(PropertyDescriptor property, ListSortDirection direction) {
            Debug.Assert (property != null,  "property is null");
            StringBuilder resultString = new StringBuilder();
            resultString.Append('[');
            resultString.Append(property.Name);
            resultString.Append(']');
            if (ListSortDirection.Descending == direction) {
                resultString.Append(" DESC");
            }

            return resultString.ToString();
        }

        void IBindingListView.RemoveFilter() {
            Bid.Trace("<ds.DataView.RemoveFilter|API> %d#\n", ObjectID);
            this.RowFilter = "";
        }

        string IBindingListView.Filter {
            get { return this.RowFilter; }
            set { this.RowFilter = value; }
        }

        ListSortDescriptionCollection IBindingListView.SortDescriptions {
            get {
                return GetSortDescriptions();
            }
        }

        internal ListSortDescriptionCollection GetSortDescriptions() {
            ListSortDescription[] sortDescArray = new ListSortDescription[0];
            if (table != null && index != null && index.IndexFields.Length > 0) {
                sortDescArray = new ListSortDescription[index.IndexFields.Length];
                for(int i = 0; i < index.IndexFields.Length; i++ ) {
                    DataColumnPropertyDescriptor columnProperty = new DataColumnPropertyDescriptor(index.IndexFields[i].Column);
                    if (index.IndexFields[i].IsDescending) {
                        sortDescArray[i] = new ListSortDescription(columnProperty, ListSortDirection.Descending);
                    }
                    else {
                        sortDescArray[i] = new ListSortDescription(columnProperty, ListSortDirection.Ascending);
                    }
                }
            }
            return new ListSortDescriptionCollection(sortDescArray);
        }


        bool IBindingListView.SupportsAdvancedSorting {
            get { return true; }
        }

        bool IBindingListView.SupportsFiltering {
            get { return true; }
        }

        #endregion

        #region ITypedList

        string System.ComponentModel.ITypedList.GetListName(PropertyDescriptor[] listAccessors) {
            if(table != null) {
                if (listAccessors == null || listAccessors.Length == 0) {
                    return table.TableName;
                }
                else {
                    DataSet dataSet = table.DataSet;
                    if (dataSet != null) {
                        DataTable foundTable = dataSet.FindTable(table, listAccessors, 0);
                        if (foundTable != null) {
                            return foundTable.TableName;
                        }
                    }
                }
            }
            return String.Empty;
        }

        PropertyDescriptorCollection System.ComponentModel.ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors) {
            if (table != null) {
                if (listAccessors == null || listAccessors.Length == 0) {
                    return table.GetPropertyDescriptorCollection(null);
                }
                else {
                    DataSet dataSet = table.DataSet;
                    if (dataSet == null)
                        return new PropertyDescriptorCollection(null);
                    DataTable foundTable = dataSet.FindTable(table, listAccessors, 0);
                    if (foundTable != null) {
                        return foundTable.GetPropertyDescriptorCollection(null);
                    }
                }
            }
            return new PropertyDescriptorCollection(null);
        }

        #endregion

        /// <devdoc>
        ///    <para>
        ///       Gets the filter for the <see cref='System.Data.DataView'/>.
        ///    </para>
        /// </devdoc>
        internal virtual IFilter GetFilter() {
            return rowFilter;
        }

        private int GetRecord(int recordIndex) {
            if (unchecked((uint)Count <= (uint)recordIndex))
                throw ExceptionBuilder.RowOutOfRange(recordIndex);
            if (recordIndex == index.RecordCount)
                return addNewRow.GetDefaultRecord();
            return index.GetRecord(recordIndex);
        }

        /// <exception cref="IndexOutOfRangeException"></exception>
        internal DataRow GetRow(int index) {
            int count = Count;
            if (unchecked((uint)count <= (uint)index)) {
                throw ExceptionBuilder.GetElementIndex(index);
            }
            if ((index == (count - 1)) && (addNewRow != null)) {
                // if we could rely on tempRecord being registered with recordManager
                // then this special case code would go away
                return addNewRow;
            }
            return table.recordManager[GetRecord(index)];
        }

        private DataRowView GetRowView(int record) {
            return GetRowView(table.recordManager[record]);
        }

        private DataRowView GetRowView(DataRow dr) {
            return rowViewCache[dr];
        }

        protected virtual void IndexListChanged(object sender, ListChangedEventArgs e) {
            if (ListChangedType.Reset != e.ListChangedType) {
                OnListChanged(e);
            }
            if (addNewRow != null && index.RecordCount == 0) { // [....] : 83032 Clear the newly added row as the underlying index is reset.
                FinishAddNew(false);
            }
            if (ListChangedType.Reset == e.ListChangedType) {
                OnListChanged(e);
            }
        }

        internal void IndexListChangedInternal(ListChangedEventArgs e) {
            rowViewBuffer.Clear();

            if ((ListChangedType.ItemAdded == e.ListChangedType) && (null != addNewMoved)) {
                if (addNewMoved.NewIndex == addNewMoved.OldIndex) {
                    // ItemAdded for addNewRow which didn't change position
                    // RowStateChange only triggers RowChanged, not ListChanged
                }
                else {
                    // translate the ItemAdded into ItemMoved for addNewRow adding into sorted collection
                    ListChangedEventArgs f = addNewMoved;
                    addNewMoved = null;
                    IndexListChanged(this, f);
                }
            }
            // the ItemAdded has to fire twice for AddNewRow (public IBindingList API documentation)
            IndexListChanged(this, e);
        }

        internal void MaintainDataView(ListChangedType changedType, DataRow row, bool trackAddRemove) {
            DataRowView buffer = null;
            switch (changedType) {
                case ListChangedType.ItemAdded:
                    Debug.Assert(null != row, "MaintainDataView.ItemAdded with null DataRow");
                    if (trackAddRemove) {
                        if (rowViewBuffer.TryGetValue(row, out buffer)) {
                            // help turn expression add/remove into a changed/move
                            bool flag = rowViewBuffer.Remove(row);
                            Debug.Assert(flag, "row actually removed");
                        }
                    }
                    if (row == addNewRow) {
                        // DataView.AddNew().Row was added to DataRowCollection
                        int index = IndexOfDataRowView(rowViewCache[addNewRow]);
                        Debug.Assert(0 <= index, "ItemAdded was actually deleted");

                        addNewRow = null;
                        addNewMoved = new ListChangedEventArgs(ListChangedType.ItemMoved, index, Count - 1);
                    }
                    else if (!rowViewCache.ContainsKey(row)) {
                        rowViewCache.Add(row, buffer ?? new DataRowView(this, row));
                    }
                    else {
                        Debug.Assert(false, "ItemAdded DataRow already in view");
                    }
                    break;
                case ListChangedType.ItemDeleted:
                    Debug.Assert(null != row, "MaintainDataView.ItemDeleted with null DataRow");
                    Debug.Assert(row != addNewRow, "addNewRow being deleted");

                    if (trackAddRemove) {
                        // help turn expression add/remove into a changed/move
                        rowViewCache.TryGetValue(row, out buffer);
                        if (null != buffer) {
                            rowViewBuffer.Add(row, buffer);
                        }
                        else {
                            Debug.Assert(false, "ItemDeleted DataRow not in view tracking");
                        }
                    }
                    if (!rowViewCache.Remove(row)) {
                        Debug.Assert(false, "ItemDeleted DataRow not in view");
                    }
                    break;
                case ListChangedType.Reset:
                    Debug.Assert(null == row, "MaintainDataView.Reset with non-null DataRow");
                    ResetRowViewCache();
                    break;
                case ListChangedType.ItemChanged:
                case ListChangedType.ItemMoved:
                    break;
                case ListChangedType.PropertyDescriptorAdded:
                case ListChangedType.PropertyDescriptorChanged:
                case ListChangedType.PropertyDescriptorDeleted:
                    Debug.Assert(false, "unexpected");
                    break;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Raises the <see cref='E:System.Data.DataView.ListChanged'/> event.
        ///    </para>
        /// </devdoc>
        protected virtual void OnListChanged(ListChangedEventArgs e) {
            Bid.Trace("<ds.DataView.OnListChanged|INFO> %d#, ListChangedType=%d{ListChangedType}\n", ObjectID, (int)e.ListChangedType);
            try {
                DataColumn col = null;
                string propertyName = null;
                switch (e.ListChangedType) {
                    case ListChangedType.ItemChanged:
                    // ItemChanged - a column value changed (0 <= e.OldIndex)
                    // ItemChanged - a DataRow.RowError changed (-1 == e.OldIndex)
                    // ItemChanged - RowState changed (e.NewIndex == e.OldIndex)

                    case ListChangedType.ItemMoved:
                        // ItemMoved - a column value affecting sort order changed
                        // ItemMoved - a state change in equivalent fields
                        Debug.Assert(((ListChangedType.ItemChanged == e.ListChangedType) && ((e.NewIndex == e.OldIndex) || (-1 == e.OldIndex))) ||
                                     (ListChangedType.ItemMoved == e.ListChangedType && (e.NewIndex != e.OldIndex) && (0 <= e.OldIndex)),
                                     "unexpected ItemChanged|ItemMoved");

                        Debug.Assert(0 <= e.NewIndex, "negative NewIndex");
                        if (0 <= e.NewIndex) {
                            DataRow dr = GetRow(e.NewIndex);
                            if (dr.HasPropertyChanged) {
                                col = dr.LastChangedColumn;
                                propertyName = (null != col) ? col.ColumnName : String.Empty;
                            }
                        }

                        break;

                    case ListChangedType.ItemAdded:
                    case ListChangedType.ItemDeleted:
                    case ListChangedType.PropertyDescriptorAdded:
                    case ListChangedType.PropertyDescriptorChanged:
                    case ListChangedType.PropertyDescriptorDeleted:
                    case ListChangedType.Reset:
                        break;
                }

                if (onListChanged != null) {
                    if ((col != null) && (e.NewIndex == e.OldIndex)) {
                        ListChangedEventArgs newEventArg = new ListChangedEventArgs(e.ListChangedType, e.NewIndex, new DataColumnPropertyDescriptor(col));
                        onListChanged(this, newEventArg);
                    }
                    else {
                        onListChanged(this, e);
                    }
                }
                if (null != propertyName) {
                    // empty string if more than 1 column changed
                    this[e.NewIndex].RaisePropertyChangedEvent(propertyName);
                }
            }
            catch (Exception f) {
                // 
                if (!Common.ADP.IsCatchableExceptionType(f)) {
                    throw;
                }
                ExceptionBuilder.TraceExceptionWithoutRethrow(f);
                // ignore the exception
            }
        }

        private void OnInitialized() {
            if (onInitialized != null) {
                onInitialized(this, EventArgs.Empty);
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Opens a <see cref='System.Data.DataView'/>.
        ///    </para>
        /// </devdoc>
        protected void Open() {
            shouldOpen = true;
            UpdateIndex();
            dvListener.RegisterMetaDataEvents(this.table);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void Reset() {
            if (IsOpen) {
                index.Reset();
            }
        }

        internal void ResetRowViewCache() {
            Dictionary<DataRow, DataRowView> rvc = new Dictionary<DataRow, DataRowView>(CountFromIndex, DataRowReferenceComparer.Default);
            DataRowView drv;

            if (null != index) {
                // SQLBU 428961: Serious performance issue when creating DataView
                // this improves performance by iterating of the index instead of computing record by index
                RBTree<int>.RBTreeEnumerator iterator = index.GetEnumerator(0);
                while (iterator.MoveNext()) {
                    DataRow row = table.recordManager[iterator.Current];
                    if (!rowViewCache.TryGetValue(row, out drv)) {
                        drv = new DataRowView(this, row);
                    }
                    rvc.Add(row, drv);
                }
            }
            if (null != addNewRow) {
                rowViewCache.TryGetValue(addNewRow, out drv);
                Debug.Assert(null != drv, "didn't contain addNewRow");
                rvc.Add(addNewRow, drv);
            }
            Debug.Assert(rvc.Count == CountFromIndex, "didn't add expected count");
            this.rowViewCache = rvc;
        }

        internal void SetDataViewManager(DataViewManager dataViewManager) {
            if (this.table == null)
                throw ExceptionBuilder.CanNotUse();

            if (this.dataViewManager != dataViewManager) {
                if (dataViewManager != null)
                    dataViewManager.nViews--;
                this.dataViewManager = dataViewManager;
                if (dataViewManager != null) {
                    dataViewManager.nViews++;
                    DataViewSetting dataViewSetting = dataViewManager.DataViewSettings[table];
                    try {
                        // [....]: check that we will not do unnesasary operation here if dataViewSetting.Sort == this.Sort ...
                        applyDefaultSort = dataViewSetting.ApplyDefaultSort;
                        DataExpression newFilter = new DataExpression(table, dataViewSetting.RowFilter);
                        SetIndex(dataViewSetting.Sort, dataViewSetting.RowStateFilter, newFilter);
                    }
                    catch (Exception e) {
                        // 
                        if (!Common.ADP.IsCatchableExceptionType(e)) {
                            throw;
                        }
                        ExceptionBuilder.TraceExceptionWithoutRethrow(e);
                        // ignore the exception
                    }
                    locked = true;
                } else {
                    SetIndex("", DataViewRowState.CurrentRows, null);
                }
            }
        }

        internal virtual void SetIndex(string newSort, DataViewRowState newRowStates, IFilter newRowFilter) {
            SetIndex2(newSort, newRowStates, newRowFilter, true);
        }

        internal void SetIndex2(string newSort, DataViewRowState newRowStates, IFilter newRowFilter, bool fireEvent) {
            Bid.Trace("<ds.DataView.SetIndex|INFO> %d#, newSort='%ls', newRowStates=%d{ds.DataViewRowState}\n", ObjectID, newSort, (int)newRowStates);
            this.sort         = newSort;
            this.recordStates = newRowStates;
            this.rowFilter    = newRowFilter;

            Debug.Assert((0 == (DataViewRowState.ModifiedCurrent & newRowStates)) ||
                         (0 == (DataViewRowState.ModifiedOriginal & newRowStates)),
                         "asking DataViewRowState for both Original & Current records");

            if (fEndInitInProgress)
                return;

            if (fireEvent) {
                // old code path for virtual UpdateIndex
                UpdateIndex(true);
            }
            else {
                // new code path for RelatedView
                Debug.Assert(null == _comparison, "RelatedView should not have a comparison function");
                UpdateIndex(true, false);
            }

            if (null != findIndexes) {
                Dictionary<string,Index> indexes = findIndexes;
                findIndexes = null;

                foreach(KeyValuePair<string,Index> entry in indexes) {
                    entry.Value.RemoveRef();
                }
            }
        }

        protected void UpdateIndex() {
            UpdateIndex(false);
        }

        protected virtual void UpdateIndex(bool force) {
            UpdateIndex(force, true);
        }

        internal void UpdateIndex(bool force, bool fireEvent) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<ds.DataView.UpdateIndex|INFO> %d#, force=%d{bool}\n", ObjectID, force);
            try {
                if (open != shouldOpen || force) {
                    this.open = shouldOpen;
                    Index newIndex = null;
                    if (open) {
                        if (table != null) {
                            if (null != SortComparison)
                            {
                                // because an Index with with a Comparison<DataRow is not sharable, directly create the index here
                                newIndex = new Index(table, SortComparison, ((DataViewRowState)(int)recordStates), GetFilter());

                                // bump the addref from 0 to 1 to added to table index collection
                                // the bump from 1 to 2 will happen via DataViewListener.RegisterListChangedEvent
                                newIndex.AddRef();
                            }
                            else
                            {
                                newIndex = table.GetIndex(Sort, ((DataViewRowState)(int)recordStates), GetFilter());
                            }
                        }
                    }

                    if (index == newIndex) {
                        return;
                    }

                    DataTable _table = index != null ? index.Table : newIndex.Table;

                    if (index != null) {
                        this.dvListener.UnregisterListChangedEvent();
                    }

                    index = newIndex;

                    if (index != null) {
                        this.dvListener.RegisterListChangedEvent(index);
                    }

                    ResetRowViewCache();

                    if (fireEvent) {
                        OnListChanged(ResetEventArgs);
                    }
                }
            }
            finally{
                Bid.ScopeLeave(ref hscp);
            }

        }

        internal void ChildRelationCollectionChanged(object sender, CollectionChangeEventArgs e) {
            DataRelationPropertyDescriptor NullProp = null;
            OnListChanged(
                e.Action == CollectionChangeAction.Add ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, new DataRelationPropertyDescriptor((System.Data.DataRelation)e.Element)) :
                e.Action == CollectionChangeAction.Refresh ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, NullProp):
                e.Action == CollectionChangeAction.Remove ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, new DataRelationPropertyDescriptor((System.Data.DataRelation)e.Element)) :
            /*default*/ null
            );
        }

        internal void ParentRelationCollectionChanged(object sender, CollectionChangeEventArgs e) {
            DataRelationPropertyDescriptor NullProp = null;
            OnListChanged(
                e.Action == CollectionChangeAction.Add ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, new DataRelationPropertyDescriptor((System.Data.DataRelation)e.Element)) :
                e.Action == CollectionChangeAction.Refresh ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, NullProp):
                e.Action == CollectionChangeAction.Remove ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, new DataRelationPropertyDescriptor((System.Data.DataRelation)e.Element)) :
            /*default*/ null
            );
        }

        protected virtual void ColumnCollectionChanged(object sender, CollectionChangeEventArgs e) {
            DataColumnPropertyDescriptor NullProp = null;
            OnListChanged(
                e.Action == CollectionChangeAction.Add ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorAdded, new DataColumnPropertyDescriptor((System.Data.DataColumn)e.Element)) :
                e.Action == CollectionChangeAction.Refresh ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorChanged, NullProp):
                e.Action == CollectionChangeAction.Remove ? new ListChangedEventArgs(ListChangedType.PropertyDescriptorDeleted, new DataColumnPropertyDescriptor((System.Data.DataColumn)e.Element)) :
                /*default*/ null
            );
        }

        internal void ColumnCollectionChangedInternal(object sender, CollectionChangeEventArgs e) {
            ColumnCollectionChanged(sender, e);
        }

        public DataTable ToTable() {
            return ToTable(null, false, new string[0]);
        }

        public DataTable ToTable(string tableName){
            return ToTable(tableName, false, new string[0]);
        }

        public DataTable ToTable(bool distinct, params string[] columnNames){
            return ToTable(null, distinct, columnNames);
        }

        public DataTable ToTable(string tableName, bool distinct, params string[] columnNames){
            Bid.Trace("<ds.DataView.ToTable|API> %d#, TableName='%ls', distinct=%d{bool}\n", ObjectID, tableName, distinct);

            if (columnNames == null){
                throw ExceptionBuilder.ArgumentNull("columnNames");
            }

            DataTable dt = new DataTable();
            dt.Locale = this.table.Locale;
            dt.CaseSensitive = this.table.CaseSensitive;
            dt.TableName = ((null != tableName) ? tableName : this.table.TableName);
            dt.Namespace = this.table.Namespace;
            dt.Prefix = this.table.Prefix;

            if (columnNames.Length == 0) {
                columnNames = new string[Table.Columns.Count];
                for (int i = 0; i < columnNames.Length; i++) {
                    columnNames[i] = Table.Columns[i].ColumnName;
                }
            }

            int [] columnIndexes = new int[columnNames.Length];

            List<object[]> rowlist = new List<object[]>();

            for(int i = 0; i < columnNames.Length ; i++){
                DataColumn dc = Table.Columns[columnNames[i]];
                if (dc == null) {
                    throw ExceptionBuilder.ColumnNotInTheUnderlyingTable(columnNames[i], Table.TableName);
                }
                dt.Columns.Add(dc.Clone());
                columnIndexes[i] = Table.Columns.IndexOf(dc);
            }

            foreach (DataRowView drview in this) {
                object[] o = new object[columnNames.Length];

                for (int j = 0; j < columnIndexes.Length; j++) {
                    o[j] = drview[columnIndexes[j]];
                }
                if ( !distinct || !RowExist(rowlist, o)) {
                    dt.Rows.Add(o);
                    rowlist.Add(o);
                }
            }

            return dt;
        }

         private bool RowExist(List<object[]> arraylist, object[] objectArray) {
            for (int i =0 ; i < arraylist.Count ; i++){
                object[] rows = arraylist[i];
                bool retval = true;
                for (int j = 0; j < objectArray.Length; j++){
                    retval &= (rows[j].Equals(objectArray[j]));
                }
                if (retval)
                    return true;
            }
            return false;
         }

         /// <summary>
         /// If <paramref name="view"/> is equivalent to the the current view with regards to all properties.
         /// <see cref="RowFilter"/> and <see cref="Sort"/> may differ by <see cref="StringComparison.OrdinalIgnoreCase"/>.
         /// </summary>
         public virtual bool Equals(DataView view) {
             if ((null == view) ||
                this.Table != view.Table ||
                this.Count != view.Count ||
                (string.Compare(this.RowFilter, view.RowFilter, StringComparison.OrdinalIgnoreCase) != 0) ||  // case insensitive
                (string.Compare(this.Sort, view.Sort, StringComparison.OrdinalIgnoreCase) != 0) ||  // case insensitive
                !Object.ReferenceEquals(SortComparison, view.SortComparison) ||
                !Object.ReferenceEquals(RowPredicate, view.RowPredicate) ||
                this.RowStateFilter != view.RowStateFilter ||
                this.DataViewManager != view.DataViewManager||
                this.AllowDelete != view.AllowDelete||
                this.AllowNew != view.AllowNew||
                this.AllowEdit != view.AllowEdit )
                return false;
             return true;
         }

        internal int ObjectID {
            get {
                return _objectID;
            }
        }
    }
}
