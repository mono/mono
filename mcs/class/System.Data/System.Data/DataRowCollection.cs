//
// System.Data.DataRowCollection.cs
//
// Author:
//   Daniel Morgan <danmorg@sc.rr.com>
//
// (C) Ximian, Inc 2002
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Data
{
	/// <summary>
	/// Collection of DataRows in a DataTable
	/// </summary>
	[Serializable]
	public class DataRowCollection : InternalDataCollectionBase {

		private ArrayList rows = null;

		// Item indexer
		public DataRow this[int index] {
			[MonoTODO]
			get {
				return (DataRow) rows[index];
			}
		}

		[MonoTODO]
		public void Add (DataRow row) {
			rows.Add(row);
		}

		[MonoTODO]
		public virtual DataRow Add(object[] values) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Clear() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public bool Contains(object key) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow Find(object key) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public DataRow Find(object[] keys) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void InsertAt(DataRow row, int pos) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void Remove(DataRow row) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RemoveAt(int index) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override ArrayList List {
			[MonoTODO]
			get {
				return rows;
			}
		}		
	}
}
