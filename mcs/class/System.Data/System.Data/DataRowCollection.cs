//
// System.Data.DataRowCollection.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (C) Ximian, Inc 2002
//

namespace System.Data
{
	/// <summary>
	/// Collection of DataRows in a DataTable
	/// </summary>
	[Serializable]
	public class DataRowCollection : InternalDataCollectionBase {

		private ArrayList rows = null;

		// Item indexer
		[Serializable]
		public DataRow this[int index] {
			[MonoTODO]
			get {
			}
		}

		[MonoTODO]
		[Serializable]
		public void Add(DataRow row) {
			row.Add(row);
		}

		[MonoTODO]
		[Serializable]
		public virtual DataRow Add(object[] values) {
		}

		[MonoTODO]
		[Serializable]
		public void Clear() {
		}

		[MonoTODO]
		[Serializable]
		public bool Contains(object key) {
		}

		[MonoTODO]
		[Serializable]
		public DataRow Find(object key) {
		}

		[MonoTODO]
		[Serializable]
		public DataRow Find(object[] keys) {
		}

		[MonoTODO]
		[Serializable]
		public void InsertAt(DataRow row, int pos) {
		}

		[MonoTODO]
		[Serializable]
		public void Remove(DataRow row) {
		}

		[Serializable]
		public void RemoveAt(int index) {
		}

		[Serializable]
		protected override ArrayList List {
			[MonoTODO]
			get {
			}
		}
		
	}
}
