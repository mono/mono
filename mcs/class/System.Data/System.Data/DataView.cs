//
// System.Data.DataView.cs
//
// Author:
//    Daniel Morgan <danmorg@sc.rr.com>
//    Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Daniel Morgan, 2002, 2003
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;

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
	public class DataView : MarshalByValueComponent, IBindingList, IList, ICollection, IEnumerable, ITypedList, ISupportInitialize
	{
		DataTable dataTable = null;
		string rowFilter = "";
		string sort = "";
		DataViewRowState rowState;
		DataRowView[] rowCache = null;
		
		// FIXME: what are the default values?
		bool allowNew = true; 
		bool allowEdit = true;
		bool allowDelete = true;
		bool applyDefaultSort = false;
		bool isSorted = false;

		bool isOpen = false;

		bool bInit = false;
		
		internal DataViewManager dataViewManager = null;

		public DataView () 
		{
			dataTable = new DataTable ();
			rowState = DataViewRowState.None;
			Open ();
		}

		public DataView (DataTable table) 
		{
			dataTable = table;
			rowState = DataViewRowState.None;
			Open ();
		}

		public DataView (DataTable table, string RowFilter,
				string Sort, DataViewRowState RowState) 
		{
			dataTable = table;
			rowState = DataViewRowState.None;
			rowFilter = RowFilter;
			sort = Sort;
			rowState = RowState;
			Open();
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether this DataView and the user interface associated with it allows deletes.")]
		[DefaultValue (true)]
		public bool AllowDelete {
			[MonoTODO]
			get {
				return allowDelete;
			}
			
			[MonoTODO]
			set {
				allowDelete = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether this DataView and the user interface associated with it allows edits.")]
		[DefaultValue (true)]
		public bool AllowEdit {
			[MonoTODO]
			get {
				return allowEdit;
			}
			
			[MonoTODO]
			set {
				allowEdit = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether this DataView and the user interface associated with it allows new rows to be added.")]
		[DefaultValue (true)]
		public bool AllowNew {
			[MonoTODO]
			get {
				return allowNew;
			}
			
			[MonoTODO]
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
				applyDefaultSort = value;
				// FIXME: update the index cache to the DataTable, and 
				//        only refresh the index when the DataTable
				//        has changes via column, row, or constraint
				//        changed events
				UpdateIndex ();
			}
		}

		// get the count of rows in the DataView after RowFilter 
		// and RowStateFilter have been applied
		[Browsable (false)]
		[DataSysDescription ("Returns the number of items currently in this view.")]
		public int Count {
			[MonoTODO]
			get {
				// FIXME: remove this line once collection change
				//        events from the DataTable are handled
				UpdateIndex ();

				return rowCache.Length;;
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
				// FIXME: use index cache to the DataTable, and 
				//        only refresh the index when the DataTable
				//        has changes via column, row, or constraint
				//        changed events
				// Remove this line once changed events are handled
				UpdateIndex ();
				
				return rowCache[recordIndex];
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
				rowFilter = value;
				// FIXME: update the index cache to the DataTable, and 
				//        only refresh the index when the DataTable
				//        has changes via column, row, or constraint
				//        changed events
				UpdateIndex ();
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
				rowState = value;
				// FIXME: update the index cache to the DataTable, and 
				//        only refresh the index when the DataTable
				//        has changes via column, row, or constraint
				//        changed events
				UpdateIndex ();
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
				sort = value;
				// FIXME: update the index cache to the DataTable, and 
				//        only refresh the index when the DataTable
				//        has changes via column, row, or constraint
				//        changed events
				UpdateIndex ();
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the table this DataView uses to get data.")]
		[DefaultValue (null)]
		[RefreshProperties (RefreshProperties.All)]
		public DataTable Table {
			[MonoTODO]
			get {
				return dataTable;
			}
			
			[MonoTODO]
			set {
				dataTable = value;
				// FIXME: update the index cache to the DataTable, and 
				//        only refresh the index when the DataTable
				//        has changes via column, row, or constraint
				//        changed events
				UpdateIndex ();
			}
		}

		[MonoTODO]
		public virtual DataRowView AddNew() 
		{
			throw new NotImplementedException ();
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
			// FIXME: use index cache to the DataTable, and 
			//        only refresh the index when the DataTable
			//        has changes via column, row, or constraint
			//        changed events
			UpdateIndex ();
			
			int row = 0;
			for (; row < rowCache.Length && row < array.Length; row++) {
				array.SetValue (rowCache[row], index + row);
			}
			if (row < array.Length) {
				for (int r = 0; r < array.Length; r++) {
					array.SetValue (null, index + r);
				}
			}
		}

		[MonoTODO]
		public void Delete(int index) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EndInit() 
		{
			bInit = false;
			// FIXME:
		}

		[MonoTODO]
		public int Find(object key) 
		{
			// FIXME: use index cache to the DataTable, and 
			//        only refresh the index when the DataTable
			//        has changes via column, row, or constraint
			//        changed events

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Find(object[] key) 
		{
			// FIXME: use an index cache to the DataTable, and 
			//        only refresh the index when the DataTable
			//        has changes via column, row, or constraint
			//        changed events

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRowView[] FindRows(object key) 
		{
			// FIXME: use an index cache to the DataTable, and 
			//        only refresh the index when the DataTable
			//        has changes via column, row, or constraint
			//        changed events

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRowView[] FindRows(object[] key) 
		{
			// FIXME: use an index cache to the DataTable, and 
			//        only refresh the index when the DataTable
			//        has changes via column, row, or constraint
			//        changed events
			
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IEnumerator GetEnumerator() 
		{
			// FIXME: use an index cache to the DataTable, and 
			//        only refresh the index when the DataTable
			//        has changes via column, row, or constraint
			//        changed events
			UpdateIndex ();					

			return new DataViewEnumerator (rowCache);
		}
		
		[MonoTODO]
		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the data returned by this DataView has somehow changed.")]
		public event ListChangedEventHandler ListChanged;

		protected bool IsOpen {
			[MonoTODO]
			get {
				return isOpen;
			}
		}

		[MonoTODO]
		protected void Close() 
		{
			// FIXME:
			isOpen = false;
		}

		[MonoTODO]
		protected virtual void ColumnCollectionChanged (object sender, 
							CollectionChangeEventArgs e) 
		{
			throw new NotImplementedException ();
		}

		protected override void Dispose (bool disposing) 
		{
			if (disposing)
				Close ();

			base.Dispose (disposing);
		}

		[MonoTODO]
		protected virtual void IndexListChanged(object sender, ListChangedEventArgs e) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnListChanged(ListChangedEventArgs e) 
		{
			throw new NotImplementedException ();
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
			UpdateIndex (true);
			isOpen = true;
		}

		// internal use by Mono
		protected void Reset() 
		{
			// TODO: what really happens?
			Close ();
			rowCache = null;
			Open ();
		}

		// internal use by Mono
		protected virtual void UpdateIndex () 
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
			
			rows = dataTable.Select (RowFilter, Sort, RowStateFilter);

			newRowCache = new DataRowView[rows.Length];
			for (int r = 0; r < rows.Length; r++) {
				newRowCache[r] = new DataRowView (this, rows[r]);
			}
			rowCache = newRowCache;
		}

		[MonoTODO]
		PropertyDescriptorCollection ITypedList.GetItemProperties (PropertyDescriptor[] listAccessors) 
		{
			// FIXME: use listAccessors somehow

			DataColumnPropertyDescriptor[] descriptors = 
				new DataColumnPropertyDescriptor[dataTable.Columns.Count];

			DataColumnPropertyDescriptor descriptor;
			DataColumn dataColumn;
			for (int col = 0; col < dataTable.Columns.Count; col ++)
			{
				dataColumn = dataTable.Columns[col];
				
				descriptor = new DataColumnPropertyDescriptor(
					dataColumn.ColumnName, col, null);
				descriptor.SetComponentType (typeof (System.Data.DataRowView));
				descriptor.SetPropertyType (dataColumn.DataType);
				
				descriptors[col] = descriptor;
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
			throw new NotImplementedException ();
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

		private class DataViewEnumerator : IEnumerator 
		{
			private DataRowView[] rows;
			int on = -1;

			internal DataViewEnumerator (DataRowView[] dataRowViews) 
			{
				rows = dataRowViews;
			}

			public object Current {
				get {
					if (on == -1 || on >= rows.Length)
						throw new InvalidOperationException ();
					return rows[on];
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
	}
}

