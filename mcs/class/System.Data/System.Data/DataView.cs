//
// System.Data.DataView.cs
//
// Author:
//    Daniel Morgan <danmorg@sc.rr.com>
//    Tim Coleman (tim@timcoleman.com)
//    Punit Todi (punits_mailbox@yahoo.com)
//    Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) Daniel Morgan, 2002, 2003
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002-2003		
// (C) 2005 Novell, Inc
//

using System;
using System.Collections;
using System.Collections.Specialized;
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
		// DataRow -> DataRowView
		UnsortedList addNewCache = new UnsortedList ();
		OptionalSortedList rowViewPool = new OptionalSortedList ();

		bool allowNew = true; 
		bool allowEdit = true;
		bool allowDelete = true;
		bool applyDefaultSort = false;
		bool isSorted = false;

		bool isOpen = false;

		bool bInit = false;
		bool useDefaultSort = true;
		
		DataViewManager dataViewManager = null;

		// These fields are used to store items temporarilly 
		// during value change events.
		DataRowView changingRowView;
		int oldIndex;
		int deletedIndex;

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
			get { return applyDefaultSort; }
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
			get { return rowViewPool.Count + addNewCache.Count; }
		}

		[Browsable (false)]
		[DataSysDescription ("This returns a pointer to back to the DataViewManager that owns this DataSet (if any).")]
		public DataViewManager DataViewManager {
			get { return dataViewManager; }
		}

		// Item indexer
		// the compiler creates a DefaultMemeberAttribute from
		// this IndexerNameAttribute
		[System.Runtime.CompilerServices.IndexerName("Item")]
		public DataRowView this[int recordIndex] {
			get {
				if (recordIndex >= rowViewPool.Count + addNewCache.Count)
					throw new IndexOutOfRangeException ();

				if (recordIndex < rowViewPool.Count)
					return (DataRowView) rowViewPool.GetByIndex (recordIndex);
				else
					return (DataRowView) addNewCache.GetByIndex (recordIndex - rowViewPool.Count);
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates an expression used to filter the data returned by this DataView.")]
		[DefaultValue ("")]
		public virtual string RowFilter {
			get { return rowFilter; }
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
			get { return rowState; }
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
			get { return sort; }
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
			get { return dataTable; }
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

			// DataRowView is added, but DataRow is still Detached.
			OnListChanged (new ListChangedEventArgs (ListChangedType.ItemAdded, rowViewPool.Count + addNewCache.Count - 1, -1));
			return rowView;
		}

		[MonoTODO]
		public void BeginInit() 
		{
			bInit = true; 
			// FIXME:
		}

		public void CopyTo (Array array, int index) 
		{
			rowViewPool.CopyTo (array, index);
			addNewCache.CopyTo (array, index + rowViewPool.Count);
		}

		public void Delete(int index) 
		{
			if (!IsOpen)
				throw new DataException ("DataView is not open.");
			if (!AllowDelete)
				throw new DataException ("Cannot delete on a DataSource where AllowDelete is false.");
			
			if (index > rowViewPool.Count + addNewCache.Count)
				throw new IndexOutOfRangeException ("There is no row at " +
						"position: " + index + ".");
			DataRowView row = this [index];
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
			if (addNewCache.Contains (rowView))
				addNewCache.Remove (rowView.Row);
			else
				rowViewPool.Remove (rowView.Row);
			rowView.Row.CancelEdit ();
		}

		internal void DeleteRowView (DataRowView rowView)
		{
			if (addNewCache.Contains (rowView))
				addNewCache.Remove (rowView.Row);
			else
				rowViewPool.Remove (rowView.Row);
			rowView.Row.Delete ();
		}

		internal void EndEditRowView (DataRowView rowView)
		{
			int index = IndexOfRow (rowView.Row);
			addNewCache.Remove (rowView.Row);
			OnListChanged (new ListChangedEventArgs (ListChangedType.ItemDeleted, index, -1));

			rowView.Row.EndEdit ();
			if (rowView.Row.RowState == DataRowState.Detached)
				Table.Rows.Add (rowView.Row);
		}

		private IExpression [] PrepareExpr (object [] key)
		{
			if (Sort == String.Empty)
				throw new ArgumentException ("Find method depends on an explicit Sort property value.");
			if (sortedColumns == null)
				throw new SystemException ("sort expression result is null");
			if (sortedColumns.Length == 0)
				throw new SystemException ("sort expression result is 0");
			if (sortedColumns.Length != key.Length)
				throw new ArgumentException (String.Format ("Expecting {0} keys being indexed based on Sort property, but got {1} keys.", sortedColumns.Length, key.Length));

			IExpression [] compExpr = new IExpression [sortedColumns.Length];
			for (int i = 0; i < sortedColumns.Length; i++)
				compExpr [i] = new Comparison (Operation.EQ,
					new ColumnReference (sortedColumns [i].Column.ColumnName),
					new Literal (key [i]));
			return compExpr;
		}

		public int Find (object key) 
		{
			return Find (new object [] {key});
		}
		
		public int Find (object[] key) 
		{
			IExpression [] compExpr = PrepareExpr (key);

			// Find first match.
			int r = 0;
			IEnumerator e = GetEnumerator ();
			bool hasNext = false;
			for (; e.MoveNext (); r++) {
				if (!hasNext)
					hasNext = true;
				if (!compExpr [0].EvalBoolean (((DataRowView) e.Current).Row))
					continue;
				break;
			}
			if (!hasNext)
				return -1;
			bool finish = false;
			for (int c = 0; c < key.Length; c++)
				if (!compExpr [c].EvalBoolean (((DataRowView) e.Current).Row))
					return -1;
			return r;
		}

		public DataRowView[] FindRows (object key) 
		{
			return FindRows (new object [] {key});
		}

		public DataRowView [] FindRows (object[] key) 
		{
			IExpression [] compExpr = PrepareExpr (key);

			// Find first match.
			int r = 0;
			IEnumerator e = GetEnumerator ();
			bool hasNext = false;
			while (e.MoveNext ()) {
				if (!hasNext)
					hasNext = true;
				if (!compExpr [0].EvalBoolean (((DataRowView) e.Current).Row))
					continue;
				break;
			}
			ArrayList al = new ArrayList ();
			// Find first no-match from here.
			do {
				if (!hasNext)
					break;
				bool finish = false;
				for (int c = 0; c < key.Length; c++) {
					if (!compExpr [c].EvalBoolean (((DataRowView) e.Current).Row)) {
						finish = true;
						break;
					}
				}
				if (finish)
					break;
				al.Add (e.Current);
			} while (e.MoveNext ());

			return (DataRowView []) al.ToArray (typeof (DataRowView));
		}

		public IEnumerator GetEnumerator() 
		{
			ArrayList al = new ArrayList (rowViewPool.Count + addNewCache.Count);
			al.AddRange (rowViewPool.Values);
			al.AddRange (addNewCache.Values);
			return new DataViewEnumerator ((DataRowView [])
				al.ToArray (typeof (DataRowView)));
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

		protected void Close ()
		{
			if (dataTable != null)
				UnregisterEventHandlers ();
			rowViewPool.Clear ();
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
			if (dataTable != null) {
				RegisterEventHandlers();
				UpdateIndex (true);
			}
			isOpen = true;
		}
		
		private void RegisterEventHandlers()
		{
			dataTable.ColumnChanging += new DataColumnChangeEventHandler(OnColumnChanging);
			dataTable.ColumnChanged  += new DataColumnChangeEventHandler(OnColumnChanged);
			dataTable.RowChanged     += new DataRowChangeEventHandler(OnRowChanged);
			dataTable.RowDeleting    += new DataRowChangeEventHandler(OnRowDeleting);
			dataTable.RowDeleted     += new DataRowChangeEventHandler(OnRowDeleted);
			dataTable.Columns.CollectionChanged += new CollectionChangeEventHandler(OnColumnCollectionChanged);
			dataTable.Constraints.CollectionChanged += new CollectionChangeEventHandler(OnConstraintCollectionChanged);
		}

		private void UnregisterEventHandlers()
		{
			dataTable.ColumnChanging -= new DataColumnChangeEventHandler(OnColumnChanging);
			dataTable.ColumnChanged  -= new DataColumnChangeEventHandler(OnColumnChanged);
			dataTable.RowChanged     -= new DataRowChangeEventHandler(OnRowChanged);
			dataTable.RowDeleting    -= new DataRowChangeEventHandler(OnRowDeleting);
			dataTable.RowDeleted     -= new DataRowChangeEventHandler(OnRowDeleted);
			dataTable.Columns.CollectionChanged -= new CollectionChangeEventHandler(OnColumnCollectionChanged);
			dataTable.Constraints.CollectionChanged -= new CollectionChangeEventHandler(OnConstraintCollectionChanged);
		}

		// These index storing and rowView preservation must be done
		// before the actual row value is changed; thus we can't use
		// RowChanging which accepts "already modified" DataRow.

		private void OnColumnChanging (object sender, DataColumnChangeEventArgs e)
		{
			changingRowView = (DataRowView) rowViewPool [e.Row];
			if (changingRowView != null) {
				oldIndex = rowViewPool.IndexOfKey (changingRowView.Row);
				rowViewPool.Remove (changingRowView.Row);
			}
			else
				oldIndex = int.MinValue;
		}

		private void OnColumnChanged (object sender, DataColumnChangeEventArgs e)
		{
			if (changingRowView != null)
				rowViewPool.Add (changingRowView.Row, changingRowView);
			changingRowView = null;
		}

		private void OnRowChanged (object sender, DataRowChangeEventArgs args)
		{
			if (args.Row == null)
				throw new SystemException ("Should not happen. Row is not supplied.");

			int newIndex;

			/* ItemAdded */
			if(args.Action == DataRowAction.Add)
			{
				if (rowFilterExpr != null &&
					!rowFilterExpr.EvalBoolean (args.Row))
					return; // do nothing.
				rowViewPool.Add (args.Row, new DataRowView (this, args.Row, false));
				newIndex = IndexOfRow (args.Row);
				OnListChanged (new ListChangedEventArgs (ListChangedType.ItemAdded, newIndex, -1));
			}
				
			/* ItemChanged or ItemMoved */
			if (args.Action == DataRowAction.Change) {
				DataRowView drv = (DataRowView) rowViewPool [args.Row];
				if (rowFilterExpr != null &&
					!rowFilterExpr.EvalBoolean (args.Row)) {
					// RowView disappearing from this view.
					if (drv != null) {
						oldIndex = IndexOfRowView (drv);
						rowViewPool.Remove (args.Row);
						OnListChanged (new ListChangedEventArgs (ListChangedType.ItemMoved, int.MinValue, oldIndex));
					}
					return;
				}
				else if (drv == null) {
					// new RowView showing up in this view.
					drv = new DataRowView (this, args.Row, false);
					rowViewPool.Add (args.Row, drv);
					newIndex = IndexOfRowView (drv);
					OnListChanged (new ListChangedEventArgs (ListChangedType.ItemMoved, newIndex, int.MinValue));
					return;
				}
				else {
					newIndex = IndexOfRow (args.Row);
					if (oldIndex == newIndex)
						OnListChanged (new ListChangedEventArgs (ListChangedType.ItemChanged, newIndex, -1));
					else
						OnListChanged (new ListChangedEventArgs (ListChangedType.ItemMoved, newIndex, oldIndex));
				}
			}
		}

		private void OnRowDeleting (object sender,
			DataRowChangeEventArgs args)
		{
			deletedIndex = IndexOfRow (args.Row);
		}

		private void OnRowDeleted (object sender, DataRowChangeEventArgs args)
		{
			OnListChanged (new ListChangedEventArgs (ListChangedType.ItemDeleted, deletedIndex, -1));
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
			if (IsOpen)
				Close ();
			UpdateIndex (true);
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
		protected virtual void UpdateIndex (bool force) 
		{
			DataRow[] rows = null;

			// I guess, "force" parameter is used to indicate
			// whether we should "query" against DataTable.
			// For example, when adding a new row, we don't have
			// to re-query.

			// Handle sort by itself, considering AddNew rows.
			rows = dataTable.Select (rowFilterExpr, null, RowStateFilter);

			OptionalSortedList newPool = null;
			if (sortedColumns != null)
				newPool = new OptionalSortedList (
					dataTable,
					sortedColumns,
					rows.Length + 3);
			else
				newPool = new OptionalSortedList (rows.Length + 3);

			for (int r = 0; r < rows.Length; r++) {
				DataRow dr = rows [r];
				DataRowView rv = (DataRowView) rowViewPool [dr];
				if (rv == null)
					rv = new DataRowView (this, dr);
				newPool.Add (dr, rv);
			}

			rowViewPool = newPool;
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
			return rowViewPool.Contains (drv) || addNewCache.Contains (drv);
		}

		int IList.IndexOf (object value) 
		{
			DataRowView drv = value as DataRowView;
			if (drv == null)
				return -1;
			return IndexOfRowView (drv);
		}
			
		void IList.Insert (int index,object value) 
		{
			throw new ArgumentException ("Cannot insert external objects to this list.");
		}

		void IList.Remove (object value) 
		{
			// LAMESPEC: MS.NET's behavior is weird. It raises
			// events and*then* raises this exception.
			throw new ArgumentException ("Cannot remove from this list.");
		}

		void IList.RemoveAt (int index) 
		{
			DataRowView drv = this [index]; // might raise OutOfRangeException here.
			if (drv == null)
				throw new ArgumentException ("Cannot remove from this list.");
			drv.Delete ();
		}

		#region IBindingList implementation

		[MonoTODO]
		void IBindingList.AddIndex (PropertyDescriptor property) 
		{
			throw new NotImplementedException ();
		}

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
		private int IndexOfRowView (DataRowView drv)
		{
			int i = rowViewPool.IndexOfValue (drv);
			if (i >= 0)
				return i;
			if (addNewCache.Count == 0)
				return -1;
			IEnumerator e = addNewCache.Values.GetEnumerator ();
			for (i = 0; e.MoveNext (); i++)
				if (((DataRowView) e.Current) == drv)
					return rowViewPool.Count + i;
			return -1;
		}

		private int IndexOfRow (DataRow dr)
		{
			int i = rowViewPool.IndexOfKey (dr);
			if (i >= 0)
				return i;
			if (addNewCache.Count == 0)
				return -1;
			IEnumerator e = addNewCache.Keys.GetEnumerator ();
			for (i = 0; e.MoveNext (); i++)
				if (((DataRow) e.Current) == dr)
					return rowViewPool.Count + i;
			throw new SystemException ("Should not happen.");
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
				// It does not care about collection being changed.
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

		private class RowViewFindComparer : IComparer 
		{
			private SortableColumn [] sortColumns;
			private DataTable table;
			public RowViewFindComparer (DataTable table, SortableColumn[] sortColumns) 
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

	internal class UnsortedList : IDictionary
	{
		Hashtable items;
		ArrayList orders;

		public UnsortedList ()
		{
			items = new Hashtable ();
			orders = new ArrayList ();
		}

		public UnsortedList (int size)
		{
			items = new Hashtable (size);
			orders = new ArrayList (size);
		}

		public void Add (object key, object value)
		{
			orders.Add (key);
			items.Add (key, value);
		}

		public bool Contains (object key)
		{
			return orders.Contains (key);
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			return new UnsortedListDictionaryEnumerator (
				orders, items);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new UnsortedListDictionaryEnumerator (
				orders, items);
		}

		public object GetByIndex (int i)
		{
			return items [orders [i]];
		}

		public int IndexOfKey (object key)
		{
			for (int i = orders.Count - 1; i >= 0; i--)
				if (orders [i] == key)
					return i;
			return -1;
		}

		public int IndexOfValue (object value)
		{
			for (int i = orders.Count - 1; i >= 0; i--)
				if (items [orders [i]] == value)
					return i;
			return -1;
		}

		public object this [object key] {
			get { return items [key]; }
			set { items [key] = value; }
		}

		public bool IsFixedSize {
			get { return false; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public int Count {
			get { return orders.Count; }
		}

		public object SyncRoot {
			get { return this; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public ICollection Keys {
			get { return orders; }
		}

		public ICollection Values {
			get {
				
				return SortedValues;
			}
		}

		public Array SortedValues {
			get {
				object [] results = new object [items.Count];
				for (int i = 0; i < orders.Count; i++)
					results [i] = items [orders [i]];
				return results;
			}
		}

		public void CopyTo (Array array, int index)
		{
			SortedValues.CopyTo (array, index);
		}

		public void Clear ()
		{
			orders.Clear ();
			items.Clear ();
		}

		public void Remove (object o)
		{
			orders.Remove (o);
			items.Remove (o);
		}

		internal class UnsortedListDictionaryEnumerator
			: IEnumerator, IDictionaryEnumerator
		{
			ArrayList orders;
			Hashtable items;
			int index = -1;
			DictionaryEntry current;

			public UnsortedListDictionaryEnumerator (
				ArrayList orders, Hashtable items)
			{
				this.orders = orders;
				this.items = items;
			}

			public bool MoveNext ()
			{
				if (index + 1 == orders.Count)
					return false;
				index++;
				object key = orders [index];
				current = new DictionaryEntry (key, items [key]);
				return true;
			}

			public DictionaryEntry Entry {
				get { return current; }
			}

			public object Current {
				get { return index < 0 ? (object) null : current; }
			}

			public object Key {
				get { return index < 0 ? (object) null : current.Key; }
			}

			public object Value {
				get { return index < 0 ? (object) null : current.Value; }
			}

			public void Reset ()
			{
				index = -1;
			}
		}
	}

	// Since IndexOf() is not always working fine, we cannot make
	// full use of SortedList (because RowSorter depends on "current"
	// value of the value.
	internal class OptionalSortedList : IDictionary
	{
		SortedList sorted;
		UnsortedList unsorted;

		public OptionalSortedList ()
		{
			unsorted = new UnsortedList ();
		}

		public OptionalSortedList (int size)
		{
			unsorted = new UnsortedList (size);
		}

		public OptionalSortedList (DataTable table,
			SortableColumn [] columns, int initSize)
		{
			sorted = new SortedList (new DataTable.RowSorter (
				table, columns), initSize);
		}

		IDictionary Instance {
			get { return sorted != null ? (IDictionary) sorted : unsorted; }
		}

		public void Add (object key, object value)
		{
			Instance.Add (key, value);
		}

		public bool Contains (object key)
		{
			return Instance.Contains (key);
		}

		public IDictionaryEnumerator GetEnumerator ()
		{
			return Instance.GetEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return Instance.GetEnumerator ();
		}

		public object GetByIndex (int i)
		{
			if (sorted != null)
				return sorted.GetByIndex (i);
			else
				return unsorted.GetByIndex (i);
		}

		public int IndexOfKey (object key)
		{
			if (sorted != null)
				return sorted.IndexOfKey (key);
			else
				return unsorted.IndexOfKey (key);
		}

		public int IndexOfValue (object value)
		{
			if (sorted != null)
				return sorted.IndexOfValue (value);
			else
				return unsorted.IndexOfValue (value);
		}

		public object this [object key] {
			get { return Instance [key]; }
			set { Instance [key] = value; }
		}

		public bool IsFixedSize {
			get { return Instance.IsFixedSize; }
		}

		public bool IsReadOnly {
			get { return Instance.IsReadOnly; }
		}

		public int Count {
			get { return Instance.Count; }
		}

		public object SyncRoot {
			get { return Instance.SyncRoot; }
		}

		public bool IsSynchronized {
			get { return Instance.IsSynchronized; }
		}

		public ICollection Keys {
			get { return Instance.Keys; }
		}

		public ICollection Values {
			get { return Instance.Values; }
		}

		public void CopyTo (Array array, int index)
		{
			Instance.CopyTo (array, index);
		}

		public void Clear ()
		{
			Instance.Clear ();
		}

		public void Remove (object o)
		{
			Instance.Remove (o);
		}
	}
}
