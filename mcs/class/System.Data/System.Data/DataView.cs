//
// System.Data.DataView.cs
//
// Author:
//    Daniel Morgan <danmorg@sc.rr.com>
//    Tim Coleman (tim@timcoleman.com)
//    Punit Todi (punits_mailbox@yahoo.com)
//    Atsushi Enomoto <atsushi@ximian.com>
//    Konstantin Triger (kostat@mainsoft.com)
//
// Copyright (C) Daniel Morgan, 2002, 2003
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002-2003		

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Data.Common;
using System.Globalization;
using Mono.Data.SqlExpressions;
using System.Text;

namespace System.Data 
{
	/// <summary>
	/// A DataView is used in the binding of data between
	/// a DataTable and Windows Forms or Web Forms allowing
	/// a view of a DataTable for editing, filtering,
	/// navigation, searching, and sorting.
	/// </summary>
	//[Designer]
	[Editor]
	[DefaultEvent ("PositionChanged")]
	[DefaultProperty ("Table")]
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.DataViewDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
	public class DataView : MarshalByValueComponent, IBindingList, IList, ICollection, IEnumerable, ITypedList, ISupportInitialize
	{
		protected DataTable dataTable = null;
		string rowFilter = String.Empty;
		IExpression rowFilterExpr;
		string sort = String.Empty;
		DataViewRowState rowState;
		protected DataRowView[] rowCache = null;

		// BeginInit() support
		bool isInitPhase = false;
		bool inEndInit = false;
		DataTable initTable;
		bool initApplyDefaultSort;
		string initSort;
		string initRowFilter;
		DataViewRowState initRowState;

		// FIXME: what are the default values?
		bool allowNew = true; 
		bool allowEdit = true;
		bool allowDelete = true;
		bool applyDefaultSort = false;
		bool isSorted = false;

		bool isOpen = false;

		bool useDefaultSort = true;
		
		Index _index;
		internal DataRow _lastAdded = null;
		
		private DataViewManager dataViewManager = null;
		internal static ListChangedEventArgs ListResetEventArgs = new ListChangedEventArgs (ListChangedType.Reset,-1,-1);

		#region Constructors

		public DataView () 
		{
			rowState = DataViewRowState.CurrentRows;
			Open ();
		}

		public DataView (DataTable table)
			: this (table, (DataViewManager) null)
		{
		}

		internal DataView (DataTable table, DataViewManager manager)
		{
			dataTable = table;
			rowState = DataViewRowState.CurrentRows;
			dataViewManager = manager;
			Open ();
		}

		public DataView (DataTable table, string rowFilter,
			string sort, DataViewRowState rowState)
			: this (table, null, rowFilter, sort, rowState)
		{
		}

		internal DataView (DataTable table, DataViewManager manager,
			string RowFilter, string Sort, DataViewRowState RowState)
		{
			dataTable = table;
			dataViewManager = manager;
			rowState = DataViewRowState.CurrentRows;
			this.RowFilter = RowFilter;
			this.Sort = Sort;
			rowState = RowState;
			Open ();
		}
		#endregion // Constructors

		#region PublicProperties

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether this DataView and the user interface associated with it allows deletes.")]
		[DefaultValue (true)]
		public bool AllowDelete {
			get {
				return allowDelete;
			}
			set {
				allowDelete = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether this DataView and the user interface associated with it allows edits.")]
		[DefaultValue (true)]
		public bool AllowEdit {
			get {
				return allowEdit;
			}
			set {
				allowEdit = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether this DataView and the user interface associated with it allows new rows to be added.")]
		[DefaultValue (true)]
		public bool AllowNew {
			get {
				return allowNew;
			}
			
			set {
				allowNew = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether to use the default sort if the Sort property is not set.")]
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.All)]
		public bool ApplyDefaultSort {
			get { return applyDefaultSort; }
			set {
				if (isInitPhase) {
					initApplyDefaultSort = value;
					return;
				}
				if (applyDefaultSort == value)
					return;

				applyDefaultSort = value;
				if (applyDefaultSort == true &&
					(sort == null || sort == string.Empty))
					PopulateDefaultSort ();
				if (!inEndInit)
					UpdateIndex (true);
			}
		}
		// get the count of rows in the DataView after RowFilter 
		// and RowStateFilter have been applied
		[Browsable (false)]
		[DataSysDescription ("Returns the number of items currently in this view.")]
		public int Count {
			[MonoTODO]
			get {
				return rowCache.Length;
			}
		}

		[Browsable (false)]
		[DataSysDescription ("This returns a pointer to back to the DataViewManager that owns this DataSet (if any).")]
		public DataViewManager DataViewManager {
			[MonoTODO]
			get {
				return dataViewManager;
			}
		}

		// Item indexer
		// the compiler creates a DefaultMemeberAttribute from
		// this IndexerNameAttribute
		[System.Runtime.CompilerServices.IndexerName("Item")]
		public DataRowView this[int recordIndex] {
			[MonoTODO]
			get {
				return rowCache [recordIndex];
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates an expression used to filter the data returned by this DataView.")]
		[DefaultValue ("")]
		public virtual string RowFilter {
			get { return rowFilter; }
			[MonoTODO]
			set {
				if (value == null)
					value = String.Empty;
				if (isInitPhase) {
					initRowFilter = value;
					return;
				}

				CultureInfo info = (Table != null) ? Table.Locale : CultureInfo.CurrentCulture;
				if (String.Compare(rowFilter, value, false, info) == 0)
					return;

				if (value == String.Empty) 
					rowFilterExpr = null;
				else {
					Parser parser = new Parser ();
					rowFilterExpr = parser.Compile (value);
				}
				rowFilter = value;
				if (!inEndInit)
					UpdateIndex (true);
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the versions of data returned by this DataView.")]
		[DefaultValue (DataViewRowState.CurrentRows)]
		public DataViewRowState RowStateFilter {
			get { return rowState; }
			set {
				if (isInitPhase) {
					initRowState = value;
					return;
				}

				if (value == rowState)
					return;

				rowState = value;
				if (!inEndInit)
					UpdateIndex (true);
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the order in which data is returned by this DataView.")]
		[DefaultValue ("")]
		public string Sort {
			get { return sort; }
			set {
				if (isInitPhase) {
					initSort = value;
					return;
				}
				if (value == sort)
					return;

				if (value == null) {	
				/* if given value is null useDefaultSort */
					useDefaultSort = true;
					/* if ApplyDefault sort is true try appling it */
					if (ApplyDefaultSort == true)
						PopulateDefaultSort ();
				}
				else {	
					/* else donot useDefaultSort. set it as false */
					/* sort is set to value specified */
					useDefaultSort = false;
					sort = value;
					//sortedColumns = SortableColumn.ParseSortString (dataTable, value, true);
				}
				if (!inEndInit)
					UpdateIndex (true);
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the table this DataView uses to get data.")]
		[DefaultValue (null)]
		[RefreshProperties (RefreshProperties.All)]
		public DataTable Table {
			get { return dataTable; }
			set {
				if (isInitPhase) {
					initTable = value;
					return;
				}

				if (value != null && value.TableName.Equals("")) {
					throw new DataException("Cannot bind to DataTable with no name.");
				}

				if (dataTable != null) {
					UnregisterEventHandlers ();
				}

				dataTable = value;

				if (dataTable != null) {
					RegisterEventHandlers ();
					OnListChanged (new ListChangedEventArgs (ListChangedType.PropertyDescriptorChanged, 0, 0));
					sort = null;
					rowFilter = null;
					rowFilterExpr = null;
					if (!inEndInit)
						UpdateIndex (true);
				}
			}
		}

		#endregion // PublicProperties
		
		#region	PublicMethods

		[MonoTODO]
		public virtual DataRowView AddNew() 
		{
			if (!IsOpen)
				throw new DataException ("DataView is not open.");
			if (!AllowNew)
				throw new DataException ("Cannot call AddNew on a DataView where AllowNew is false.");
			
			if (_lastAdded != null) {
				// FIXME : finish last added
				CompleteLastAdded(true);
			}

			_lastAdded = dataTable.NewRow ();
			UpdateIndex(true);
			OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, Count - 1, -1));
			 
			return this[Count - 1];
		}

		internal void CompleteLastAdded(bool add)
		{
			DataRow dr = _lastAdded;

			if (add) {
				try {
					dataTable.Rows.Add(_lastAdded);
					OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, Count - 1, -1));
					_lastAdded = null;
				}
				catch(Exception e) {
					_lastAdded = dr;
					throw e;
				}
			}
			else {
				_lastAdded.CancelEdit();
				_lastAdded = null;
				OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, Count - 1));
			}
		}

		[MonoTODO]
		public void BeginInit() 
		{
			initTable = Table;
			initApplyDefaultSort = ApplyDefaultSort;
			initSort = Sort;
			initRowFilter = RowFilter;
			initRowState = RowStateFilter;

			isInitPhase = true;
		}

		[MonoTODO]
		public void CopyTo (Array array, int index) 
		{
			if (index + rowCache.Length > array.Length) {
				throw new IndexOutOfRangeException();
			}

			int row = 0;
			for (; row < rowCache.Length && row < array.Length; row++) {
				array.SetValue (rowCache[row], index + row);
			}
		}

		public void Delete(int index) 
		{
			if (!IsOpen)
				throw new DataException ("DataView is not open.");

			if (_lastAdded != null && index == Count) {
				CompleteLastAdded(false);
				return;
			}

			if (!AllowDelete)
				throw new DataException ("Cannot delete on a DataSource where AllowDelete is false.");
			
			if (index > rowCache.Length)
				throw new IndexOutOfRangeException ("There is no row at " +
						"position: " + index + ".");
			DataRowView row = rowCache [index];
			row.Row.Delete();
		}

#if NET_2_0
		[MonoTODO]
		public virtual bool Equals (DataView dv)
		{
			throw new NotImplementedException ();
		}
#endif

		[MonoTODO]
		public void EndInit() 
		{
			isInitPhase = false;

			inEndInit = true;

			Table = initTable;
			ApplyDefaultSort = initApplyDefaultSort;
			Sort = initSort;
			RowFilter = initRowFilter;
			RowStateFilter = initRowState;

			inEndInit = false;

			UpdateIndex (true);
		}

		[MonoTODO]
		public int Find(object key) 
		{
			object [] keys = new object[] { key };
			return Find(keys);
		}
		
		[MonoTODO]
		public int Find(object[] keys) 
		{
			if (sort == null || sort == string.Empty) {
				throw new ArgumentException ("Find finds a row based on a Sort order, and no Sort order is specified");
			}

			if (Index == null) {
				UpdateIndex(true);
			}

			int index = -1; 				
			try {
				index = Index.FindIndex(keys);
			}
			catch(FormatException) {
				// suppress exception
			}
			catch(InvalidCastException) {
				// suppress exception
			}
			return index;
		}
		
		[MonoTODO]
		public DataRowView[] FindRows(object key) 
		{
			return FindRows(new object[] {key});
		}

		[MonoTODO]
		public DataRowView[] FindRows(object[] keys) 
		{
			if (sort == null || sort == string.Empty) {
				throw new ArgumentException ("Find finds a row based on a Sort order, and no Sort order is specified");
			}

			if (Index == null) {
				UpdateIndex(true);
			}

			int[] indexes = Index.FindAllIndexes(keys);

			DataRowView[] rowViewArr = new DataRowView[indexes.Length];			
			for (int r = 0; r < indexes.Length; r++) {
				rowViewArr[r] = rowCache[indexes[r]];
			}
			return rowViewArr;
		}

		public IEnumerator GetEnumerator() 
		{
			DataRowView[] dataRowViews = new DataRowView[Count];
			CopyTo(dataRowViews,0);
			return dataRowViews.GetEnumerator();
		}

		#endregion // PublicMethods
		
		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the data returned by this DataView has somehow changed.")]
		public event ListChangedEventHandler ListChanged;

		[Browsable (false)]
		[DataSysDescription ("Indicates whether the view is open.")]
		protected bool IsOpen {
			get { return isOpen; }
		}

		internal Index Index
		{
			get {
				return _index;
			}

			set {
				if (_index != null) {
					_index.RemoveRef();
					Table.DropIndex(_index);
				}

				_index = value;

				if (_index != null) {
					_index.AddRef();
				}
			}
		}

		protected void Close ()
		{
			if (dataTable != null)
				UnregisterEventHandlers ();
			Index = null;
			rowCache = null;
			isOpen = false;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				Close ();

			base.Dispose (disposing);
		}

		protected virtual void IndexListChanged (
			object sender, ListChangedEventArgs e)
		{
		}

		[MonoTODO]
		protected virtual void OnListChanged(ListChangedEventArgs e) 
		{
			// Yes, under MS.NET, when it is overriden, the 
			// events are not fired (even if it is essential
			// to internal processing).
			try {
			if (ListChanged != null)
				ListChanged (this, e);
			} catch {
			}
		}

		internal void ChangedList(ListChangedType listChangedType, int newIndex,int oldIndex)
		{
			ListChangedEventArgs e = new ListChangedEventArgs(listChangedType,newIndex,oldIndex);
			OnListChanged(e);
		}

		[MonoTODO]
		protected void Open() 
		{
			// I wonder if this comment is still valid, but keep
			// in the meantime.

			// FIXME: create the initial index cache to the DataTable, and 
			//        only refresh the index when the DataTable
			//        has changes via column, row, or constraint
			//        changed events. the index cache is generally
			//        a DataViewRow array that points to the actual
			//        DataRows in the this DataTable's DataRowCollection;
			//        this index is really a cache that gets 
			//        created during Open(), gets Updated 
			//        when various properties of this view
			//        changes, gets Updated when this DataTable's 
			//        row, column, or constraint collections have changed.
			//        I'm not sure what else.
			//        The data view will know one of the DataTable's
			//        collections have changed via one of 
			//        its changed events.
			//        Otherwise, if getting a/the DataRowView(s),
			//        Count, or other properties, then just use the
			//        index cache.
			//		dataTable.ColumnChanged  += new DataColumnChangeEventHandler(OnColumnChanged);
			
			UpdateIndex (true);
			if (dataTable != null) {
				RegisterEventHandlers();
			}
			isOpen = true;
		}
		
		private void RegisterEventHandlers()
		{
			//dataTable.ColumnChanging += new DataColumnChangeEventHandler(OnColumnChanging);
			dataTable.ColumnChanged  += new DataColumnChangeEventHandler(OnColumnChanged);
			dataTable.RowChanged     += new DataRowChangeEventHandler(OnRowChanged);
			//dataTable.RowDeleting    += new DataRowChangeEventHandler(OnRowDeleting);
			dataTable.RowDeleted     += new DataRowChangeEventHandler(OnRowDeleted);
			dataTable.Columns.CollectionChanged += new CollectionChangeEventHandler(ColumnCollectionChanged);
			dataTable.Constraints.CollectionChanged += new CollectionChangeEventHandler(OnConstraintCollectionChanged);
		}

		private void UnregisterEventHandlers()
		{
//			dataTable.ColumnChanging -= new DataColumnChangeEventHandler(OnColumnChanging);
			dataTable.ColumnChanged  -= new DataColumnChangeEventHandler(OnColumnChanged);
			dataTable.RowChanged     -= new DataRowChangeEventHandler(OnRowChanged);
//			dataTable.RowDeleting    -= new DataRowChangeEventHandler(OnRowDeleting);
			dataTable.RowDeleted     -= new DataRowChangeEventHandler(OnRowDeleted);
			dataTable.Columns.CollectionChanged -= new CollectionChangeEventHandler(ColumnCollectionChanged);
			dataTable.Constraints.CollectionChanged -= new CollectionChangeEventHandler(OnConstraintCollectionChanged);
		}

		// These index storing and rowView preservation must be done
		// before the actual row value is changed; thus we can't use
		// RowChanging which accepts "already modified" DataRow.

		private void OnColumnChanged(object sender, DataColumnChangeEventArgs args)
		{	/* not used */
			//UpdateIndex(true);
		}
		
		private void OnRowChanged(object sender, DataRowChangeEventArgs args)
		{
			int oldIndex,newIndex;
			oldIndex = newIndex = -1;
			oldIndex = IndexOf (args.Row);
			UpdateIndex (true);
			newIndex = IndexOf (args.Row);

			/* ItemAdded */
			if(args.Action == DataRowAction.Add)
			{
				OnListChanged (new ListChangedEventArgs (ListChangedType.ItemAdded, newIndex, -1));
			}
				
			/* ItemChanged or ItemMoved */
			if (args.Action == DataRowAction.Change) {
					if (oldIndex == newIndex)
						OnListChanged (new ListChangedEventArgs (ListChangedType.ItemChanged, newIndex, -1));
					else
						OnListChanged (new ListChangedEventArgs (ListChangedType.ItemMoved, newIndex, oldIndex));
				}
			}

		private void OnRowDeleted (object sender, DataRowChangeEventArgs args)
		{
			/* ItemDeleted */
			int newIndex;
			newIndex = IndexOf (args.Row);
			UpdateIndex (true);
			OnListChanged (new ListChangedEventArgs (ListChangedType.ItemDeleted, newIndex, -1));
		}
		
		protected virtual void ColumnCollectionChanged (object sender, CollectionChangeEventArgs args)
		{
			// UpdateIndex() is not invoked here (even if the sort
			// column is being removed).

			/* PropertyDescriptor Add */
			if (args.Action == CollectionChangeAction.Add)
				OnListChanged (new ListChangedEventArgs (ListChangedType.PropertyDescriptorAdded,0,0));
			/* PropertyDescriptor Removed */
			if (args.Action == CollectionChangeAction.Remove)
				OnListChanged (new ListChangedEventArgs (ListChangedType.PropertyDescriptorDeleted,0,0));
			/* FIXME: PropertyDescriptor Changed ???*/
			if (args.Action == CollectionChangeAction.Refresh)
				OnListChanged (new ListChangedEventArgs (ListChangedType.PropertyDescriptorChanged,0,0));
		}
		private void OnConstraintCollectionChanged(object sender, CollectionChangeEventArgs args)
		{
			//	The Sort variable is set to the UniqueConstraint column.
			//  if ApplyDefault Sort is true and Sort is null or is not set Explicitly
			
			// FIXME: The interal cache may change as result of change in Constraint collection
			// one such scenerio is taken care.
			// There may be more. I dont know what else can be done.
			/* useDefaultSort is set to false when Sort is set explicitly */
			if (args.Action == CollectionChangeAction.Add && args.Element is UniqueConstraint) {
				if (ApplyDefaultSort == true && useDefaultSort == true)
					PopulateDefaultSort ((UniqueConstraint) args.Element);
			}
			// UpdateIndex() is not invoked here.
		}

		// internal use by Mono
		protected void Reset() 
		{
			// TODO: what really happens?
			Close ();
			rowCache = null;
			Open ();
			OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, -1 ));
		}

#if NET_2_0
		[MonoTODO]
		public DataTable ToTable ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataTable ToTable (bool isDistinct, string[] columnNames)
		{
			throw new NotImplementedException ();
		}
#endif
		protected void UpdateIndex() {
			UpdateIndex(false);
		}

		// This is method is internal to 
		// the Mono implementation of DataView; it
		// is not to be used from your code.
		//
		// Update the DataRowView array which is an index cache
		// into the DataTable's DataRowCollection.
		//
		// I assume this is what UpdateIndex is used for
		protected virtual void UpdateIndex(bool force) 
		{
			if (Table == null) {
				// FIXME
				return;
			}

			if (Index == null || force) {
				ListSortDirection[] sortOrder = null;
				DataColumn[] columns = DataTable.ParseSortString(Table, Sort, out sortOrder, false);
				Index = dataTable.GetIndex(columns,sortOrder,RowStateFilter,FilterExpression,true);
			}
			else {
				Index.Key.RowStateFilter = RowStateFilter;
				Index.Reset();
			}

			int[] records = Index.GetAll();

			if (records != null) {
				InitDataRowViewArray(records,Index.Size);
			}
			else {
				rowCache = new DataRowView[0];
			}

			OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, -1, -1));
		}

		internal virtual IExpression FilterExpression
		{
			get {
				return rowFilterExpr;
			}
		}

		private void InitDataRowViewArray(int[] records,int size) 
		{
			if (_lastAdded != null) {
				rowCache = new DataRowView[size + 1];	
			}
			else {
				rowCache = new DataRowView[size];			
			}

			for (int r = 0; r < size; r++) {
				rowCache[r] = new DataRowView (this, Table.RecordCache[records[r]],r);
			}

			if(_lastAdded != null) {
				rowCache[size] = new DataRowView(this,_lastAdded,size);
			}
		}

		[MonoTODO]
		PropertyDescriptorCollection ITypedList.GetItemProperties (PropertyDescriptor[] listAccessors) 
		{
			// FIXME: use listAccessors somehow
			DataColumnPropertyDescriptor [] descriptors = 
				new DataColumnPropertyDescriptor [dataTable.Columns.Count];

			DataColumnPropertyDescriptor descriptor;
			DataColumn dataColumn;
			for (int col = 0; col < dataTable.Columns.Count; col ++) {
				dataColumn = dataTable.Columns[col];
				descriptor = new DataColumnPropertyDescriptor(
					dataColumn.ColumnName, col, null);
				descriptor.SetComponentType (typeof (System.Data.DataRowView));
				descriptor.SetPropertyType (dataColumn.DataType);
				descriptor.SetReadOnly (dataColumn.ReadOnly);
				descriptors [col] = descriptor;
			}
			return new PropertyDescriptorCollection (descriptors);
		}

		[MonoTODO]
		string ITypedList.GetListName (PropertyDescriptor[] listAccessors) 
		{
			return "";
		}

		bool ICollection.IsSynchronized { 
			[MonoTODO]
			get {
				return false;
			} 
		}

		object ICollection.SyncRoot { 
			[MonoTODO]
			get {
				// FIXME:
				return this;
			}
		}

		bool IList.IsFixedSize {
			[MonoTODO]
			get {
				return false;
			}
		}
		
		bool IList.IsReadOnly {
			[MonoTODO]
			get {
				return false;
			}
		}

		object IList.this[int recordIndex] {
			[MonoTODO]
			get {
				return this[recordIndex];
			}

			[MonoTODO]
			set{
				throw new InvalidOperationException();
			}
		}

		int IList.Add (object value) 
		{
			throw new ArgumentException ("Cannot add external objects to this list.");
		}

		void IList.Clear () 
		{
			throw new ArgumentException ("Cannot clear this list.");
		}

		bool IList.Contains (object value) 
		{
			DataRowView drv = value as DataRowView;
			if (drv == null)
				return false;

			return drv.DataView == this;
		}

		int IList.IndexOf (object value) 
		{
			DataRowView drv = value as DataRowView;
			if (drv != null && drv.DataView == this) {
				return drv.Index;
			}

			return -1;
		}

		void IList.Insert(int index,object value) 
		{
			throw new ArgumentException ("Cannot insert external objects to this list.");
		}

		void IList.Remove(object value) 
		{
			DataRowView drv = value as DataRowView;
			if (drv != null && drv.DataView == this) {
				((IList)this).RemoveAt(drv.Index);
			}

			throw new ArgumentException ("Cannot remove external objects to this list.");
		}

		void IList.RemoveAt(int index) 
		{
			Delete(index);
		}

		#region IBindingList implementation

		[MonoTODO]
		void IBindingList.AddIndex (PropertyDescriptor property) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		object IBindingList.AddNew () 
		{
			return this.AddNew ();
		}

		[MonoTODO]
		void IBindingList.ApplySort (PropertyDescriptor property, ListSortDirection direction) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IBindingList.Find (PropertyDescriptor property, object key) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IBindingList.RemoveIndex (PropertyDescriptor property) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IBindingList.RemoveSort () 
		{
			throw new NotImplementedException ();
		}
		
		bool IBindingList.AllowEdit {
			[MonoTODO]
			get {
				return AllowEdit;
			}
		}

		bool IBindingList.AllowNew {
			[MonoTODO]
			get {
				return AllowNew;
			}
		}

		bool IBindingList.AllowRemove {
			[MonoTODO]
			get {
				return AllowDelete;
			}
		}

		bool IBindingList.IsSorted {
			[MonoTODO]
			get {
				return isSorted;
			}
		}

		ListSortDirection IBindingList.SortDirection {
			[MonoTODO]
			get {
				// FIXME: 
				return ListSortDirection.Ascending;
			}
		}

		PropertyDescriptor IBindingList.SortProperty {
			[MonoTODO]
			get {
				// FIXME:
				return null;
			}
		}

		bool IBindingList.SupportsChangeNotification {
			[MonoTODO]
			get {
				return false;
			}
		}

		bool IBindingList.SupportsSearching {
			[MonoTODO]
			get {
				return false;
			}
		}

		bool IBindingList.SupportsSorting {
			[MonoTODO]
			get {
				return false;
			}
		}

		#endregion // IBindingList implementation
		private int IndexOf(DataRow dr)
		{
			for (int i=0; i < rowCache.Length; i++)
				if (dr.Equals (rowCache [i].Row))
				return i;
			return -1;
		}
		
		private void PopulateDefaultSort () {
			sort = "";
			foreach (Constraint c in dataTable.Constraints) {
				if (c is UniqueConstraint) {
					PopulateDefaultSort ((UniqueConstraint) c);
					break;
				}
			}
		}

		private void PopulateDefaultSort (UniqueConstraint uc) {
			if (isInitPhase)
				return;

			DataColumn[] columns = uc.Columns;
			if (columns.Length == 0) {
				sort = String.Empty;
				return;
			}

			StringBuilder builder = new StringBuilder();
			builder.Append(columns[0].ColumnName);
			for (int i = 1; i<columns.Length; i++) {
				builder.Append(", ");
				builder.Append(columns[i].ColumnName);
			}
			sort = builder.ToString();
		}
		
		// FIXME : complete the implementation
		internal DataView CreateChildView (DataRelation relation,object[] keyValues)
		{
			if (relation == null || relation.ParentTable != Table) {
				throw new ArgumentException("The relation is not parented to the table to which this DataView points.");
			}
			return new RelatedDataView(relation.ChildColumns,keyValues);
		}

		// FIXME : complete the implementation
		internal DataView CreateChildView (string name,object[] keyValues)
		{
			DataRelation relation = Table.ChildRelations[name];

			if (relation != null) {
				return CreateChildView(relation,keyValues);
			}

			throw new ArgumentException("Relation " + name + " not found in the table");
		}

		private int GetRecord(int index) {
			if (index < 0 || index >= Count)
				throw new IndexOutOfRangeException(String.Format("There is no row at position {0}.", index));

			return(index == Index.Size) ?
				_lastAdded.IndexFromVersion(DataRowVersion.Default) :
				Index.IndexToRecord(index);
		}

		internal DataRowVersion GetRowVersion(int index) {
			int record = GetRecord(index);
			return Table.RecordCache[record].VersionFromIndex(record);
		}
	}
}
