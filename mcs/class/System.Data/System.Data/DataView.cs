//
// System.Data.DataView.cs
//
// Author:
//    Daniel Morgan <danmorg@sc.rr.com>
//
// (C) Ximian, Inc 2002
//

using System.Collections;
using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// A DataView is used in the binding of data between
	/// a DataTable and Windows Forms or Web Forms allowing
	/// a view of a DataTable for editing, filtering,
	/// navigation, searching, and sorting.
	/// </summary>
	public class DataView : MarshalByValueComponent, //IBindingList,
	IEnumerable, // ITypedList, IList, ICollection, 
		ISupportInitialize {

		[MonoTODO]	
		public DataView() {
		}

		[MonoTODO]
		public DataView(DataTable table) {
		}

		[MonoTODO]
		public DataView(DataTable table, string RowFilter,
			string Sort, DataViewRowState RowState) {
		}

		public bool AllowDelete {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
			}
		}

		public bool AllowEdit {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
			}
		}

		public bool AllowNew {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
			}
		}

		public bool ApplyDefaultSort {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
			}
		}

		public int Count {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

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
				throw new NotImplementedException ();
			}
		}

		public virtual string RowFilter {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
			}
		}

		public DataViewRowState RowStateFilter {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
			}
		}

		public string Sort {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
			}
		}

		public DataTable Table {
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoTODO]
			set {
			}
		}

		[MonoTODO]
		public virtual DataRowView AddNew() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void BeginInit() {
		}

		[MonoTODO]
		public void CopyTo(Array array,	int index) {
		}

		[MonoTODO]
		public void Delete(int index) {
		}

		[MonoTODO]
		public void EndInit() {
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
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public event ListChangedEventHandler ListChanged;

		protected bool IsOpen {
			[MonoTODO]
			get {
			throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected void Close() {
		}

		[MonoTODO]
		protected virtual void ColumnCollectionChanged(
			object sender, CollectionChangeEventArgs e) {
		}

		protected override void Dispose (bool disposing)
		{
		}

		[MonoTODO]
		protected virtual void IndexListChanged(object sender, ListChangedEventArgs e)
		{
		}

		[MonoTODO]
		protected virtual void OnListChanged(ListChangedEventArgs e)
		{
		}

		[MonoTODO]
		protected void Open() {
		}

	}

}
