//
// System.Data.DataView.cs
//
// Author:
//    Daniel Morgan <danmorg@sc.rr.com>
//    Tim Coleman (tim@timcoleman.com)
//    Punit Todi (punits_mailbox@yahoo.com)
// Copyright (C) Daniel Morgan, 2002, 2003
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002-2003		

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Mono.Data.SqlExpressions;

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
		DataTable dataTable = null;
		string rowFilter = "";
		IExpression rowFilterExpr;
		string sort = "";
		SortableColumn [] sortedColumns = null;
		DataViewRowState rowState;
		DataRowView[] rowCache = new DataRowView[0];
		// DataRow -> DataRowView
		Hashtable addNewCache = new Hashtable ();
		Hashtable rowViewPool = new Hashtable ();

		bool allowNew = true; 
		bool allowEdit = true;
		bool allowDelete = true;
		bool applyDefaultSort = false;
		bool isSorted = false;

		bool isOpen = false;

		bool bInit = false;
		bool useDefaultSort = true;
		
		DataViewManager dataViewManager = null;
		#region Constructors
		public DataView () 
		{
			rowState = DataViewRowState.CurrentRows;
			Open ();
		}

		public DataView (DataTable table)
			: this (table, null)
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
			Open();
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
			[MonoTODO]
			get {				
				return applyDefaultSort;
			}
			
			[MonoTODO]
			set {
				if (applyDefaultSort == value)
					return;

				applyDefaultSort = value;
				if (applyDefaultSort == true && (sort == null || sort == string.Empty)) {
					foreach (Constraint c in dataTable.Constraints)	{
						if (c is UniqueConstraint) {
							// FIXME: Compute SortableColumns[] directly.
							Sort = GetSortString ((UniqueConstraint) c);
							break;
						}
					}
				}
				UpdateIndex (true);
				OnListChanged (new ListChangedEventArgs (ListChangedType.Reset,-1,-1));
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
			[MonoTODO]
			get {
				return rowFilter;
			}
			
			[MonoTODO]
			set {
				if (value == null)
					value = String.Empty;
				if (rowFilter == value)
					return;
				if (value == String.Empty) 
					rowFilterExpr = null;
				else {
					Parser parser = new Parser ();
					rowFilterExpr = parser.Compile (value);
				}
				rowFilter = value;
				UpdateIndex (true);
				OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, - 1, -1));
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the versions of data returned by this DataView.")]
		[DefaultValue (DataViewRowState.CurrentRows)]
		public DataViewRowState RowStateFilter {
			[MonoTODO]
			get {
				return rowState;
			}
			
			[MonoTODO]
			set {
				if (value == rowState)
					return;
				rowState = value;
				UpdateIndex (true);
				OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, - 1, -1));
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the order in which data is returned by this DataView.")]
		[DefaultValue ("")]
		public string Sort {
			[MonoTODO]
			get {
				return sort;
			}
			
			[MonoTODO]
			set {
				if (value == sort)
					return;

				if (value == null) {	
				/* if given value is null useDefaultSort */
					useDefaultSort = true;
					/* if ApplyDefault sort is true try appling it */
					if (ApplyDefaultSort == true) {
						foreach (Constraint c in dataTable.Constraints) {
							if (c is UniqueConstraint) {
								// FIXME: Compute SortableColumns[] directly.
								Sort = GetSortString ((UniqueConstraint)c);
								break;
							}
						}
						
					}
				}
				else {	
					/* else donot useDefaultSort. set it as false */
					/* sort is set to value specified */
					useDefaultSort = false;
					sort = value;
					sortedColumns = SortableColumn.ParseSortString (dataTable, value, true);
				}
				UpdateIndex (true);
				OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, - 1, -1));
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the table this DataView uses to get data.")]
		[DefaultValue (null)]
		[RefreshProperties (RefreshProperties.All)]
		[TypeConverter (typeof (DataTableTypeConverter))]
		public DataTable Table {
			[MonoTODO]
			get {
				return dataTable;
			}
			
			[MonoTODO]
			set {
				if (value != null && value.TableName.Equals("")) {
					throw new DataException("Cannot bind to DataTable with no name.");
				}

				if (dataTable != null) {
					UnregisterEventHandlers();
				}

				dataTable = value;

				if (dataTable != null) {
					RegisterEventHandlers();
					UpdateIndex (true);
					OnListChanged (new ListChangedEventArgs (ListChangedType.Reset, - 1, -1));
				}
			}
		}
		#endregion // PublicProperties
		
		#region	PublicMethods
		public virtual DataRowView AddNew() 
		{
			if (!IsOpen)
				throw new DataException ("DataView is not open.");
			if (!AllowNew)
				throw new DataException ("Cannot call AddNew on a DataView where AllowNew is false.");
			
			DataRow row = dataTable.NewRow ();
			if (row == null)
				throw new SystemException ("Row not created");
			DataRowView rowView = new DataRowView (this, row, true);
			addNewCache.Add (row, rowView);
			rowViewPool.Add (row, rowView);

			// Add to the end of the list (i.e. recreate rowCache),
			// regardless of Sort property.
			DataRowView [] newCache = new DataRowView [rowCache.Length + 1];
			rowCache.CopyTo (newCache, 0);
			newCache [newCache.Length - 1] = rowView;
			rowCache = newCache;

			// DataRowView is added, but DataRow is still Detached.
			OnListChanged (new ListChangedEventArgs (ListChangedType.ItemAdded, newCache.Length - 1, -1));
			return rowView;
		}

		[MonoTODO]
		public void BeginInit() 
		{
			bInit = true; 
			// FIXME:
		}

		[MonoTODO]
		public void CopyTo (Array array, int index) 
		{
			rowCache.CopyTo (array, index);
		}

		public void Delete(int index) 
		{
			if (!IsOpen)
				throw new DataException ("DataView is not open.");
			if (!AllowDelete)
				throw new DataException ("Cannot delete on a DataSource where AllowDelete is false.");
			
			if (index > rowCache.Length)
				throw new IndexOutOfRangeException ("There is no row at " +
						"position: " + index + ".");
			DataRowView row = rowCache [index];
			row.Row.Delete ();
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
			bInit = false;
			// FIXME:
		}

		internal void CancelEditRowView (DataRowView rowView)
		{
			addNewCache.Remove (rowView.Row);
			rowViewPool.Remove (rowView.Row);
			// FIXME: it should not be required. MS does not do it.
			UpdateIndex ();
			rowView.Row.CancelEdit ();
		}

		internal void DeleteRowView (DataRowView rowView)
		{
			addNewCache.Remove (rowView.Row);
			rowViewPool.Remove (rowView.Row);
			// FIXME: it should not be required. MS does not do it.
			UpdateIndex ();
			rowView.Row.Delete ();
		}

		internal void EndEditRowView (DataRowView rowView)
		{
			Table.Rows.Add (rowView.Row);
			addNewCache.Remove (rowView.Row);
			rowView.Row.EndEdit ();
		}

		public int Find(object key) 
		{
			object [] keys = new object[1];
			keys[0] = key;
			return Find(keys);
		}
		
		public int Find (object[] key) 
		{
			int index; 
			if (sort == null || sort == string.Empty)
				throw new ArgumentException ("Find finds a row based on a Sort order, and no Sort order is specified");
			else {
				// FIXME: maybe some of those thecks could be removed.
				if (sortedColumns == null)
					throw new SystemException ("sort expression result is null");
				if (sortedColumns.Length == 0)
					throw new SystemException ("sort expression result is 0");
				if (sortedColumns.Length != key.Length)
					throw new ArgumentException ("Expecting " + sortedColumns.Length +
						" value(s) from the key being indexed, but recieved "+
						key.Length+" value(s).");
				RowViewComparer rowComparer = new RowViewComparer (dataTable,sortedColumns);
				int searchResult = Array.BinarySearch (rowCache,key,rowComparer);
				if (searchResult < 0)
					return -1;
				else
					return searchResult;
			}
		}

		public DataRowView[] FindRows (object key) 
		{
			return FindRows (new object [] {key});
		}

		public DataRowView[] FindRows (object[] key) 
		{
			if (Sort == String.Empty)
				throw new ArgumentException ("Find method depends on an explicit Sort property value.");
			if (sortedColumns.Length != key.Length)
				throw new ArgumentException (String.Format ("Expecting {0} keys being indexed based on Sort property, but got {1} keys.", sortedColumns.Length, key.Length));

			IExpression [] compExpr = new IExpression [sortedColumns.Length];
			for (int i = 0; i < sortedColumns.Length; i++)
				compExpr [i] = new Comparison (Operation.EQ,
					new ColumnReference (sortedColumns [i].Column.ColumnName),
					new Literal (key [i]));

			// Find first match.
			int r = 0;
			for (; r < rowCache.Length; r++) {
				if (!compExpr [0].EvalBoolean (rowCache [r].Row))
					continue;
				break;
			}
			if (r == rowCache.Length) // no match
				return new DataRowView [0];

			bool finish = false;
			int start = r;
			// Find first no-match from here.
			for (; r < rowCache.Length; r++) {
				for (int c = 0; c < key.Length; c++) {
					if (!compExpr [c].EvalBoolean (rowCache [r].Row)) {
						finish = true;
						break;
					}
				}
				if (finish)
					break;
			}

			DataRowView [] ret = new DataRowView [r - start];
			for (int i = 0; i < ret.Length; i++)
			ret [i] = rowCache [start + i];

			return ret;
		}

		public IEnumerator GetEnumerator() 
		{
			return new DataViewEnumerator (rowCache);
		}
		#endregion // PublicMethods
		
		[MonoTODO]
		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the data returned by this DataView has somehow changed.")]
		public event ListChangedEventHandler ListChanged;

		[Browsable (false)]
		[DataSysDescription ("Indicates whether the view is open.")]
		protected bool IsOpen {
			[MonoTODO]
			get {
				return isOpen;
			}
		}

		[MonoTODO]
		protected void Close() 
		{
			if (dataTable != null)
				UnregisterEventHandlers ();
			rowCache = new DataRowView [0];
			isOpen = false;
		}

		protected virtual void ColumnCollectionChanged (
			object sender, CollectionChangeEventArgs e)
		{
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

		protected virtual void OnListChanged (ListChangedEventArgs e) 
		{
			// Yes, under MS.NET, when it is overriden, the 
			// events are not fired (even if it is essential
			// to internal processing).
			if (ListChanged != null)
				ListChanged (this, e);
		}

		[MonoTODO]
		protected void Open() 
		{
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
			if (dataTable != null) {
				RegisterEventHandlers();
				UpdateIndex (true);
			}
			isOpen = true;
		}
		
		private void RegisterEventHandlers()
		{
			dataTable.RowChanged     += new DataRowChangeEventHandler(OnRowChanged);
			dataTable.RowDeleted     += new DataRowChangeEventHandler(OnRowDeleted);
			dataTable.Columns.CollectionChanged += new CollectionChangeEventHandler(OnColumnCollectionChanged);
			dataTable.Constraints.CollectionChanged += new CollectionChangeEventHandler(OnConstraintCollectionChanged);
		}

		private void UnregisterEventHandlers()
		{
			dataTable.RowChanged     -= new DataRowChangeEventHandler(OnRowChanged);
			dataTable.RowDeleted     -= new DataRowChangeEventHandler(OnRowDeleted);
			dataTable.Columns.CollectionChanged -= new CollectionChangeEventHandler(OnColumnCollectionChanged);
			dataTable.Constraints.CollectionChanged -= new CollectionChangeEventHandler(OnConstraintCollectionChanged);
		}
		
		private void OnRowChanged(object sender, DataRowChangeEventArgs args)
		{
			int oldIndex,newIndex;
			oldIndex = newIndex = -1;
			oldIndex = IndexOf (args.Row);
			// FIXME: it should not be required. MS does not do it.
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
			// FIXME: it should not be required. MS does not do it.
			UpdateIndex (true);
			OnListChanged (new ListChangedEventArgs (ListChangedType.ItemDeleted, newIndex, -1));
		}
		
		private void OnColumnCollectionChanged (object sender, CollectionChangeEventArgs args)
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
						Sort = GetSortString ((UniqueConstraint) args.Element);
			}

			// UpdateIndex() is not invoked here.

			/* ItemReset */
			OnListChanged (new ListChangedEventArgs (ListChangedType.Reset,-1,-1));
		}

		// internal use by Mono
		protected void Reset() 
		{
			// TODO: what really happens?
			Close ();
			rowCache = new DataRowView[0];
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

		// internal use by Mono
		protected void UpdateIndex () 
		{
			UpdateIndex (false);
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
			DataRowView[] newRowCache = null;
			DataRow[] rows = null;

			// I guess, "force" parameter is used to indicate
			// whether we should "query" against DataTable.
			// For example, when adding a new row, we don't have
			// to re-query.

			// Handle sort by itself, considering AddNew rows.
			rows = dataTable.Select (rowFilterExpr, null, RowStateFilter);
			DataRow [] tmp = new DataRow [rows.Length + addNewCache.Count];
			rows.CopyTo (tmp, 0);
			addNewCache.Keys.CopyTo (tmp, rows.Length);
			rows = tmp;
			if (sortedColumns != null)
				new DataTable.RowSorter (dataTable, sortedColumns).SortRows (rows);

			newRowCache = new DataRowView [rows.Length];
			Hashtable newPool = rowViewPool.Count > 0 ? new Hashtable (rows.Length + 2) : rowViewPool;
			for (int r = 0; r < rows.Length; r++) {
				DataRow dr = rows [r];
				DataRowView rv = (DataRowView) rowViewPool [dr];
				if (rv == null) {
					rv = new DataRowView (this, dr);
					newPool.Add (dr, rv);
				}
				newRowCache[r] = rv;
			}
			rowViewPool = newPool;
			rowCache = newRowCache;
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

		//int ICollection.Count { 
		//	get {
		//		return Count;
		//	} 
		//}

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

		//void ICollection.CopyTo (Array array, int index) 
		//{
		//	CopyTo (array, index);
		//}

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

		[MonoTODO]
		int IList.Add (object value) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.Clear () 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		bool IList.Contains (object value) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		int IList.IndexOf (object value) 
		{
			throw new NotImplementedException ();
		}
			
		[MonoTODO]
		void IList.Insert(int index,object value) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.Remove(object value) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void IList.RemoveAt(int index) 
		{
			throw new NotImplementedException ();
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
		
		private string GetSortString (UniqueConstraint uc)
		{
			string sortKey = null;
			foreach (DataColumn dc in uc.Columns)
				sortKey += dc.ColumnName + ", ";
			sortKey = sortKey.Substring (0,sortKey.Length - 2);
			return sortKey;
		}
		
		private object [] GetSearchKey (DataRow dr)
		{
			if (sortedColumns == null) 
				return null;
			object [] keys = new object [sortedColumns.Length];
			int i = 0;
			foreach (SortableColumn sc in sortedColumns) {
				keys [i] = dr [sc.Column];
				i++;
			}
			return keys;
		}

		private class DataViewEnumerator : IEnumerator 
		{
			private DataRowView [] rows;
			int on = -1;

			internal DataViewEnumerator (DataRowView [] dataRowViews) 
			{
				rows = dataRowViews;
			}

			public object Current {
				get {
					if (on == -1 || on >= rows.Length)
						throw new InvalidOperationException ();
					return rows [on];
				}
			}

			public bool MoveNext () 
			{
				// TODO: how do you determine
				// if a collection has been
				// changed?
				if (on < rows.Length - 1) {
					on++;
					return true;
				}

				return false; // EOF
			}

			public void Reset () {
				on = -1;
			}
		}
		
		private class RowViewComparer : IComparer 
		{
			private SortableColumn [] sortColumns;
			private DataTable table;
			public RowViewComparer (DataTable table, SortableColumn[] sortColumns) 
			{
				this.table = table;			
				this.sortColumns = sortColumns;
			}

			public SortableColumn[] SortedColumns {
				get {
					return sortColumns;
				}
			}
			
			int IComparer.Compare (object x, object y) 
			{
				if(x == null)
					throw new SystemException ("Object to compare is null: x");
				if(y == null)
					throw new SystemException ("Object to compare is null: y");
				DataRowView rowView = (DataRowView) y;
				object [] keys = (object [])x;
				for(int i = 0; i < sortColumns.Length; i++) {
					SortableColumn sortColumn = sortColumns [i];
					DataColumn dc = sortColumn.Column;

					IComparable row = (IComparable) rowView.Row [dc];
					object key = keys [i];
					
					int result = CompareObjects (key,row);
					if (result != 0) {
						if (sortColumn.SortDirection == ListSortDirection.Ascending)
							return result;
						else 
							return -result;
					}
				}
				return 0;
			}

			private int CompareObjects (object a, object b) 
			{
				if (a == b)
					return 0;
				else if (a == null)
					return -1;
				else if (a == DBNull.Value)
					return -1;
				else if (b == null)
					return 1;
				else if (b == DBNull.Value)
					return 1;

				if((a is string) && (b is string)) {
					a = ((string) a).ToUpper (table.Locale);
					b = ((string) b).ToUpper (table.Locale);			
				}

				if (a is IComparable)
					return ((a as IComparable).CompareTo (b));
				else if (b is IComparable)
					return -((b as IComparable).CompareTo (a));

				throw new ArgumentException ("Neither a nor b IComparable");
			}
		}

	}
}
