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
	public class DataView : MarshalByValueComponent, IBindingList,
		IList, ICollection, IEnumerable, ITypedList, 
		ISupportInitialize {

		public DataView() {
		}

		public DataView(DataTable table) {
		}

		public DataView(DataTable table, string RowFilter,
			string Sort, DataViewRowState RowState) {
		}

		public bool AllowDelete {
			[MonoTODO]
			get {
			}
			
			[MonoTODO]
			set {
			}
		}

		public bool AllowEdit {
			[MonoTODO]
			get {
			}
			
			[MonoTODO]
			set {
			}
		}

		public bool AllowNew {
			[MonoTODO]
			get {
			}
			
			[MonoTODO]
			set {
			}
		}

		public bool ApplyDefaultSort {
			[MonoTODO]
			get {
			}
			
			[MonoTODO]
			set {
			}
		}

		public int Count {
			[MonoTODO]
			get {
			}
		}

		public DataViewManager DataViewManager {
			[MonoTODO]
			get {
			}
		}

		// Item indexer
		public DataRowView this[int recordIndex] {
			[MonoTODO]
			get {
			}
		}

		public virtual string RowFilter {
			[MonoTODO]
			get {
			}
			
			[MonoTODO]
			set {
			}
		}

		public DataViewRowState RowStateFilter {
			[MonoTODO]
			get {
			}
			
			[MonoTODO]
			set {
			}
		}

		public string Sort {
			[MonoTODO]
			get {
			}
			
			[MonoTODO]
			set {
			}
		}

		public DataTable Table {
			[MonoTODO]
			get {
			}
			
			[MonoTODO]
			set {
			}
		}

		[MonoTODO]
		public virtual DataRowView AddNew() {
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
		}

		[MonoTODO]
		public int Find(object[] key) {
		}

		[MonoTODO]
		public DataRowView[] FindRows(object key) {
		}

		[MonoTODO]
		public DataRowView[] FindRows(object[] key) {
		}

		[MonoTODO]
		public IEnumerator GetEnumerator() {
		}
		
		[MonoTODO]
		public event ListChangedEventHandler ListChanged;

		protected bool IsOpen {
			[MonoTODO]
			get {
			}
		}

		[MonoTODO]
		protected void Close() {
		}

		[MonoTODO]
		protected virtual void ColumnCollectionChanged(
			object sender, CollectionChangeEventArgs e) {
		}

		protected override void Dispose(bool disposing) {
		}

		[MonoTODO]
		public void Dispose() {
		}

		[MonoTODO]
		protected virtual void Dispose(	bool disposing) {
		}

		[MonoTODO]
		protected virtual void IndexListChanged(object sender,
			ListChangedEventArgs e) {
		}

		[MonoTODO]
		protected virtual void OnListChanged(ListChangedEventArgs e) {
		}

		[MonoTODO]
		protected void Open() {
		}

	}

}
