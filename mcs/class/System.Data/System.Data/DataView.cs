//
// System.Data.DataView.cs
//
// Author:
//    Daniel Morgan <danmorg@sc.rr.com>
//    Tim Coleman (tim@timcoleman.com)
//
// (C) Ximian, Inc 2002
// Copyright (C) Tim Coleman, 2002
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Data {
	/// <summary>
	/// A DataView is used in the binding of data between
	/// a DataTable and Windows Forms or Web Forms allowing
	/// a view of a DataTable for editing, filtering,
	/// navigation, searching, and sorting.
	/// </summary>
	[DefaultEvent ("PositionChanged")]
	[DefaultProperty ("Table")]
	public class DataView : MarshalByValueComponent, IBindingList, IList, ICollection, IEnumerable, ITypedList, ISupportInitialize
	{

		DataTable dataTable = null;
		string rowFilter = "";
		string sort = "";
		DataViewRowState rowState;

		[MonoTODO]	
		public DataView () {
			dataTable = new DataTable ();
			rowState = DataViewRowState.None;
		}

		[MonoTODO]
		public DataView (DataTable table) {

			dataTable = table;
			rowState = DataViewRowState.None;
		}

		[MonoTODO]
		public DataView (DataTable table, string RowFilter,
			string Sort, DataViewRowState RowState) : this (table) {
			
			rowFilter = RowFilter;
			sort = Sort;
			rowState = RowState;
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether this DataView and the user interface associated with it allows deletes.")]
		[DefaultValue (true)]
		public bool AllowDelete {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether this DataView and the user interface associated with it allows edits.")]
		[DefaultValue (true)]
		public bool AllowEdit {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether this DataView and the user interface associated with it allows new rows to be added.")]
		[DefaultValue (true)]
		public bool AllowNew {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates whether to use the default sort if the Sort property is not set.")]
		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.All)]
		public bool ApplyDefaultSort {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		// get the count of rows in the DataView after RowFilter 
		// and RowStateFilter have been applied
		[Browsable (false)]
		[DataSysDescription ("Returns the number of items currently in this view.")]
		public int Count {
			[MonoTODO]
			get {
				// TODO: apply RowFilter
				// TODO: apply RowStateFilter
				return dataTable.Rows.Count;				
			}
		}

		[Browsable (false)]
		[DataSysDescription ("This returns a pointer to back to the DataViewManager that owns this DataSet (if any).")]
		public DataViewManager DataViewManager {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		// Item indexer
		public DataRowView this[int recordIndex] {
			[MonoTODO]
			get {
				return new DataRowView(this, recordIndex);
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates an expression used to filter the data returned by this DataView.")]
		[DefaultValue ("")]
		public virtual string RowFilter {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the versions of data returned by this DataView.")]
		[DefaultValue (DataViewRowState.CurrentRows)]
		public DataViewRowState RowStateFilter {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
				throw new NotImplementedException ();
			}
		}

		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the order in which data is returned by this DataView.")]
		[DefaultValue ("")]
		public string Sort {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
				throw new NotImplementedException ();
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
			}
		}

		[MonoTODO]
		public virtual DataRowView AddNew() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void BeginInit() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void CopyTo(Array array,	int index) {
			// TODO: apply RowFilter
			// TODO: apply RowStateFilter
			for (int row = 0; row < dataTable.Rows.Count; row++) {
				array.SetValue(this[row], index + row);
			}
		}

		[MonoTODO]
		public void Delete(int index) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void EndInit() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Find(object key) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public int Find(object[] key) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRowView[] FindRows(object key) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRowView[] FindRows(object[] key) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public IEnumerator GetEnumerator() {
			// TODO: apply RowFilter
			// TODO: apply RowStateFilter
			DataRowView[] dataRowViews;
			dataRowViews = new DataRowView[dataTable.Rows.Count];
			this.CopyTo (dataRowViews, 0);
			return new DataViewEnumerator (dataRowViews);
		}
		
		[MonoTODO]
		[DataCategory ("Data")]
		[DataSysDescription ("Indicates the data returned by this DataView has somehow changed.")]
		public event ListChangedEventHandler ListChanged;

		protected bool IsOpen {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected void Close() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void ColumnCollectionChanged(
			object sender, CollectionChangeEventArgs e) {

			throw new NotImplementedException ();
		}

		protected override void Dispose (bool disposing) {
			
		}

		[MonoTODO]
		protected virtual void IndexListChanged(object sender, ListChangedEventArgs e) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnListChanged(ListChangedEventArgs e) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void Open() {
			
		}
		
		[MonoTODO]
		PropertyDescriptorCollection ITypedList.GetItemProperties (
			PropertyDescriptor[] listAccessors) {

			// FIXME: use listAccessors somehow

			DataColumnPropertyDescriptor[] descriptors = 
				new DataColumnPropertyDescriptor[dataTable.Columns.Count];

			DataColumnPropertyDescriptor descriptor;
			DataColumn dataColumn;
			for(int col = 0; col < dataTable.Columns.Count; col++)
			{
				dataColumn = dataTable.Columns[col];
				
				descriptor = new DataColumnPropertyDescriptor(
					dataColumn.ColumnName, null);
				descriptor.SetComponentType(typeof(System.Data.DataRowView));
				descriptor.SetPropertyType(dataColumn.DataType);
				
				descriptors[col] = descriptor;
			}

			return new PropertyDescriptorCollection (descriptors);
		}

		[MonoTODO]
		string ITypedList.GetListName (PropertyDescriptor[] listAccessors) {
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
				throw new NotImplementedException ();
			} 
		}

		object ICollection.SyncRoot { 
			[MonoTODO]
			get {
				// FIXME:
				throw new NotImplementedException ();
			}
		}

		//void ICollection.CopyTo (Array array, int index) {
		//	CopyTo (array, index);
		//}

		bool IList.IsFixedSize {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}
		
		bool IList.IsReadOnly {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		object IList.this[int recordIndex] {
			get {
				return this[recordIndex];
			}

			[MonoTODO]
			set{
				// FIXME: throw an exception
				// because it can not be set
				throw new InvalidOperationException("Can not set Item property of a DataView");
			}
		}

		int IList.Add (object value) {
			throw new NotImplementedException ();
		}

		void IList.Clear () {
			throw new NotImplementedException ();
		}

		bool IList.Contains (object value) {
			throw new NotImplementedException ();
		}

		int IList.IndexOf (object value) {
			throw new NotImplementedException ();
		}
				
		void IList.Insert(int index,object value) {
			throw new NotImplementedException ();
		}

		void IList.Remove(object value) {
			throw new NotImplementedException ();
		}

		void IList.RemoveAt(int index) {
			throw new NotImplementedException ();
		}

		#region IBindingList implementation

		void IBindingList.AddIndex (PropertyDescriptor property) {
			throw new NotImplementedException ();
		}

		object IBindingList.AddNew () {
			throw new NotImplementedException ();
		}

		void IBindingList.ApplySort (PropertyDescriptor property, ListSortDirection direction) {
			throw new NotImplementedException ();
		}

		int IBindingList.Find (PropertyDescriptor property, object key) {
			throw new NotImplementedException ();
		}

		void IBindingList.RemoveIndex (PropertyDescriptor property) {
			throw new NotImplementedException ();
		}

		void IBindingList.RemoveSort () {
			throw new NotImplementedException ();
		}
		
		bool IBindingList.AllowEdit {
			get {
				throw new NotImplementedException ();
			}
		}

		bool IBindingList.AllowNew {
			get {
				throw new NotImplementedException ();
			}
		}

		bool IBindingList.AllowRemove {
			get {
				throw new NotImplementedException ();
			}
		}

		bool IBindingList.IsSorted {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		ListSortDirection IBindingList.SortDirection {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		PropertyDescriptor IBindingList.SortProperty {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		bool IBindingList.SupportsChangeNotification {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		bool IBindingList.SupportsSearching {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		bool IBindingList.SupportsSorting {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion // IBindingList implementation

		private class DataViewEnumerator : IEnumerator {
			private DataRowView[] rows;
			int on = -1;

			internal DataViewEnumerator (DataRowView[] dataRowViews) {
				rows = dataRowViews;
			}

			public object Current {
				get {
					if(on == rows.Length - 1)
						throw new InvalidOperationException ();
					return rows[on];
				}
			}

			public bool MoveNext() {
				// TODO: how do you determine
				// if a collection has been
				// changed?
				if(on < rows.Length - 1) {
					on++;
					return true;
				}

				return false; // EOF
			}

			public void Reset() {
				on = -1;
			}
		}
	}
}
